using System;
using MiscUtil.IO;
using System.Collections.Generic;

namespace MineProxy.Packets
{
    public class TabCompleteClient: PacketFromClient
    {
        public const byte ID = 0x14;

        public override byte PacketID { get { return ID; } }

        public string Command { get; set; }

        public CoordInt Position { get; set; }

        protected override void Parse(EndianBinaryReader r)
        {
            Command = ReadString8(r);
            if (r.ReadBoolean())
                Position = CoordInt.Read(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteString8(w, Command);
            w.Write((bool)(Position != null));
            if (Position != null)
                Position.Write(w);
        }
    }
}

