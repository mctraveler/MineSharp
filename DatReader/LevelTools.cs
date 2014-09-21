using System;
using MineProxy.NBT;

namespace DatReader
{
    public class LevelTools
    {
        public static int Command(string[] args)
        {
            Tag t = FileNBT.Read(args [0]);
            
            if (args.Length < 2)
            {
                Console.WriteLine(t);
                return 0;
            }

            if (args [1] == "worldSpawn")
            {
				t ["Data"] ["SpawnX"].Int = 16;
				t ["Data"] ["SpawnY"].Int = 200;
				t ["Data"] ["SpawnZ"].Int = -16;
                FileNBT.Write(t, args [0]);
                return 0;
            }
            
            if (args [1] == "newseed")
            {
				t ["Data"] ["RandomSeed"].Long = 12345;
                FileNBT.Write(t, args [0]);
                return 0;
            }

            Console.WriteLine("Unknown Level command");
            return -1;
        }
    }
}

