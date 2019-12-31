﻿using FSO.SimAntics.Engine;
using FSO.SimAntics.NetPlay.Model;
using FSO.SimAntics.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FSO.SimAntics.Model.TSOPlatform;
using FSO.SimAntics.NetPlay.EODs.Model;

namespace FSO.SimAntics.Marshals.Threads
{
    public class VMThreadMarshal : VMSerializable
    {
        public VMStackFrameMarshal[] Stack;
        public VMQueuedActionMarshal[] Queue;
        public sbyte ActiveQueueBlock;
        public short[] TempRegisters = new short[20];
        public int[] TempXL = new int[2];
        public VMPrimitiveExitCode LastStackExitCode = VMPrimitiveExitCode.GOTO_FALSE;

        public VMAsyncState BlockingState; //NULLable
        public VMEODPluginThreadState EODConnection; //NULLable
        public bool Interrupt;

        public ushort ActionUID;
        public int DialogCooldown;

        public uint ScheduleIdleStart;

        public int Version;

        public VMThreadMarshal() { }
        public VMThreadMarshal(int version) { Version = version; }

        public void SerializeInto(BinaryWriter writer)
        {
            writer.Write(Stack.Length);
            foreach (var item in Stack)
            {
                writer.Write((byte)((item is VMRoutingFrameMarshal) ? 1 : 0)); //mode, 1 for routing frame
                item.SerializeInto(writer);
            }

            writer.Write(Queue.Length);
            foreach (var item in Queue) item.SerializeInto(writer);
            writer.Write(ActiveQueueBlock);

            writer.Write(VMSerializableUtils.ToByteArray(TempRegisters));
            foreach (var item in TempXL) writer.Write(item);
            writer.Write((byte)LastStackExitCode);

            writer.Write(BlockingState != null);
            if (BlockingState != null) VMAsyncState.SerializeGeneric(writer, BlockingState);
            writer.Write(EODConnection != null);
            if (EODConnection != null) EODConnection.SerializeInto(writer);
            writer.Write(Interrupt);

            writer.Write(ActionUID);
            writer.Write(DialogCooldown);

            writer.Write(ScheduleIdleStart);
        }

        public void Deserialize(BinaryReader reader)
        {
            var stackN = reader.ReadInt32();
            Stack = new VMStackFrameMarshal[stackN];
            for (int i = 0; i < stackN; i++)
            {
                var type = reader.ReadByte();
                Stack[i] = (type == 1) ? new VMRoutingFrameMarshal(Version) : new VMStackFrameMarshal(Version);
                Stack[i].Deserialize(reader);
            }

            var queueN = reader.ReadInt32();
            Queue = new VMQueuedActionMarshal[queueN];
            for (int i = 0; i < queueN; i++)
            {
                Queue[i] = new VMQueuedActionMarshal(Version);
                Queue[i].Deserialize(reader);
            }
            if (Version > 4) ActiveQueueBlock = reader.ReadSByte();

            TempRegisters = new short[20];
            for (int i = 0; i < 20; i++) TempRegisters[i] = reader.ReadInt16();
            TempXL = new int[2];
            for (int i = 0; i < 2; i++) TempXL[i] = reader.ReadInt32();
            LastStackExitCode = (VMPrimitiveExitCode)reader.ReadByte();

            if (reader.ReadBoolean()) BlockingState = VMAsyncState.DeserializeGeneric(reader, Version);
            else BlockingState = null;
            if (Version > 2 && reader.ReadBoolean())
            {
                EODConnection = new VMEODPluginThreadState();
                EODConnection.Version = Version;
                EODConnection.Deserialize(reader);
            }
            
            Interrupt = reader.ReadBoolean();

            ActionUID = reader.ReadUInt16();
            DialogCooldown = reader.ReadInt32();
            if (Version > 15) ScheduleIdleStart = reader.ReadUInt32();
        }
    }
}