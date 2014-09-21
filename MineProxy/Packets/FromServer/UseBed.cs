using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class UseBed : PacketFromServer, IEntity
    {
        public const byte ID = 0x0A;

        public override byte PacketID { get { return ID; } }
		
        public int EID { get; set; }
		
        public CoordInt Position { get; set; }

        public override string ToString()
        {
            return string.Format("[UseBed: EID={0}, {1}]", EID, Position);
        }
		
        public UseBed()
        {
            Position = new CoordInt();
        }
		
        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Position = CoordInt.Read(r);
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            Position.Write(w);
        }
    }
}

