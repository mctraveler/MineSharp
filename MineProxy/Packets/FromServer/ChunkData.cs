using System;
using System.IO;
using MiscUtil.IO;
using Ionic.Zlib;

namespace MineProxy.Packets
{
    public class ChunkData : PacketFromServer
    {
        public const byte ID = 0x21;

        public override byte PacketID { get { return ID; } }

        /// <summary>
        /// Chunk index
        /// </summary>
        /// <value>The x.</value>
        public int X { get; set; }

        /// <summary>
        /// Chunk index
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// Ground-up continous
        /// </summary>
        public bool Complete { get; set; }

        /// <summary>
        /// bits mark if chunk 0 to 15 is present in the uncompressed data
        /// </summary>
        public ushort ChunkBitMap { get; set; }

        public byte[] BlockType { get; set; }

        public byte[] BlockMeta { get; set; }

        public byte[] BlockLight { get; set; }

        public byte[] BlockSkyLight { get; set; }

        public byte[] Biome { get; set; }

        public override string ToString()
        {
            return string.Format("[MapChunk: ({0}, {1}) ({2}, {3})]", X, Z, Complete, ChunkBitMap);
        }

        //[Obsolete("Not tested")]
        public ChunkData()
        {
            Complete = true;
        }

        protected override void Parse(EndianBinaryReader r)
        {
            X = r.ReadInt32();
            Z = r.ReadInt32();
            Complete = r.ReadBoolean();
            ChunkBitMap = r.ReadUInt16();
            int length = ReadVarInt(r);
            InitBuffer();
            if (length != BlockType.Length + BlockMeta.Length + BlockLight.Length + BlockSkyLight.Length + (Complete ? Biome.Length : 0))
                throw new InvalidDataException();
            var s = r.BaseStream;
            ReadBuffer(s, BlockType);
            ReadBuffer(s, BlockMeta);
            ReadBuffer(s, BlockLight);
            ReadBuffer(s, BlockSkyLight);
            if (Complete)
                ReadBuffer(s, Biome);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((int)X);
            w.Write((int)Z);
            w.Write((bool)Complete);
            w.Write((ushort)ChunkBitMap);
            WriteVarInt(w, BlockType.Length + BlockMeta.Length + BlockLight.Length + BlockSkyLight.Length + (Complete ? Biome.Length : 0));
            w.Write(BlockType, 0, BlockType.Length);
            w.Write(BlockMeta, 0, BlockMeta.Length);
            w.Write(BlockLight, 0, BlockLight.Length);
            w.Write(BlockSkyLight, 0, BlockSkyLight.Length);
            if (Complete)
                w.Write(Biome, 0, Biome.Length);
        }

        /// <summary>
        /// Prepare uncompressed data buffer with an empty array
        /// </summary>
        void InitBuffer()
        {
            int count = 0;
            int bitmap = ChunkBitMap;
            while (bitmap != 0)
            {
                count += bitmap & 1;
                bitmap = bitmap >> 1;
            }

            BlockType = new byte[count * 16 * 16 * 16];
            BlockMeta = new byte[count * 16 * 16 * 8];
            BlockLight = new byte[count * 16 * 16 * 8];
            BlockSkyLight = new byte[count * 16 * 16 * 8];
            if (Complete)
                Biome = new byte[16 * 16];
        }

        public void InitContinousChunks(int count)
        {
            ChunkBitMap = 0;
            for (int n = 0; n < count; n++)
                ChunkBitMap += (ushort)(1 << n);
            Complete = true;
            InitBuffer();
        }

        public int GetIndex(int x, int y, int z)
        {
            x = x & 0xF;
            z = z & 0xF;
            return x + z * 16 + y * 16 * 16;
        }

        public int GetIndex(CoordInt c)
        {
            int x = c.X & 0xF;
            int z = c.Z & 0xF;

            return x + z * 16 + c.Y * 16 * 16;
        }

        private void ReadBuffer(Stream stream, byte[] buffer)
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

