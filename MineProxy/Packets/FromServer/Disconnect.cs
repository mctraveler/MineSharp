using System;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;
using MineProxy.Chatting;

namespace MineProxy.Packets
{
    public class Disconnect : PacketFromServer
    {
        public const byte ID = 0x40;

        public override byte PacketID { get { return ID; } }
		
        public ChatJson Reason { get; set; }

        public override string ToString()
        {
            return string.Format("[Disconnect: Reason={0}]", Reason);
        }
		
        public Disconnect(string message)
        {
            this.Reason = new ChatJson();
            this.Reason.Text = message;
        }
		
        public Disconnect()
        {

        }

        protected override void Parse(EndianBinaryReader r)
        {
            Reason = ChatJson.Parse(ReadString8(r));
            Debug.WriteLine(this);
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteString8(w, Reason.Serialize());
        }
    }
}

