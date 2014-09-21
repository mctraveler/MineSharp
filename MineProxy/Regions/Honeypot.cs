using System;

namespace MineProxy
{
    /// <summary>
    /// Helper class for Worldregions starting with
    /// </summary>
    public static class Honeypot
    {
        public const string Type = "hp";

        public static bool Trigger(Client player, WorldRegion r)
        {
#if DEBUG
            player.TellAbove("DEBUG: ", "You triggered the ban");
#else
            player.BanByServer(DateTime.Now.AddMinutes(30), r.Name);
#endif
            return true;
        }
    }
}

