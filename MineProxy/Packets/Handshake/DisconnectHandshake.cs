using System;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;
using MineProxy.Chatting;

namespace MineProxy.Packets
{
    public class DisconnectHandshake : PacketFromServer
    {
        public const byte ID = 0x00;

        public override byte PacketID { get { return ID; } }
		
        public ChatJson Message { get; set; }

        public override string ToString()
        {
            return string.Format("[Disconnect: Reason={0}]", Message.Serialize());
        }
		
        public DisconnectHandshake(string message)
        {
            var msg = ChatMessageServer.CreateText(message);
            this.Message = msg.Json;
            this.Message.Color = "red";
        }
		
        public DisconnectHandshake(byte[] buffer)
        {
            SetPacketBuffer(buffer);
            Parse();
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Message = ChatJson.Parse(ReadString8 (r));
            Debug.WriteLine(this);
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteString8 (w, Message.Serialize());
        }
    }
}

