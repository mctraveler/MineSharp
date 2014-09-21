using System;
using System.Collections.Generic;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class IncrementStatistic : PacketFromServer
    {
        public const byte ID = 0x37;

        public override byte PacketID { get { return ID; } }

        public List<Entry> Entries { get; set; }

        public class Entry
        {
            public string Name { get; set; }

            public int Amount { get; set; }
        }

        public IncrementStatistic()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Entries = new List<Entry>();
            int count = ReadVarInt(r);
            for (int n = 0; n < count; n++)
            {
                var e = new Entry();
                e.Name = ReadString8(r);
                e.Amount = ReadVarInt(r);
                Entries.Add(e); 
            }
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, (int)Entries.Count);
            foreach (var e in Entries)
            {
                WriteString8(w, e.Name);
                WriteVarInt(w, (int)e.Amount);
            }
        }
    }
}

