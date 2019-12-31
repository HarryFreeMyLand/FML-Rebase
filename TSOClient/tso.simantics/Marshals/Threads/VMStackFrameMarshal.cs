﻿using FSO.SimAntics.NetPlay.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FSO.SimAntics.Marshals.Threads
{
    public class VMStackFrameMarshal : VMSerializable
    {
        public ushort RoutineID;
        public ushort InstructionPointer;
        public short Caller;
        public short Callee;
        public short StackObject;
        public uint CodeOwnerGUID;
        public short[] Locals;
        public short[] Args;
        public bool DiscardResult;
        public bool ActionTree;

        public int Version;

        public VMStackFrameMarshal() { }
        public VMStackFrameMarshal(int version) { Version = version; }

        public virtual void Deserialize(BinaryReader reader)
        {
            RoutineID = reader.ReadUInt16();
            InstructionPointer = reader.ReadUInt16();
            Caller = reader.ReadInt16();
            Callee = reader.ReadInt16();
            StackObject = reader.ReadInt16();
            CodeOwnerGUID = reader.ReadUInt32();

            var localN = reader.ReadInt32();
            if (localN > -1)
            {
                Locals = new short[localN];
                for (int i = 0; i < localN; i++) Locals[i] = reader.ReadInt16();
            }

            var argsN = reader.ReadInt32();
            if (argsN > -1)
            {
                Args = new short[argsN];
                for (int i = 0; i < argsN; i++) Args[i] = reader.ReadInt16();
            }

            if (Version > 3) DiscardResult = reader.ReadBoolean();
            ActionTree = reader.ReadBoolean();
        }

        public virtual void SerializeInto(BinaryWriter writer)
        {
            writer.Write(RoutineID);
            writer.Write(InstructionPointer);
            writer.Write(Caller);
            writer.Write(Callee);
            writer.Write(StackObject);
            writer.Write(CodeOwnerGUID);
            writer.Write((Locals == null)?-1:Locals.Length);
            if (Locals != null) writer.Write(VMSerializableUtils.ToByteArray(Locals));
            writer.Write((Args == null) ? -1 : Args.Length);
            if (Args != null) writer.Write(VMSerializableUtils.ToByteArray(Args));
            writer.Write(DiscardResult);
            writer.Write(ActionTree);
        }
    }
}
