using System;
using MineProxy.Data;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class Spectate : PacketFromClient
    {
        public const byte ID = 0x18;

        public override byte PacketID { get { return ID; } }

        public Guid TargetPlayer { get; set; }

        public override string ToString()
        {
            return string.Format("[Spectate: {0}]", TargetPlayer);
        }

        protected override void Parse(EndianBinaryReader r)
        {
            TargetPlayer = new Guid(ReadString8(r));
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write(TargetPlayer.ToString());
        }
    }
}

