using System;
using System.Threading;
using MineProxy.Worlds;
using System.Collections.Generic;
using MineProxy.Packets;

namespace MineProxy.Regions
{
    public static class WarpPortalVisuals
    {
        //static Timer ticker;

        public static void Init()
        {
            //ticker = new Timer(Tick, null, 1000, 4000);
        }

        public static void Stop()
        {
            //ticker.DisposeLog();
            //ticker = null;
        }

        static void Tick(object state)
        {
            //Send portal effects
            //Particle.portal();
        }

        /// <summary>
        /// If regions are changed reload portal updates
        /// </summary>
        public static void RegionsUpdated(List<WorldRegion> list)
        {

        }

        /// <summary>
        /// Send portal blocks of nearby portals
        /// </summary>
        /// <param name="s">S.</param>
        /// <param name="r">The red component.</param>
        public static void EnterRegion(Client player, WorldRegion r)
        {
            if (r == null || r.SubRegions == null)
                return;

            var packets = new List<PacketFromServer>();
            foreach (var sub in r.SubRegions)
            {
                if (sub.Type != WarpZone.Type)
                    continue;

                //Send block updates
                for (int x = sub.MinX; x < sub.MaxX; x++)
                {
                    for (int y = sub.MinY; y < sub.MaxY; y++)
                    {
                        for (int z = sub.MinZ; z < sub.MaxZ; z++)
                        {
                            packets.Add(new BlockChange(new CoordInt(x, y, z), BlockID.NetherPortal));
                        }
                    }
                }
            }

            if (packets.Count > 0)
            {
                Debug.WriteLine("Sending " + packets.Count + "portal packets");
                player.Queue.Queue(new Packets.PrecompiledPacket(packets));
            }
        }
    }
}

