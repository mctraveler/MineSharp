using System;

namespace MineProxy
{
    public enum ProtocolVersion
    {
        v1_8 = 47,
        next
        //Not yet released
    }

    public static class ProtocolVersionExtensions
    {
        public static string ToText(this ProtocolVersion ver)
        {
            return ver.ToString().Replace("_", ".").Replace("pre", "-pre").TrimStart(new char[]{ 'v' });
        }
    }

}

