using System;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using MineProxy.Chatting;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class ChatMessageClient : PacketFromClient
    {
        public const byte ID = 0x01;

        public override byte PacketID { get { return ID; } }

        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }

        public ChatMessageClient(string text)
        {
            this.Text = text;
        }

        public ChatMessageClient()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Text = ReadString8(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteString8(w, Text);
        }
    }
}

