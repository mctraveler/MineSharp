using System;
using MiscUtil.Conversion;
using System.IO;
using MiscUtil.IO;
using Ionic.Zlib;

namespace MineProxy.NBT
{
    /// <summary>
    /// 16x16 chunk representation within.
    /// </summary>
    public class McaChunk
    {
        public Tag Tag { get; set; }

        public int dx, dz;

        public McaChunk(int dx, int dz)
        {
            this.dx = dx;
            this.dz = dz;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}]", dx, dz);
        }

        public static McaChunk ReadChunk(byte[] buffer, int dx, int dz)
        {
            //from mca file header
            int i = 4 * (dx + dz * 32);
            int offset = (buffer [i] << 16) | (buffer [i + 1] << 8) | (buffer [i + 2]);
            int sectors = buffer [i + 3];

            if (offset == 0 && sectors == 0)
                return null;

            Console.WriteLine("Read Chunk " + dx + "," + dz + " @ " + offset + ":" + sectors);

            if (offset == 0 || sectors == 0)
                throw new InvalidDataException("zero offset/sector");

            offset = offset << 12; //4096 = 2**12
            int length = EndianBitConverter.Big.ToInt32(buffer, offset); //byte length
            if (((length + 5 - 1) >> 12) + 1 != sectors)
                throw new InvalidDataException("Body length is larger than the sectors, " + length + " > " + (sectors * 4096));
            if (buffer [offset + 4] != 2)
                throw new NotImplementedException("Only support zlib compression");
            
            McaChunk mc = new McaChunk(dx, dz);

            MemoryStream ms = new MemoryStream(buffer, offset + 5, length);
            
            using (ZlibStream compressed = new ZlibStream(ms, CompressionMode.Decompress))
            {
                EndianBinaryReader r = new EndianBinaryReader(EndianBitConverter.Big, compressed);
                mc.Tag = Tag.ReadTag(r);
            }
            
            return mc;
        }
        
        public byte[] CompressChunk()
        {
            //File and body headers ar emanaged outside

            //All we do here is to write the compressed tag

            using (MemoryStream ms = new MemoryStream())
            {
                using (ZlibStream compressed = new ZlibStream(ms, CompressionMode.Compress, true))
                {
                    var w = new EndianBinaryWriter(EndianBitConverter.Big, compressed);
                    Tag.Write(w);
                    compressed.Flush();
                }
                return ms.ToArray();
            }
        }

    }
}

