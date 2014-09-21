using System;
using MiscUtil.IO;
using MineProxy.Chatting;

namespace MineProxy.Packets
{
    public class UpdateSignClient : PacketFromClient
    {
        public const byte ID = 0x12;

        public override byte PacketID { get { return ID; } }

        public CoordInt Position { get; set; }

        public ChatJson Text1 { get; set; }

        public ChatJson Text2 { get; set; }

        public ChatJson Text3 { get; set; }

        public ChatJson Text4 { get; set; }

        [Obsolete("Not tested")]
        public UpdateSignClient()
        {
            Position = new CoordInt();
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Position = CoordInt.Read(r);
            try
            {
                Text1 = ChatJson.Parse(ReadString8(r));
                Text2 = ChatJson.Parse(ReadString8(r));
                Text3 = ChatJson.Parse(ReadString8(r));
                Text4 = ChatJson.Parse(ReadString8(r));
            } 
            #if !DEBUG
            catch (Exception ex)
            {
            Log.WriteServer (ex);
            }
            #endif
            finally
            {
            }      	
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            Position.Write(w);
            WriteString8(w, Text1.Serialize());
            WriteString8(w, Text2.Serialize());
            WriteString8(w, Text3.Serialize());
            WriteString8(w, Text4.Serialize());
        }
    }
}

