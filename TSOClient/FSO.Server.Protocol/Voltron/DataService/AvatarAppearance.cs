﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Server.Protocol.Voltron.DataService
{
    public class AvatarAppearance
    {
        public ulong AvatarAppearance_BodyOutfitID { get; set; }
        public byte AvatarAppearance_SkinTone { get; set; }
        public ulong AvatarAppearance_HeadOutfitID { get; set; }
    }
}
