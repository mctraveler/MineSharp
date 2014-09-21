using System;

namespace MineProxy.Packets
{
    public class SetCompression : PacketFromServer
    {
        public const int ID = 0x03;

        public override byte PacketID { get { return ID; } }

        public const int DefaultMaxSize = 256;

        /// <summary>
        /// Maximum size before being compressed
        /// </summary>
        public int MaxSize { get; set; }

        public SetCompression()
        {
            this.MaxSize = DefaultMaxSize;
        }

        public SetCompression(byte[] buffer)
        {
            SetPacketBuffer(buffer);
            Parse();
        }

        public override string ToString()
        {
            return string.Format("[SetCompression: MaxSize={0}]", MaxSize);
        }

        protected override void Parse(MiscUtil.IO.EndianBinaryReader r)
        {
            MaxSize = ReadVarInt(r);
        }

        protected override void Prepare(MiscUtil.IO.EndianBinaryWriter w)
        {
            WriteVarInt(w, MaxSize);
        }
    }
}

