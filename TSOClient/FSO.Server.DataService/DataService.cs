﻿using FSO.Common.DataService.Framework;
using FSO.Common.DataService.Framework.Attributes;
using FSO.Common.Security;
using FSO.Common.Serialization;
using FSO.Common.Serialization.Primitives;
using FSO.Files.Formats.tsodata;
using FSO.Server.DataService.Model;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;

namespace FSO.Common.DataService
{
    public class DataService : IDataService
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();

        private Dictionary<uint, IDataServiceProvider> ProviderByTypeId = new Dictionary<uint, IDataServiceProvider>();
        private Dictionary<Type, IDataServiceProvider> ProviderByType = new Dictionary<Type, IDataServiceProvider>();
        private Dictionary<MaskedStruct, IDataServiceProvider> ProviderByDerivedStruct = new Dictionary<MaskedStruct, IDataServiceProvider>();
        private Dictionary<MaskedStruct, StructField[]> MaskedStructToActualFields = new Dictionary<MaskedStruct, StructField[]>();
        private Dictionary<uint, StructField[]> StructToActualFields = new Dictionary<uint, StructField[]>();
        private Dictionary<uint, Type> ModelTypeById = new Dictionary<uint, Type>();
        private Dictionary<Type, uint> ModelIdByType = new Dictionary<Type, uint>();

        private IModelSerializer Serializer;
        private TSODataDefinition DataDefinition;

