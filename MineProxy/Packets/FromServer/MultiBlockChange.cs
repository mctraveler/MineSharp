using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public partial class MultiBlockChange : PacketFromServer
    {
        public const byte ID = 0x22;

        public override byte PacketID { get { return ID; } }

        public int X { get; set; }

        public int Z { get; set; }

        /// <summary>
        /// <para>Format: 4 bytes per block.</para>
        /// <para>Byte0Bit0-3: X relative</para>
        /// <para>Byte0Bit4-7: Z relative</para>
        /// <para>Byte1: Y absolute</para>
        /// <para>Byte2,Byte3bit01: BlockID</para>
        /// <para>Byte3Bit4567: Metadata</para>
        /// </summary>
        public byte[] Data { get; set; }

        public override string ToString()
        {
            return string.Format("[MultiBlockChange: {0}, {1}, Blocks={2}]", X, Z, Data.Length / 4);
        }

        [Obsolete("Untested")]
        public MultiBlockChange()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            X = r.ReadInt32();
            Z = r.ReadInt32();
            int ArraySize = ReadVarInt(r);
            throw new NotImplementedException();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((int)X);
            w.Write((int)Z);
            throw new NotImplementedException();
        }
    }
}

