using System;
using MiscUtil.IO;


namespace MineProxy.Packets
{
    public class ClientSettings : PacketFromClient
    {
        public const byte ID = 0x15;

        public override byte PacketID { get { return ID; } }

        public string Locale { get; set; }

        /// <summary>
        /// View distance in chunks
        /// </summary>
        public byte ViewDistance { get; set; }

        /// <summary>
        /// Chat Enabled: Bits 0-1. 00: Enabled. 01: Commands only. 10: Hidden.
        /// </summary>
        public byte ChatFlags { get; set; }

        public bool ChatColors { get; set; }

        public byte DisplayedSkinParts { get; set; }

        protected override void Parse(EndianBinaryReader r)
        {
            Locale = ReadString8(r);
            ViewDistance = r.ReadByte();
            ChatFlags = r.ReadByte();
            ChatColors = r.ReadBoolean();
            DisplayedSkinParts = r.ReadByte();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteString8(w, Locale);
            w.Write((byte)ViewDistance);
            w.Write((byte)ChatFlags);
            w.Write((bool)ChatColors);
            w.Write((byte)DisplayedSkinParts);
        }
    }
}

