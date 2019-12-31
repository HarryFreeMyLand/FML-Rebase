﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSOClient.Network
{
    /// <summary>
    /// Size of packet headers.
    /// </summary>
    public enum PacketHeaders
    {
        UNENCRYPTED = 3,
        ENCRYPTED = 5
    }
}
