using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class ChunkDataBulk : PacketFromServer
    {
        public const byte ID = 0x26;

        public override byte PacketID { get { return ID; } }

        public bool Unknown { get; set; }

        public byte[] CompressedChunkColumns { get; set; }

        public UItem[] ColumnMeta { get; set; }
            
        public override string ToString()
        {
            return string.Format("[ChunkDataBulk: {0}, {1}]", CompressedChunkColumns.Length, ColumnMeta.Length);
        }

        public class UItem
        {
            public int X { get; set; }

            public int Z { get; set; }

            /// <summary>
            /// Bits are set for.
            /// </summary>
            public short OccupiedBitmap { get; set; }

            /// <summary>
            /// bits are set for subchunks with additional data
            /// </summary>
            public short AdditionalBitmap { get; set; }

            public override string ToString()
            {
                return string.Format("[Item: {0}, {1}, {2}, {3}]", X, Z, OccupiedBitmap, AdditionalBitmap);
            }

            public UItem(EndianBinaryReader r)
            {
                X = r.ReadInt32();
                Z = r.ReadInt32();
                OccupiedBitmap = r.ReadInt16();
                AdditionalBitmap = r.ReadInt16();
#if DEBUG
                if (AdditionalBitmap != 0)
                    Log.WriteServer("AdditionalData: " + AdditionalBitmap.ToString("X"));
#endif
            }

            public void Write(EndianBinaryWriter w)
            {
                w.Write((int)X);
                w.Write((int)Z);
                w.Write((short)OccupiedBitmap);
                w.Write((short)AdditionalBitmap);
            }
        }

        public ChunkDataBulk()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            int columnCount = r.ReadInt16();
            int compressedLength = r.ReadInt32();
            Unknown = r.ReadBoolean();
            CompressedChunkColumns = r.ReadBytesOrThrow(compressedLength);
            ColumnMeta = new UItem[columnCount];
            for (int n = 0; n < columnCount; n++)
            {
                ColumnMeta [n] = new UItem(r);
            }
        }
        
        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((short)ColumnMeta.Length);
            w.Write((int)CompressedChunkColumns.Length);
            w.Write((bool)Unknown);
            w.Write(CompressedChunkColumns);
            foreach (UItem i in ColumnMeta)
                i.Write(w);
        }
    }
}

