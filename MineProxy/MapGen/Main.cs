using System;
using MineProxy.Castle;
using MineProxy.ChunkManagement;
using System.IO;

namespace MapGen
{
    class MainClass
    {
        public static void Main(string[] args)
        {
#if DEBUG
            Directory.SetCurrentDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Nuxas/wrapperTest/Castles/"));
#endif
            Console.WriteLine("Map Generator");
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: mapgen \"castle name\" \"seed\"");
                return;
            }
            int seed;
            if (args.Length >= 2)
                seed = args [1].GetHashCode();
            else
                seed = new Random().Next();

            var castle = new CastleWorld(args [0]);
            var cg = new Terrain(seed);
            castle.Chunks.Reset(cg);
            castle.Chunks.Flush();
            Console.WriteLine("done");
        }
    }
}
