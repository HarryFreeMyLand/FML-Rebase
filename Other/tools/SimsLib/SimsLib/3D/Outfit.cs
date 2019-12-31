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

namespace SimsLib.ThreeD
{
    /// <summary>
    /// Outfits collect together the light-, medium-, and dark-skinned versions of an 
    /// appearance and associate them collectively with a hand group and a body region (head or body).
    /// </summary>
    public class Outfit
    {
        public uint LightAppearanceFileID;
        public uint LightAppearanceTypeID;

        public uint MediumAppearanceFileID;
        public uint MediumAppearanceTypeID;

        public uint DarkAppearanceFileID;
        public uint DarkAppearanceTypeID;

        private uint m_HandGroup;
        public uint Region;

        public ulong HandGroup
        {
            //18 is HandGroup's TypeID.
            get { return (ulong)m_HandGroup << 32 | 18; }
        }

        public ulong GetAppearance(AppearanceType type)
        {
            switch (type)
            {
                case AppearanceType.Light:
                    return (ulong)LightAppearanceFileID << 32 | LightAppearanceTypeID;
                case AppearanceType.Medium:
                    return (ulong)MediumAppearanceFileID << 32 | LightAppearanceTypeID;
                case AppearanceType.Dark:
                    return (ulong)DarkAppearanceFileID << 32 | LightAppearanceTypeID;
            }

            return 0;
        }

        public void Read(Stream stream)
        {
            using (var io = IoBuffer.FromStream(stream))
            {
                var version = io.ReadUInt32();
                var unknown = io.ReadUInt32();

                LightAppearanceFileID = io.ReadUInt32();
                LightAppearanceTypeID = io.ReadUInt32();

                MediumAppearanceFileID = io.ReadUInt32();
                MediumAppearanceTypeID = io.ReadUInt32();

                DarkAppearanceFileID = io.ReadUInt32();
                DarkAppearanceTypeID = io.ReadUInt32();

                m_HandGroup = io.ReadUInt32();
                Region = io.ReadUInt32();
            }
        }
    }
}