using System;
using MiscUtil.IO;
using System.IO;
using MiscUtil.Conversion;

namespace MineProxy.Packets
{
    public class ServerPing : PacketFromClient
    {
        public const int ID = 0x01;

        public override byte PacketID { get { return ID; } }

        public long Time { get; set; }

        public override string ToString()
        {
            return string.Format("[ServerPing {0}]", Time);
        }

        public ServerPing(byte[] buffer)
        {
            SetPacketBuffer(buffer);
            Parse();
        }

        protected override void Parse(EndianBinaryReader r)
        {
            this.Time = r.ReadInt64();
            DebugGotAll(r);
        }
        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((long)Time);
        }
    }
}

