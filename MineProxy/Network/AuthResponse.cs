using System;
using System.Collections.Generic;

namespace MineProxy.Network
{
    /// <summary>
    /// From https://sessionserver.mojang.com/session/minecraft/hasJoined?username=username&serverId=hash
    /// </summary>
    public class AuthResponse
    {
        public Guid id;

        public List<Property> properties = new List<Property>();

        public class Property
        {
            public string name;
            public byte[] value;
            public byte[] signature;
        }
    }
}

