﻿using FSO.Common;
using FSO.Files.Formats.IFF;
using FSO.Files.Formats.IFF.Chunks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace FSO.Content
{
    public class ChangeManager
    {
        HashSet<IffFile> ChangedFiles = new HashSet<IffFile>();
        private Queue<ResAction> ResActionQueue = new Queue<ResAction>();
        private Queue<ResInvokeElem> InvokeQueue = new Queue<ResInvokeElem>();

        public void Invoke(Delegate func, params object[] args)
        {
            ResInvokeElem inv;
            lock (InvokeQueue)
            {
                inv = new ResInvokeElem(func, args);
                InvokeQueue.Enqueue(inv);
            }
            inv.Sync.WaitOne();
        }

        public void IffChanged(IffFile file)
        {
            file.RuntimeInfo.Dirty = true;
            lock (this) ChangedFiles.Add(file);
        }

        public void ChunkChanged(IffChunk chunk)
        {
            chunk.RuntimeInfo = ChunkRuntimeState.Modified;
            chunk.ChunkParent.RuntimeInfo.Dirty = true;
            IffChanged(chunk.ChunkParent);
        }

        public void UnregisterObjects(IffFile file)
        {
            var objRegistry = Content.Get().WorldObjects;
            var defs = file.List<OBJD>();
            if (defs != null)
            {
                foreach (var def in defs)
                {
                    objRegistry.RemoveObject(def.GUID);
                }
            }
        }

        public void RegisterObjects(IffFile file)
        {
            var objRegistry = Content.Get().WorldObjects;
            var defs = file.List<OBJD>();
            if (defs != null)
            {
                foreach (var def in defs)
                {
                    objRegistry.AddObject(file, def);
                }
            }
        }

        public HashSet<IffFile> GetChangeList()
        {
            lock (this) return new HashSet<IffFile>(ChangedFiles);
        }

        public void DiscardChanges(IEnumerable<IffFile> files)
        {
            lock (this) foreach (var file in files) DiscardChange(file);
        }

        public void DiscardChange(IffFile file)
        {
            lock (this)
            {
                UnregisterObjects(file);
                file.Revert();
                ChangedFiles.Remove(file);
                RegisterObjects(file);
            }
        }

        public void DiscardChanges(IEnumerable<IffChunk> chunks)
        {
            lock (this) foreach (var chunk in chunks) DiscardChange(chunk);
        }

        public void DiscardChange(IffChunk chunk)
        {
            lock (this)
            {
                UnregisterObjects(chunk.ChunkParent);
                chunk.ChunkParent.Revert(chunk);
                if (chunk.ChunkParent.ListAll().Count(x => x.RuntimeInfo == ChunkRuntimeState.Modified || x.RuntimeInfo == ChunkRuntimeState.Delete) == 0)
                    ChangedFiles.Remove(chunk.ChunkParent);
                RegisterObjects(chunk.ChunkParent);
            }
        }

        public void DeletePatch(IffChunk chunk)
        {
            lock (this)
            {
                chunk.ChunkData = chunk.OriginalData;
                DiscardChange(chunk);
            }
        }

        public void SaveChanges(IEnumerable<IffFile> files)
        {
            lock (this) foreach (var file in files) SaveChange(file);
        }

        public void SaveChange(IffFile file)
        {
            lock (this)
            {
                if (file.RuntimeInfo.State == IffRuntimeState.Standalone)
                {
                    //just save out iff
                    var filename = file.RuntimeInfo.Path;
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                    using (var stream = new FileStream(filename, FileMode.Create))
                        file.Write(stream);

                    foreach (var chunk in file.ListAll())
                    {
                        chunk.RuntimeInfo = ChunkRuntimeState.Normal;
                    }
                }
                else
                {
                    string dest = Path.Combine(FSOEnvironment.ContentDir, ((file.RuntimeInfo.State == IffRuntimeState.PIFFClone) ? "Objects/" : "Patch/User/"));
                    Directory.CreateDirectory(dest);
                    if (file.RuntimeInfo.State == IffRuntimeState.ReadOnly)
                    {
                        file.RuntimeInfo.State = IffRuntimeState.PIFFPatch;
                    }

                    var stringResources = new HashSet<Type> { typeof(STR), typeof(CTSS), typeof(TTAs) };
                    var sprites = (file.RuntimeInfo.UseCase == IffUseCase.ObjectSprites);
                    var oldPatches = file.RuntimeInfo.Patches.ToList();
                    file.RuntimeInfo.Patches.Clear();

                    //find the old piff. should be the piff that doesn't end in .str.piff
                    var oldPIFF = oldPatches.FirstOrDefault(x => !x.Filename.EndsWith(".str.piff"))?.List<PIFF>()?.ElementAt(0);

                    var piff = FSO.Files.Formats.PiffEncoder.GeneratePiff(file, null, stringResources, oldPIFF);

                    string name = file.Filename.Substring(0, file.Filename.Length - 4); //get without extension

                    if (piff != null)
                    {
                        if (piff.Filename == null) piff.Filename = name + (sprites ? ".spf" : "") + ".piff";
                        var filename = dest + piff.Filename;
                        using (var stream = new FileStream(filename, FileMode.Create)) piff.Write(stream);
                        file.RuntimeInfo.Patches.Add(piff);
                    }

                    if (!sprites)
                    {
                        oldPIFF = oldPatches.FirstOrDefault(x => x.Filename.EndsWith(".str.piff"))?.List<PIFF>()?.ElementAt(0);
                        piff = FSO.Files.Formats.PiffEncoder.GeneratePiff(file, stringResources, null, oldPIFF);
                        if (piff != null)
                        {
                            if (piff.Filename == null) piff.Filename = name + ".str.piff";
                            var filename = dest + piff.Filename;
                            using (var stream = new FileStream(filename, FileMode.Create)) piff.Write(stream);
                            file.RuntimeInfo.Patches.Add(piff);
                        }
                    }
                }

                file.RuntimeInfo.Dirty = false;
                ChangedFiles.Remove(file);
            }
        }

        /// <summary>
        /// Runs the queued resource modifications. Should be executed from the game thread, so that any
        /// queued operations are run on the game thread. The chunks in use will be locked - it's up to external
        /// threads to consider that when retrieving a chunk's information for use.
        /// </summary>
        public void RunResModifications()
        {
            lock (this)
            {
                lock (ResActionQueue)
                {
                    while (ResActionQueue.Count > 0) ResActionQueue.Dequeue().Execute();
                }

                lock (InvokeQueue)
                {
                    while (InvokeQueue.Count > 0)
                    {
                        var inv = InvokeQueue.Dequeue();

                        inv.Function.DynamicInvoke(inv.Args);
                        inv.Sync.Set();
                    }
                }
            }
        }

        /// <summary>
        /// Queues an action to run on a game resource, which will be run later by the game thread.
        /// </summary>
        /// <param name="action"></param>
        public void QueueResMod(ResAction action)
        {
            lock (ResActionQueue)
            {
                ResActionQueue.Enqueue(action);
            }
        }

        public void BlockingResMod(ResAction action)
        {
            var wait = new AutoResetEvent(false);
            action.SetSignal(wait);
            lock (ResActionQueue)
            {
                ResActionQueue.Enqueue(action);
            }
            wait.WaitOne();
        }
    }

    public class ResInvokeElem {
        public Delegate Function;
        public object[] Args;
        public AutoResetEvent Sync;

        public ResInvokeElem(Delegate func, object[] args)
        {
            Function = func;
            Args = args;
            Sync = new AutoResetEvent(false);
        }
    }
}
