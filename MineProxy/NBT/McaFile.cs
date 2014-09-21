using System;
using System.IO.Compression;
using System.IO;
using System.Collections.Generic;
using System.Text;
using MiscUtil.IO;
using MiscUtil.Conversion;
using Org.BouncyCastle.Utilities.Zlib;

namespace MineProxy.NBT
{
    /// <summary>
    /// Representation of the mca map file format.
    /// </summary>
    public class McaFile
    {
        Dimensions dimension;
        int x, z;
        string path;

        public readonly McaChunk[,] chunks = new McaChunk[32, 32];

        public McaFile()
        {
        }

        public McaFile(Dimensions dim, int x, int z)
        {
            this.dimension = dim;
            this.x = x;
            this.z = z;

            //Decode chunk data
            /*MemoryStream ms = new MemoryStream(buffer, offset, sectors * 4096, false);
            using (GZipStream gzip = new GZipStream (ms, CompressionMode.Decompress))
            {
                EndianBinaryReader r = new EndianBinaryReader(EndianBitConverter.Big, gzip);
                Tag.ReadTag(r);
            }*/
        }

        public static string Path(Dimensions dim, int x, int z)
        {
            string path = "world/";
            if (dim != Dimensions.Overworld)
                path += "DIM" + ((int)dim) + "/";
            path += "region/r." + (x >> 9) + "." + (z >> 9) + ".mca";
            return path;
        }

        /// <summary>
        /// True if the file exists.
        /// </summary>
        /// <value><c>true</c> if exists; otherwise, <c>false</c>.</value>
        public bool Exists
        {
            get
            {
                return File.Exists(Path(dimension, x, z));
            }
        }

        /// <summary>
        /// Determines whether this instance has chunk the specified dx dy.
        /// </summary>
        /// <returns><c>true</c> if this instance has chunk the specified dx dy; otherwise, <c>false</c>.</returns>
        /// <param name="dx">chunk index inside region</param>
        /// <param name="dy">Dy.</param>
        public bool HasChunk(int dx, int dz)
        {
            return chunks [dx, dz] != null;
        }

        public static McaFile Load(Dimensions dim, int x, int z)
        {
            string path = McaFile.Path(dim, x, z);
            if (File.Exists(path) == false)
                return null;

            McaFile mca = new McaFile(dim, x, z);
            mca.LoadFromFile(path);
            return mca;
        }
        
        public static McaFile Load(string path)
        {
            McaFile mca = new McaFile();
            mca.LoadFromFile(path);
            return mca;
        }
        
        void LoadFromFile(string path)
        {
            this.path = path;
            byte[] buffer = File.ReadAllBytes(path);
            for (int dx = 0; dx < 32; dx++)
            {
                for (int dz = 0; dz < 32; dz++)
                {
                    chunks [dx, dz] = McaChunk.ReadChunk(buffer, dx, dz);
                }
            }
        }


        public void Save()
        {
            SaveAs(path);
        }

        public void SaveAs(string path)
        {
            this.path = path;

            string tmp = path + ".tmp";
            if (File.Exists(tmp))
                File.Delete(tmp);
            using (var s = File.Create(tmp))
            {
                //Fill header with zeroes
                byte[] zeros = new byte[4096 * 2];
                s.Write(zeros, 0, zeros.Length);

                for (int dx = 0; dx < 32; dx++)
                {
                    for (int dz = 0; dz < 32; dz++)
                    {
                        if (chunks [dx, dz] == null)
                            continue;

                        long start = s.Position;

                        if ((start & 0xFFF) != 0)
                            throw new InvalidProgramException("Must be even 12 bit start");

                        byte[] compressed = chunks [dx, dz].CompressChunk();
                        int size = compressed.Length + 5; //TEST + 5

                        //Write body header
                        byte[] sizeBytes = BigEndianBitConverter.Big.GetBytes(size);
                        s.Write(sizeBytes, 0, 4);
                        s.WriteByte(2); //zlib format
                        if (s.Position != start + 5)
                            throw new InvalidProgramException();
                        //Write compressed body
                        s.Write(compressed, 0, compressed.Length);

                        //Write file header
                        int i = 4 * (dx + dz * 32);
                        int sectors = ((size + 5 - 1) >> 12) + 1;
                        int offset = (int)(start >> 12);
                        Console.WriteLine("Write Chunk " + dx + "," + dz + " @ " + offset + ":" + sectors + "\t" + size + " bytes");
                        s.Seek(i, SeekOrigin.Begin);
                        s.WriteByte((byte)(offset >> 16));
                        s.WriteByte((byte)(offset >> 8));
                        s.WriteByte((byte)offset);
                        s.WriteByte((byte)sectors);

                        //Next position
                        s.Seek((offset + sectors) << 12, SeekOrigin.Begin);
                        if (s.Position < start + size)
                            throw new InvalidProgramException();
                    }
                }
            }
            if (File.Exists(path))
                File.Delete(path);
            File.Move(tmp, path);
            Console.WriteLine("Saved: " + path);

        }

        public static void Write(Tag tag, string path)
        {
            using (GZipStream gzip = new GZipStream (new FileStream (path, FileMode.Create), CompressionMode.Compress))
            {
                EndianBinaryWriter writer = new EndianBinaryWriter(EndianBitConverter.Big, gzip);
                tag.Write(writer);
            }
        }
    }
}

