﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the Iffinator.

The Initial Developer of the Original Code is
Mats 'Afr0' Vederhus. All Rights Reserved.

Contributor(s): Nicholas Roth.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Iffinator.Flash
{
    public class SpriteFrame
    {
        private uint m_FrameIndex;
        private uint m_Version, m_Size;     //These are only stored in version 1001
        private ushort m_Width, m_Height;
        private uint m_Flag;
        private ushort m_PaletteID;
        private PaletteMap m_PalMap;
        private Color m_TransparentPixel;
        private ushort m_X, m_Y;
        private FastPixel m_BitmapData;
        private FastPixel m_ZBuffer;
        private bool m_HasZBuffer;
        private bool m_HasAlpha;

        /// <summary>
        /// Stored for frames that are in SPR2s version 1000.
        /// NOT part of a frame in the actual file!
        /// Should be equal to the index in the offsettable.
        /// </summary>
        public uint FrameIndex
        {
            get { return m_FrameIndex; }
            set { m_FrameIndex = value; }
        }

        /// <summary>
        /// Stored in version 1001. Value is
        /// invariably 1001.
        /// </summary>
        public uint Version
        {
            get { return m_Version; }
            set { m_Version = value; }
        }

        /// <summary>
        /// The size of this compressed frame, in bytes.
        /// Only stored in version 1001.
        /// </summary>
        public uint Size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }

        public ushort Width
        {
            get { return m_Width; }
            set { m_Width = value; }
        }

        public ushort Height
        {
            get { return m_Height; }
            set { m_Height = value; }
        }

        public uint Flag
        {
            get { return m_Flag; }
            set { m_Flag = value; }
        }

        public ushort PaletteID
        {
            get { return m_PaletteID; }
            set { m_PaletteID = value; }
        }

        public PaletteMap PalMap
        {
            get { return m_PalMap; }
            set { m_PalMap = value; }
        }

        /*public int TransparentPixel
        {
            get { return m_TransparentPixel; }
            set { m_TransparentPixel = value; }
        }*/

        public Color TransparentPixel
        {
            get { return m_TransparentPixel; }
            set { m_TransparentPixel = value; }
        }

        public ushort XLocation
        {
            get { return m_X; }
            set { m_X = value; }
        }

        public ushort YLocation
        {
            get { return m_Y; }
            set { m_Y = value; }
        }

        public FastPixel BitmapData
        {
            get { return m_BitmapData; }
            set { m_BitmapData = value; }
        }

        public FastPixel ZBuffer
        {
            get { return m_ZBuffer; }
            set { m_ZBuffer = value; }
        }

        /// <summary>
        /// Does this frame have a z-buffer?
        /// Only SPR2 sprites supports this.
        /// If a SPR2 sprite has an alpha channel,
        /// it must also have a z-buffer.
        /// </summary>
        public bool HasZBuffer
        {
            get { return m_HasZBuffer; }
        }

        /// <summary>
        /// Does this frame have a alpha channel?
        /// Only SPR2 sprites supports this.
        /// If a SPR2 sprite has an alpha channel,
        /// it must also have a z-buffer.
        /// </summary>
        public bool HasAlphaBuffer
        {
            get { return m_HasAlpha; }
        }

        public SpriteFrame()
        {
        }

        /// <summary>
        /// Initializes this spriteframe.
        /// </summary>
        /// <param name="Alpha">Does this spriteframe have an alpha-buffer? Only applicable for SPR2.</param>
        /// <param name="HasZBuffer">Does this spriteframe have a z-buffer? Only applicable for SPR2.</param>
        public void Init(bool Alpha, bool HasZBuffer)
        {
            m_HasAlpha = Alpha;

            if (m_Width > 0 && m_Height > 0)
            {
                m_BitmapData = new FastPixel(new Bitmap(m_Width, m_Height), Alpha);
                m_BitmapData.Lock();

                if (HasZBuffer)
                {
                    m_HasZBuffer = true;

                    m_ZBuffer = new FastPixel(new Bitmap(m_Width, m_Height), Alpha);
                    m_ZBuffer.Lock();
                }
            }
            else
            {
                m_BitmapData = new FastPixel(new Bitmap(1, 1), Alpha);
                m_BitmapData.Lock();

                if (HasZBuffer)
                {
                    m_HasZBuffer = true;

                    m_ZBuffer = new FastPixel(new Bitmap(1, 1), Alpha);
                    m_ZBuffer.Lock();
                }
            }
        }
    }
}
