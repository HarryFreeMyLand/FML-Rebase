﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the SimsLib.

The Initial Developer of the Original Code is
ddfczm. All Rights Reserved.

Contributor(s):
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimsLib.ThreeD
{
    /// <summary>
    /// Meshes define a textured polygon model whose vertices wrap around the bones of a skeleton.
    /// </summary>
    public class Mesh /*: I3DGeometry*/
    {
        /** 3D Data **/
        public MeshVertex[] RealVertexBuffer;
        public MeshVertex[] BlendVertexBuffer;
        protected short[] IndexBuffer;
        protected int NumPrimitives;
        public BoneBinding[] BoneBindings;
        public BlendData[] BlendData;

        private bool GPUMode;
        private DynamicVertexBuffer GPUBlendVertexBuffer;
        private IndexBuffer GPUIndexBuffer;

        public Mesh()
        {
        }

        public Mesh Clone()
        {
            var result = new Mesh()
            {
                BlendData = BlendData,
                BoneBindings = BoneBindings,
                NumPrimitives = NumPrimitives,
                IndexBuffer = IndexBuffer,
                RealVertexBuffer = RealVertexBuffer,
                BlendVertexBuffer = (MeshVertex[])BlendVertexBuffer.Clone()
            };
            return result;
        }

        public void StoreOnGPU(GraphicsDevice device)
        {
            GPUMode = true;
            GPUBlendVertexBuffer = new DynamicVertexBuffer(device, MeshVertex.SizeInBytes * BlendVertexBuffer.Length, BufferUsage.None);
            GPUBlendVertexBuffer.SetData(BlendVertexBuffer);

            GPUIndexBuffer = new IndexBuffer(device, sizeof(short) * IndexBuffer.Length, BufferUsage.None, IndexElementSize.SixteenBits);
            GPUIndexBuffer.SetData(IndexBuffer);
        }

        public void InvalidateMesh()
        {
            if (GPUMode)
            {
                GPUBlendVertexBuffer.SetData(BlendVertexBuffer);
            }
        }

        /// <summary>
        /// Transforms the verticies making up this mesh into
        /// the designated bone positions.
        /// </summary>
        /// <param name="bone">The bone to start with. Should always be the ROOT bone.</param>
        public void TransformVertices(Bone bone)
        {
            var binding = this.BoneBindings.FirstOrDefault(x => x.BoneName == bone.Name);
            if (binding != null)
            {
                for (var i = 0; i < binding.RealVertexCount; i++)
                {
                    var vertexIndex = binding.FirstRealVertex + i;
                    var blendVertexIndex = vertexIndex;//binding.FirstBlendVertex + i;

                    var realVertex = this.RealVertexBuffer[vertexIndex];
                    var matrix = Matrix.CreateTranslation(realVertex.Position) * bone.AbsoluteMatrix;

                    //Position
                    var newPosition = Vector3.Transform(Vector3.Zero, matrix);
                    this.BlendVertexBuffer[blendVertexIndex].Position = newPosition;

                    //Normals
                    matrix = Matrix.CreateTranslation(
                        new Vector3(realVertex.Normal.X,
                                    realVertex.Normal.Y,
                                    realVertex.Normal.Z)) * bone.AbsoluteMatrix;
                }
            }

            foreach (var child in bone.Children)
            {
                TransformVertices(child);
            }

            if (bone.Name == "ROOT")
            {
                this.InvalidateMesh();
            }
        }

        #region I3DGeometry Members

        public void DrawGeometry(GraphicsDevice gd)
        {
            if (GPUMode)
            {
                gd.VertexDeclaration = new VertexDeclaration(gd, MeshVertex.VertexElements);
                gd.Indices = GPUIndexBuffer;
                gd.Vertices[0].SetSource(GPUBlendVertexBuffer, 0, MeshVertex.SizeInBytes);
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, BlendVertexBuffer.Length, 0, NumPrimitives);
            }
            else
            {
                gd.VertexDeclaration = new VertexDeclaration(gd, MeshVertex.VertexElements);
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, BlendVertexBuffer, 0, BlendVertexBuffer.Length, IndexBuffer, 0, NumPrimitives);
            }
        }

        #endregion

        public void Draw(GraphicsDevice gd)
        {
            gd.VertexDeclaration = new VertexDeclaration(gd, MeshVertex.VertexElements);
            gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, BlendVertexBuffer, 0, BlendVertexBuffer.Length, IndexBuffer, 0, NumPrimitives);
        }

        public unsafe void Read(Stream stream)
        {
            using (var io = IoBuffer.FromStream(stream))
            {
                var version = io.ReadInt32();
                var boneCount = io.ReadInt32();
                var boneNames = new string[boneCount];
                for (var i = 0; i < boneCount; i++)
                {
                    boneNames[i] = io.ReadPascalString();
                }

                var faceCount = io.ReadInt32();
                NumPrimitives = faceCount;

                IndexBuffer = new short[faceCount * 3];
                int offset = 0;
                for (var i = 0; i < faceCount; i++)
                {
                    IndexBuffer[offset++] = (short)io.ReadInt32();
                    IndexBuffer[offset++] = (short)io.ReadInt32();
                    IndexBuffer[offset++] = (short)io.ReadInt32();
                }

                /** Bone bindings **/
                var bindingCount = io.ReadInt32();
                BoneBindings = new BoneBinding[bindingCount];
                for (var i = 0; i < bindingCount; i++)
                {
                    BoneBindings[i] = new BoneBinding
                    {
                        BoneIndex = io.ReadInt32(),
                        FirstRealVertex = io.ReadInt32(),
                        RealVertexCount = io.ReadInt32(),
                        FirstBlendVertex = io.ReadInt32(),
                        BlendVertexCount = io.ReadInt32()
                    };

                    BoneBindings[i].BoneName = boneNames[BoneBindings[i].BoneIndex];
                }

                var realVertexCount = io.ReadInt32();
                RealVertexBuffer = new MeshVertex[realVertexCount];

                for (var i = 0; i < realVertexCount; i++)
                {
                    RealVertexBuffer[i].UV.X = io.ReadFloat();
                    RealVertexBuffer[i].UV.Y = io.ReadFloat();
                }

                /** Blend data **/
                var blendVertexCount = io.ReadInt32();
                BlendData = new BlendData[blendVertexCount];
                for (var i = 0; i < blendVertexCount; i++)
                {
                    BlendData[i] = new BlendData
                    {
                        Weight = (float)io.ReadInt32() / 0x8000,
                        OtherVertex = io.ReadInt32()
                    };
                }

                var realVertexCount2 = io.ReadInt32();
                BlendVertexBuffer = new MeshVertex[realVertexCount];

                for (int i = 0; i < realVertexCount; i++)
                {
                    RealVertexBuffer[i].Position = new Microsoft.Xna.Framework.Vector3(
                        -io.ReadFloat(),
                        io.ReadFloat(),
                        io.ReadFloat()
                    );

                    BlendVertexBuffer[i].Position = RealVertexBuffer[i].Position;
                    BlendVertexBuffer[i].Normal = new Microsoft.Xna.Framework.Vector3(
                        -io.ReadFloat(),
                        io.ReadFloat(),
                        io.ReadFloat()
                    );
                    BlendVertexBuffer[i].UV = RealVertexBuffer[i].UV;
                }
            }
        }
    }
}