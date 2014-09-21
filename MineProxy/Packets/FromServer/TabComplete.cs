using System;
using MiscUtil.IO;
using System.Collections.Generic;

namespace MineProxy.Packets
{
    public class TabComplete: PacketFromServer
    {
        public const byte ID = 0x3A;

        public override byte PacketID { get { return ID; } }

        public List<string> Alternatives { get; set; }

        public TabComplete()
        {
            Alternatives = new List<string>();
        }

        public static TabComplete Single(string str)
        {
            var tc = new TabComplete();
            tc.Alternatives.Add(str);
            return tc;
        }

        protected override void Parse(EndianBinaryReader r)
        {
            int count = ReadVarInt(r);
            Alternatives = new List<string>(count);
            for (int n = 0; n < count; n++)
                Alternatives.Add(ReadString8(r));
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, Alternatives.Count);
            foreach (var s in Alternatives)
                WriteString8(w, s);
        }
    }
}

