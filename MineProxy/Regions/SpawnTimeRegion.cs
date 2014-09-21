using System;
using MineProxy.Chatting;
using MineProxy.Worlds;
using MineProxy.Packets;

namespace MineProxy
{
    public static class SpawnTimeRegion
    {
        public const string Type = "spawntime";

        internal static void Entering(Client player)
        {
        }

        internal static void Leaving(Client player)
        {
            //Resume outside time
            player.SendToClient(World.Main.Time);
        }

        internal static bool FilterClient(WorldRegion region, Client player, Packet packet)
        {
            if (packet.PacketID != PlayerPositionLookClient.ID)
                return false;

            CoordDouble pos = player.Session.Position;
            
            //Inner region
            if (region.SubRegions.Count == 0)
                return false;
            WorldRegion inner = region.SubRegions[0];
			
            //Find proportions between region edges
            double dOuter = pos.X - region.MinX;
            dOuter = Math.Min(dOuter, region.MaxX - pos.X);
            dOuter = Math.Min(dOuter, pos.Z - region.MinZ);
            dOuter = Math.Min(dOuter, region.MaxZ - pos.Z);
            double dInner = inner.MinX - pos.X;
            dInner = Math.Max(dInner, pos.X - inner.MaxX);
            dInner = Math.Max(dInner, inner.MinZ - pos.Z);
            dInner = Math.Max(dInner, pos.Z - inner.MaxZ);

            double frac = dOuter / (dOuter + dInner);
#if DEBUG
            //player.Tell(frac.ToString("0.00") + ": " + dOuter + " " + dInner);
#endif

            player.SendToClient(NightTime(frac));
			
            return false;
        }

        internal static bool FilterServer(WorldSession player, Packet packet)
        {
            return packet.PacketID == TimeUpdate.ID;
        }

        static TimeUpdate NightTime(double fraction)
        {
            if (fraction < 0)
                fraction = 0;
            if (fraction > 1)
                fraction = 1;
            long real = (World.Main.Time.Time + 6000) % 24000;
            if (real > 12000)
                real -= 24000;
            long diff = (long)(fraction * (-real));
#if DEBUG
            Parser.TellNuxas("DEBUG: ", fraction.ToString("0.00") + ": " + " " + diff);
#endif
            return new TimeUpdate(World.Main.Time.Time + diff);
        }
    }
}

