﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FSO.Common.Utils;
using FSO.Files.Utils;

namespace FSO.Vitaboy
{
    /// <summary>
    /// Bindings points to meshes and appearances.
    /// </summary>
    public class Binding
    {
        public string Bone;
        public uint MeshGroupID;
        public uint MeshFileID;
        public uint MeshTypeID;

        public string MeshName; //bmd
        public string TextureName;

        public int CensorFlagBits;
        public int Zero;

        public uint TextureGroupID;
        public uint TextureFileID;
        public uint TextureTypeID;

        public Binding TS1Copy()
        {
            return new Binding() { MeshName = this.MeshName, TextureName = this.TextureName };
        }

        /// <summary>
        /// Reads a binding from a stream.
        /// </summary>
        /// <param name="stream">A Stream instance holding a binding.</param>
        public void Read(Stream stream)
        {
            using (var io = IoBuffer.FromStream(stream))
            {
                var version = io.ReadUInt32();
                if (version != 1)
                {
                    throw new Exception("Unknown binding version");
                }

                Bone = io.ReadPascalString();
                var meshType = io.ReadUInt32();
                if (meshType == 8)
                {
                    this.MeshGroupID = io.ReadUInt32();
                    this.MeshFileID = io.ReadUInt32();
                    this.MeshTypeID = io.ReadUInt32();
                }

                var textureType = io.ReadUInt32();
                if (textureType == 8)
                {
                    this.TextureGroupID = io.ReadUInt32();
                    this.TextureFileID = io.ReadUInt32();
                    this.TextureTypeID = io.ReadUInt32();
                }
            }
        }

        public void Write(Stream stream)
        {
            using (var io = IoWriter.FromStream(stream))
            {
                io.WriteUInt32(1);
                io.WritePascalString(Bone);
                io.WriteUInt32(8);
                io.WriteUInt32(MeshGroupID);
                io.WriteUInt32(MeshFileID);
                io.WriteUInt32(MeshTypeID);
                io.WriteUInt32(8);
                io.WriteUInt32(TextureGroupID);
                io.WriteUInt32(TextureFileID);
                io.WriteUInt32(TextureTypeID);
            }
        }
    }
}