        public DataService(IModelSerializer serializer, FSO.Content.Content content){
            this.Serializer = serializer;
            this.DataDefinition = content.DataDefinition;

            //Build Struct => Field[] maps for quicker serialization
            foreach (var derived in DataDefinition.DerivedStructs)
            {
                var type = MaskedStructUtils.FromID(derived.ID);
                List<StructField> fields = new List<StructField>();
                var parent = DataDefinition.Structs.First(x => x.ID == derived.Parent);

                foreach (var field in parent.Fields)
                {
                    var mask = derived.FieldMasks.FirstOrDefault(x => x.ID == field.ID);
                    if (mask == null) { continue; }
                    /*
                    var action = DerivedStructFieldMaskType.KEEP;
                    if (mask != null){
                        action = mask.Type;
                    }
                    if (action == DerivedStructFieldMaskType.REMOVE){
                        //These seems wrong, ServerMyAvatar and MyAvatar both exclude bookmarks by this logic
                        //continue;
                    }
                    */
                    fields.Add(field);
                }
                MaskedStructToActualFields.Add(type, fields.ToArray());
            }

            foreach(var _struct in DataDefinition.Structs){
                StructToActualFields.Add(_struct.ID, _struct.Fields.ToArray());
            }

            var assembly = Assembly.GetAssembly(typeof(DataService));

            foreach (Type type in assembly.GetTypes()){
                System.Attribute[] attributes = System.Attribute.GetCustomAttributes(type);

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is DataServiceModel)
                    {
                        var _struct = DataDefinition.GetStruct(type.Name);
                        if(_struct != null){
                            ModelTypeById.Add(_struct.ID, type);
                            ModelIdByType.Add(type, _struct.ID);
                        }
                    }
                }
            }
        }

        public Task<T> Get<T>(object key){
            return Get(typeof(T), key).ContinueWith<T>(x => (T)x.Result);
        }


        public Task<T[]> GetMany<T>(object[] keys){
            return GetMany(typeof(T), keys).ContinueWith<T[]>(x =>
            {
                if (x.IsFaulted) { throw x.Exception; }
                var result = new List<T>();
                foreach(var item in x.Result){
                    result.Add((T)item);
                }
                return result.ToArray();
            });
        }

        public Task<object[]> GetMany(Type type, object[] keys){
            var provider = ProviderByType[type];
            return GetMany(provider, keys);
        }

        private Task<object[]> GetMany(IDataServiceProvider provider, object[] keys){
            Task<object>[] tasks = new Task<object>[keys.Length];
            for(int i=0; i < keys.Length; i++){
                tasks[i] = Get(provider, keys[i]);
            }

            return Task.WhenAll<object>(tasks);
        }

        public Task<object> Get(Type type, object key){
            var provider = ProviderByType[type];
            return Get(provider, key);
        }

        public Task<object> Get(uint type, object key){
            var provider = ProviderByTypeId[type];
            return Get(provider, key);
        }


        public IDataServiceProvider GetProvider(uint type)
        {
            return ProviderByTypeId[type];
        }

        public Task<object> Get(MaskedStruct type, object key)
        {
            if (!ProviderByDerivedStruct.ContainsKey(type))
            {
                return null;
            }
            var provider = ProviderByDerivedStruct[type];
            return Get(provider, key);
        }

        protected virtual Task<object> Get(IDataServiceProvider provider, object key){
            return provider.Get(key);
        }

        public void Invalidate<T>(object key)
        {
            var provider = ProviderByType[typeof(T)];
            provider.Invalidate(key);
        }

        public void Invalidate<T>(object key, T replacement)
        {
            var provider = ProviderByType[typeof(T)];
            provider.Invalidate(key, replacement);
        }

        public List<cTSOTopicUpdateMessage> SerializeUpdate(MaskedStruct mask, object value, uint id){
            return SerializeUpdateFields(MaskedStructToActualFields[mask], value, id);
        }

        public List<cTSOTopicUpdateMessage> SerializeUpdate(StructField[] fields, object value, uint id)
        {
            return SerializeUpdateFields(fields, value, id);
        }

        public cTSOTopicUpdateMessage SerializeUpdate(object value, params uint[] dotPath)
        {
            return SerializeUpdateField(value, dotPath);
        }

        public StructField GetFieldByName(Type type, string field)
        {
            if (ModelIdByType.ContainsKey(type))
            {
                var modelId = ModelIdByType[type];
                var structs = StructToActualFields[modelId];
                return structs.Where(x => field == x.Name).FirstOrDefault();
            }
            return null;
        }

        public StructField[] GetFieldsByName(Type type, params string[] fields)
        {
            if (ModelIdByType.ContainsKey(type))
            {
                var modelId = ModelIdByType[type];
                return StructToActualFields[modelId].Where(x => fields.Contains(x.Name)).ToArray();
            }
            return new StructField[]{};
        }

        public async Task<cTSOTopicUpdateMessage> SerializePath(params uint[] dotPath)
        {
            var path = await ResolveDotPath(dotPath);
            var value = path.GetValue();
            
            return SerializeUpdateField(value.Value, dotPath);
        }

        public async void ApplyUpdate(cTSOTopicUpdateMessage update, ISecurityContext context)
        {
            try
            {
                var partialDotPath = new uint[update.DotPath.Length - 1];
                Array.Copy(update.DotPath, partialDotPath, partialDotPath.Length);
                var path = await ResolveDotPath(partialDotPath);

                var target = path.GetValue();
                if (target.Value == null) { throw new Exception("Cannot set property on null value"); }

                //Apply the change!
                var targetType = target.Value.GetType();
                var finalPath = update.DotPath[update.DotPath.Length - 1];
                var value = GetUpdateValue(update.Value);

                var provider = GetProvider(path.GetProvider());
                var entity = path.GetEntity();

                if (IsList(targetType))
                {
                    //Array, we expect the final path component to be an array index
                    var arr = (IList)target.Value;
                    var parent = path.GetParent();
                    var objectField = parent.Value.GetType().GetProperty(target.Name);
                    if (finalPath < arr.Count)
                    {
                        //Update existing or remove on null (at index)
                        if (IsNull(value))
                        {
                            var removeItem = ((IList)target.Value)[(int)finalPath];
                            provider.DemandMutation(entity.Value, MutationType.ARRAY_REMOVE_ITEM, path.GetKeyPath(), removeItem, context);
                            objectField.SetValue(parent.Value, GetGenericMethod(targetType, "RemoveAt").Invoke(target.Value, new object[] { (int)finalPath }));

                            //TODO: make this async?
                            if (target.Persist)
                            {
                                provider.PersistMutation(entity.Value, MutationType.ARRAY_REMOVE_ITEM, path.GetKeyPath(), removeItem);
                            }
                        }
                        else
                        {
                            provider.DemandMutation(entity.Value, MutationType.ARRAY_SET_ITEM, path.GetKeyPath(), value, context);

                            objectField.SetValue(parent.Value, GetGenericMethod(targetType, "SetItem").Invoke(target.Value, new object[] { (int)finalPath, value }));
                            //arr[(int)finalPath] = value;

                            //TODO: make this async?
                            if (target.Persist)
                            {
                                provider.PersistMutation(entity.Value, MutationType.ARRAY_SET_ITEM, path.GetKeyPath(), value);
                            }
                        }
                    }
                    else if (finalPath >= arr.Count)
                    {
                        //Insert
                        provider.DemandMutation(entity.Value, MutationType.ARRAY_SET_ITEM, path.GetKeyPath(), value, context);

                        objectField.SetValue(parent.Value, GetGenericMethod(targetType, "Add").Invoke(target.Value, new object[] { value }));
                        //arr.Add(value);

                        if (target.Persist)
                        {
                            provider.PersistMutation(entity.Value, MutationType.ARRAY_SET_ITEM, path.GetKeyPath(), value);
                        }
                    }
                }
                else
                {
                    //We expect a field value
                    if (target.TypeId == 0)
                    {
                        throw new Exception("Trying to set field on unknown type");
                    }

                    var _struct = DataDefinition.GetStruct(target.TypeId);
                    var field = _struct.Fields.FirstOrDefault(x => x.ID == finalPath);
                    if (field == null) { throw new Exception("Unknown field in dot path"); }

                    var objectField = target.Value.GetType().GetProperty(field.Name);
                    if (objectField == null) { throw new Exception("Unknown field in model: " + objectField.Name); }

                    //If the value is null (0) and the field has a decoration of NullValueIndicatesDeletion
                    //Delete the value instead of setting it
                    var nullDelete = objectField.GetCustomAttribute<Key>();
                    if (nullDelete != null && IsNull(value))
                    {
                        var parent = path.GetParent();
                        if (IsList(parent.Value))
                        {
                            var listParent = path.Path[path.Path.Length - 3];
                            var lpField = listParent.Value.GetType().GetProperty(parent.Name);

                            provider.DemandMutation(entity.Value, MutationType.ARRAY_REMOVE_ITEM, path.GetKeyPath(1), target.Value, context);
                            lpField.SetValue(listParent.Value, GetGenericMethod(parent.Value.GetType(), "Remove", new Type[] { parent.Value.GetType().GenericTypeArguments[0] })
                                .Invoke(parent.Value, new object[] { target.Value }));
                            //((IList)parent.Value).Remove(target.Value);

                            if (parent.Persist)
                            {
                                provider.PersistMutation(entity.Value, MutationType.ARRAY_REMOVE_ITEM, path.GetKeyPath(1), target.Value);
                            }
                        }
                        else
                        {
                            //TODO
                        }
                    }
                    else
                    {
                        var persist = objectField.GetCustomAttribute<Persist>();
                        var clientSourced = (target.Value as AbstractModel)?.ClientSourced ?? false;
                        //if the client is internally managing this value, do not update it.
                        if (!clientSourced || objectField.GetCustomAttribute<ClientSourced>() == null)
                        {
                            provider.DemandMutation(entity.Value, MutationType.SET_FIELD_VALUE, path.GetKeyPath(objectField.Name), value, context);

                            var lastValue = objectField.GetValue(target.Value);
                            //only set a value if it changed. lessens performance impact for polling
                            if (!(lastValue?.Equals(value) ?? (value == null)))
                                objectField.SetValue(target.Value, value);

                            if (persist != null)
                            {
                                provider.PersistMutation(entity.Value, MutationType.SET_FIELD_VALUE, path.GetKeyPath(objectField.Name), value);
                            }
                        }
                    }
                }
            } catch (Exception e)
            {
                
                if (e is SecurityException)
                {
                    LOG.Error("Unauthorised data service update:" + e.Message);
                } else
                {
                    LOG.Error(e, "Data service update failed.");
                }
                
            }
        }

        private uint? GetStructType(object value)
        {
            if(value != null)
            {
                if (ModelIdByType.ContainsKey(value.GetType()))
                {
                    return ModelIdByType[value.GetType()];
                }
                return null;
            }
            return null;
        }

        private async Task<DotPathResult> ResolveDotPath(params uint[] _path)
        {
            var result = new DotPathResult();
            result.Path = new DotPathResultComponent[_path.Length];

            Queue<uint> path = new Queue<uint>(_path);

            var typeId = path.Dequeue();
            var entityId = path.Dequeue();
            var obj = await Get(typeId, entityId);
            if (obj == null) { throw new Exception("Unknown object in dot path"); }

            result.Path[0] = new DotPathResultComponent {
                Value = null,
                Id = typeId,
                Type = DotPathResultComponentType.PROVIDER,
                Name = null
            };
            result.Path[1] = new DotPathResultComponent{
                Value = obj,
                Id = entityId,
                Type = DotPathResultComponentType.ARRAY_ITEM,
                TypeId = typeId,
                Name = entityId.ToString()
            };

            var _struct = DataDefinition.GetStructFromValue(obj);
            if (_struct == null) { throw new Exception("Unknown struct in dot path"); }
            var index = 2;

            while (path.Count > 0)
            {
                var nextField = path.Dequeue();
                var field = _struct.Fields.FirstOrDefault(x => x.ID == nextField);
                if (field == null) { throw new Exception("Unknown field in dot path"); }

                var objectField = obj.GetType().GetProperty(field.Name);
                if (objectField == null) { throw new Exception("Unknown field " + field.Name); }
                obj = objectField.GetValue(obj);
                if (obj == null) { throw new Exception("Member not found, unable to apply update"); }
                _struct = DataDefinition.GetStructFromValue(obj);

                result.Path[index++] = new DotPathResultComponent {
                    Value = obj,
                    Id = field.ID,
                    TypeId = _struct != null ? _struct.ID : 0,
                    Type = DotPathResultComponentType.FIELD,
                    Name = field.Name,
                    Persist = objectField.GetCustomAttribute<Persist>() != null
                };


                if (field.Classification == StructFieldClassification.List)
                {
                    //Array index comes next
                    if (path.Count > 0)
                    {
                        var arr = (IList)obj;
                        var arrIndex = path.Dequeue();
                        if (arrIndex >= arr.Count)
                        {
                            if (arr.Count == 0) throw new Exception("Item at index not found, unable to apply update");
                            arrIndex = (uint)(arr.Count - 1);
                        }

                        if (arrIndex < arr.Count)
                        {
                            obj = arr[(int)arrIndex];
                            if (obj == null) { throw new Exception("Item at index not found, unable to apply update"); }
                            _struct = DataDefinition.GetStructFromValue(obj);

                            result.Path[index++] = new DotPathResultComponent
                            {
                                Value = obj,
                                Id = arrIndex,
                                Type = DotPathResultComponentType.ARRAY_ITEM,
                                TypeId = _struct != null ? _struct.ID : 0,
                                Name = arrIndex.ToString()
                            };

                        }
                        else
                        {
                            throw new Exception("Item at index not found, unable to apply update");
                        }
                    }
                }
            }
            return result;
        }

        private bool IsList(object value)
        {
            if (value == null) { return false; }
            return IsList(value.GetType());
        }

        private bool IsList(Type targetType)
        {
            return targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(ImmutableList<>);
        }

        private MethodInfo GetGenericMethod(Type targetType, string name, Type[] args)
        {
            return targetType.GetMethod(name, args);
        }

        private MethodInfo GetGenericMethod(Type targetType, string name)
        {
            return targetType.GetMethod(name);
        }

        private bool IsNull(object value)
        {
            if (value == null) { return true; }
            if(value is uint)
            {
                return ((uint)value) == 0;
            }else if (value is int)
            {
                return ((int)value) == 0;
            }else if (value is ushort)
            {
                return ((ushort)value) == 0;
            }
            else if (value is short)
            {
                return ((short)value) == 0;
            }
            return false;
        }

        private object GetUpdateValue(object value)
        {
            if(value is cTSOProperty){
                //Convert to model
                return ConvertProperty(value as cTSOProperty);
            }
            return value;
        }

        public object ConvertProperty(cTSOProperty property)
        {
            var _struct = DataDefinition.GetStruct(property.StructType);
            if (_struct == null) { return null; }

            if (!ModelTypeById.ContainsKey(_struct.ID)){
                return null;
            }

            var type = ModelTypeById[_struct.ID];
            var instance = ModelActivator.NewInstance(type);
            
            foreach(var field in property.StructFields){
                var _field = _struct.Fields.FirstOrDefault(x => x.ID == field.StructFieldID);
                if (_field == null) { continue; }
                
                SetFieldValue(instance, _field.Name, GetUpdateValue(field.Value));
            }

            return instance;
        }

        private List<cTSOTopicUpdateMessage> SerializeUpdateFields(StructField[] fields, object instance, uint id)
        {
            var result = new List<cTSOTopicUpdateMessage>();
            foreach (var field in fields)
            {
                object value = GetFieldValue(instance, field.Name);
                if (value == null) { continue; }
                
                //Might be a struct
                try
                {
                    result.Add(SerializeUpdateField(value, new uint[] {
                        field.ParentID,
                        id,
                        field.ID
                    }));
                }
                catch(Exception ex){
                }
            }
            return result;
        }

        private cTSOTopicUpdateMessage SerializeUpdateField(object value, uint[] path)
        {
            try
            {
                var clsid = Serializer.GetClsid(value);
                if (!clsid.HasValue)
                {
                    throw new Exception("Unable to serialize value with type: " + value.GetType());
                }

                var update = new cTSOTopicUpdateMessage();
                update.DotPath = path;
                update.Value = value;
                return update;
            }
            catch (Exception ex)
            {
                LOG.Error(ex);
                throw ex;
            }
        }

        private object GetFieldValue(object obj, string fieldName)
        {
            if (obj == null) { return null; }

            var objectField = obj.GetType().GetProperty(fieldName);
            if (objectField == null) { return null; }

            var value = objectField.GetValue(obj);

            return value;
        }

        private void SetFieldValue(object obj, string fieldName, object value)
        {
            var objectField = obj.GetType().GetProperty(fieldName);
            if (objectField == null) { return; }

            objectField.SetValue(obj, value);
        }

        public void AddProvider(IDataServiceProvider provider){
            provider.Init();

            var type = provider.GetValueType();
            var structDef = DataDefinition.Structs.First(x => x.Name == type.Name);

            ProviderByTypeId.Add(structDef.ID, provider);
            ProviderByType.Add(type, provider);

            var derived = DataDefinition.DerivedStructs.Where(x => x.Parent == structDef.ID);
            foreach(var item in derived){
                ProviderByDerivedStruct.Add(MaskedStructUtils.FromID(item.ID), provider);
            }
        }
        
    }
}
