using System;

namespace MineProxy
{
    [Flags]
    public enum Permissions
    {
        /// <summary>
        /// In the admin list at all
        /// </summary>
        AnyAdmin = 0,
        ColorSign = 1,
        Ban = 2,
        Region = 4,
        CreativeFly = 8,
        CreativeBuild = 0x10,
        Cloak = 0x20,
        Castle = 0x40,
        /// <summary>
        /// Shutdown/restart the server
        /// </summary>
        Server = 0x80,
    }
}

