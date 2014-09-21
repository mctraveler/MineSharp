using System;
using MineProxy.NBT;
using MineProxy;

namespace DatReader
{
    public class PlayerTools
    {
        public static int Command(string[] args)
        {
            Tag t = FileNBT.Read(args [0]);
            
            if (args.Length < 2)
            {
                Console.WriteLine(t);
                return 0;
            }

            PlayerDat d = new PlayerDat(t);
            
            if (args [1] == "test")
            {
                //Generate new Tag from player data
                Console.WriteLine();
                Console.WriteLine(d.ExportTag());
            }
            
            if (args [1] == "noxp")
            {
                if (d.XpTotal == 0 && d.XpLevel == 0 && d.Xp == 0)
                    return 0;
                d.Xp = 0;
                d.XpLevel = 0;
                d.XpTotal = 0;
                FileNBT.Write(d.ExportTag(), args [0]);
                return 0;
            }
            
            if (args [1] == "nocreative")
            {
                if (d.playerGameType != 1)
                    return 0;
                d.playerGameType = 0;
                FileNBT.Write(d.ExportTag(), args [0]);
                Console.WriteLine("Changed mode to 0");
                return 0;
            }
            
            if (args [1] == "lotsxp")
            {
                d.Xp = 50;
                d.XpLevel = 5000;
                d.XpTotal = 5000;
                FileNBT.Write(d.ExportTag(), args [0]);
                return 0;
            }
            
            if (args [1] == "spawn")
            {
                d.Pos.X = 16;
                d.Pos.Y = 200;
				d.Pos.Z = -16;
				FileNBT.Write(d.ExportTag(), args [0]);
                return 0;
            }

            
            if (args [1] == "pos")
            {
                Console.WriteLine(args.Length);
                d.Pos.X = int.Parse(args [2].Replace("(", "").Replace(",", ""));
                d.Pos.Y = int.Parse(args [3].Replace(",", ""));
                d.Pos.Z = int.Parse(args [4].Replace(")", "").Replace(",", ""));
                FileNBT.Write(d.ExportTag(), args [0]);
                Console.WriteLine("New pos: " + d.Pos);
                return 0;
            }

            Console.WriteLine("Unknown player command");
            return -1;
        }
    }
}

