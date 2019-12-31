﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FSO.Vitaboy
{
    /// <summary>
    /// Vertex that makes up a mesh.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MeshVertex:IVertexType
    {
        public Vector3 Position;
        /** UV Mapping **/
        public Vector2 UV;
        public Vector3 Normal;

        public static int SizeInBytes = sizeof(float) * 8;

        public readonly static VertexDeclaration VertexElements = new VertexDeclaration
        (
             new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement( sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement( sizeof(float) * 5, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0 )
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexElements; }
        }
    }
}
