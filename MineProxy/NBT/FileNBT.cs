using System;
using System.IO.Compression;
using System.IO;
using System.Collections.Generic;
using System.Text;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace MineProxy.NBT
{
    public static class FileNBT
    {
        public static Tag Read(string path)
        {
            using (GZipStream gzip = new GZipStream (new FileStream (path, FileMode.Open, FileAccess.Read), CompressionMode.Decompress))
            {
                EndianBinaryReader r = new EndianBinaryReader(EndianBitConverter.Big, gzip);
                return Tag.ReadTag(r);
            }
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

