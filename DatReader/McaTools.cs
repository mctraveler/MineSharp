using System;
using MineProxy.NBT;
using System.IO;
using MineProxy;

namespace DatReader
{
    public class McaTools
    {
        public static int Command(string[] args)
        {
            McaFile mca = McaFile.Load(args [0]);

            if (args.Length == 1)
            {
                Console.WriteLine(mca);
                return 0;
            }

            switch (args [1].ToLowerInvariant())
            {
                case "nop":
                    mca.SaveAs(args [2]);
                    Console.WriteLine("Saved as is");
                    return 0;

                case "cleanentities":
                    int removed = CleanEntities(mca);
                    mca.Save();
                    Console.WriteLine("Removed " + removed);
                    return 0;

                case "deletechunk":
                    DeleteChunk(mca, args);
                    mca.Save();
                    Console.WriteLine("Removed chunks");
                    return 0;

                case "nofire":
                    int fires = RemoveFire(mca);
                    Console.WriteLine("Removed " + fires + " fires");
                    mca.Save();
                    return 0;

                default:
                    Console.Error.WriteLine("Unknown command: " + args [1]);
                    return -1;
            }
        }

        static int RemoveFire(McaFile mca)
        {
            int removed = 0;
            foreach (var c in mca.chunks)
            {
                if (c == null)
                    continue;
                var sections = c.Tag ["Level"] ["Sections"] as TagList<TagCompound>;
                foreach (TagCompound s in sections.List)
                {
                    byte[] blocks = s ["Blocks"].ByteArray;
                    for (int n = 0; n < blocks.Length; n++)
                    {
                        if (blocks [n] == (byte)BlockID.Sand)
                        {
                            removed += 1;
                            blocks [n] = 0;
                        }
                    }
                }
            }
            return removed;
        }

        static int CleanEntities(McaFile mca)
        {
            int removed = 0;
            
            foreach (var c in mca.chunks)
            {
                if (c == null)
                    continue;
                var l = c.Tag ["Level"] ["Entities"] as TagList<TagCompound>;
                if (l == null)
                    continue;///empty lists are of type list<tagbyte>
                removed += l.List.Count;
                l.List.Clear();
            }
            return removed;
        }

        static int DeleteChunk(McaFile mca, string[] args)
        {
            for (int n = 2; n < args.Length; n++)
            {
                string[] p = args [n].Split('.');
                int dx = int.Parse(p [0]);
                int dz = int.Parse(p [1]);

                mca.chunks [dx, dz] = null;
            }
            return 0;
        }

        static int Custom()
        {
            //Manual work
            string path = "/home/nuxas/Traveler/current/world/region/r.0.-5.mca";
            McaFile mca = McaFile.Load(path);
            Console.WriteLine(mca.chunks [0, 0].Tag);
            mca.chunks [15, 23] = null;
            mca.chunks [16, 23] = null;
            mca.chunks [14, 24] = null;
            mca.chunks [11, 22] = null;
            mca.chunks [10, 21] = null;
                
            //Clear entities
            foreach (var c in mca.chunks)
            {
                if (c == null)
                    continue;
                var l = c.Tag ["Level"] ["Entities"] as TagList<TagCompound>;
                if (l == null)
                    continue;///empty lists are of type list<tagbyte>
                l.List.Clear();
            }
                
            mca.SaveAs(path + ".temp");
            File.Delete(path);
            File.Move(path + ".temp", path);
            return 0;
        }
    }
}

