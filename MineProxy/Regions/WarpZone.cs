using System;
using System.IO;
using MineProxy.Worlds;

namespace MineProxy
{
    /// <summary>
    /// A special region which when used in SetWorld(Worl.Warp, here) can tp the player to that other place
    /// </summary>
    public class WarpZone// : WorldRegion
    {
        public const string Type = "warp";

        public CoordDouble Destination { get; set; }

        public int DestinationDimension { get; set; }

        public Worlds.World DesinationWorld { get; set; }

        public WarpZone(string destination)
        {
            string[] p = destination.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            //World
            if (p.Length == 1)
            {
                this.DesinationWorld = GetWorld(p [0]);
                this.Destination = null;
                return; //Leave Destination null
            }

            if (p.Length < 5)
                this.DesinationWorld = World.Main;
            else
                this.DesinationWorld = GetWorld(p [4]);

            if (p.Length < 4)
                this.DestinationDimension = 0;
            else
                this.DestinationDimension = int.Parse(p [3]);

            if (p.Length < 3)
                return; //invalid destination
            
            this.Destination = new CoordDouble(double.Parse(p [0].Trim()), double.Parse(p [1].Trim()), double.Parse(p [2].Trim()));
        }

        static World GetWorld(string codename)
        {
            codename = Path.GetFileName(codename); //for safety

            var parts = codename.Split(':');
            if (parts.Length != 2)
                return Worlds.World.Construct;

            if (parts [0] == "vanilla")
            {
                if (World.VanillaWorlds.ContainsKey(parts [1]))
                    return World.VanillaWorlds [parts [1]];
            }

            //If no match
            return Worlds.World.Construct;
        }
    }
}

