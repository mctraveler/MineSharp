using System;
using MiscUtil.IO;
using System.IO;
using MiscUtil.Conversion;

namespace MineProxy.Packets
{
    public class ServerPong : PacketFromServer
    {
        public const int ID = 0x01;

        public override byte PacketID { get { return ID; } }

        public long Time { get; set; }

        public ServerPong(long time)
        {
            this.Time = time;
        }

        public override string ToString()
        {
            return string.Format("[ServerPong {0}]", Time);
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Time = r.ReadInt64();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((long)Time);
        }
    }
}

