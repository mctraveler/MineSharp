using System;
using MiscUtil.IO;
using System.Collections.Generic;

namespace MineProxy.Packets
{
    public class TeamUpdate : PacketFromServer
    {
        public const byte ID = 0x3E;

        public override byte PacketID { get { return ID; } }

        /// <summary>
        /// Codename
        /// </summary>
        public string Name { get; set; }

        public TeamMode Mode { get; set; }

        /// <summary>
        /// Displayed
        /// </summary>
        public string Title { get; set; }

        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public sbyte Flags { get; set; }

        public string U1 { get; set; }

        public sbyte U2 { get; set; }

        /// <summary>
        /// Player UUID
        /// </summary>
        public List<string> Players { get; set; }

        public TeamUpdate()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Name = ReadString8(r);
            Mode = (TeamMode)r.ReadByte();

            if (Mode == TeamMode.Created || Mode == TeamMode.Updated)
            {
                Title = ReadString8(r);
                Prefix = ReadString8(r);
                Suffix = ReadString8(r);
                Flags = r.ReadSByte();
                U1 = ReadString8(r);
                U2 = r.ReadSByte();
            }
            if (Mode == TeamMode.Created || Mode == TeamMode.PlayerAdded || Mode == TeamMode.PlayerRemoved)
            {
                int count = ReadVarInt(r);
                Players = new List<string>();
                for (int n = 0; n < count; n++)
                    Players.Add(ReadString8(r));
            }
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteString8(w, Name);
            w.Write((byte)Mode);
            //NOT fully tested/verified
            if (Mode == TeamMode.Created || Mode == TeamMode.Updated)
            {
                WriteString8(w, Title);
                WriteString8(w, Prefix);
                WriteString8(w, Suffix);
                w.Write((sbyte)Flags);
                WriteString8(w, U1);
                w.Write((sbyte)U2);
            }
            if (Mode == TeamMode.Created || Mode == TeamMode.PlayerAdded || Mode == TeamMode.PlayerRemoved)
            {
                WriteVarInt(w, Players.Count);
                foreach (string p in Players)
                    WriteString8(w, p);
            }
        }
    }

    public enum TeamMode
    {
        Created = 0,
        Removed = 1,
        Updated = 2,
        PlayerAdded = 3,
        PlayerRemoved = 4,
    }
}

