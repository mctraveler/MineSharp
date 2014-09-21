using System;
using System.IO;
using Ionic.Zlib;

namespace MineProxy.Packets
{
    public static class Compression
    {
        public static byte ReadCompressedID(byte[] compressed)
        {
            using (MemoryStream input = new MemoryStream(compressed))
            using (ZlibStream zin = new ZlibStream(input, CompressionMode.Decompress))
            {
                int val = zin.ReadByte();
                if (val < 0)
                    throw new InvalidDataException();
                return (byte)val;
            }
        }

        public static byte[] Decompress(byte[] compressed, int uncompressedSize)
        {
            using (MemoryStream input = new MemoryStream(compressed))
            using (ZlibStream zin = new ZlibStream(input, CompressionMode.Decompress))
            {
                byte[] uncompressed = new byte[uncompressedSize];
                ReadBuffer(zin, uncompressed);
                return uncompressed;
            }
        }

        /// <summary>
        /// Compress to CompressedData
        /// </summary>
        public static MemoryStream Compress(byte[] buffer)
        {
            var output = new MemoryStream();
            using (ZlibStream zout = new ZlibStream(output, CompressionMode.Compress))
            {
                zout.Write(buffer, 0, buffer.Length);
                zout.Flush();
            }
            return output;
        }

        private static void ReadBuffer(Stream stream, byte[] buffer)
        {
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int read = stream.Read(buffer, totalRead, buffer.Length - totalRead);
                //int read = stream.Read(buffer, totalRead, buffer.Length - totalRead); //DOES NOT WORK!!!
                if (read == 0)
                    throw new EndOfStreamException();
                totalRead += read;
            }

        }

    }
}

