using System;
using MiscUtil.IO;
using System.Collections.Generic;
using System.Threading;
using MineProxy.Chatting;

namespace MineProxy.Packets
{
    public class PlayerListHeaderFooter : PacketFromServer
    {
        public const byte ID = 0x47;

        public override byte PacketID { get { return ID; } }

        public ChatJson Header { get; set; }

        public ChatJson Footer { get; set; }

        public override string ToString()
        {
            return string.Format("[PlayerListHeaderFooter: {0}, {1}]", Header, Footer);
        }

        public PlayerListHeaderFooter()
        {
        }

        //Only parsed so we can block it
        protected override void Parse(EndianBinaryReader r)
        {
            Header = ChatJson.Parse(ReadString8(r));
            Footer = ChatJson.Parse(ReadString8(r));
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteString8(w, Header.Serialize());
            WriteString8(w, Footer.Serialize());
        }
    }
}

