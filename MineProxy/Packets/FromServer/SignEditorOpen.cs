using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class SignEditorOpen : PacketFromServer
    {
        public const byte ID = 0x36;

        public override byte PacketID { get { return ID; } }
		
        public CoordInt Position { get; set; }
    
        protected override void Parse(EndianBinaryReader r)
        {
            Position = CoordInt.Read(r);
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            Position.Write(w);
        }
    }
}

