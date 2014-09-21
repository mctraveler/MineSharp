using System;
using MineProxy;
using MineProxy.NBT;
using System.IO;

namespace DatReader
{
    class MainClass
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: dr filename command");
                return -1;
            }

            if (args [0].EndsWith(".mca"))
                return McaTools.Command(args);
            
            if (args [0].EndsWith("level.dat"))
                return LevelTools.Command(args);
            
            if (args [0].EndsWith(".dat"))
                return PlayerTools.Command(args);

            Console.Error.WriteLine("Unknown file suffix: " + args [0]);
            return -1;
        }
    }
}
