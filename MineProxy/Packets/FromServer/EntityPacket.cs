using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public sealed class EntityPacket : PacketFromServer, IEntity
    {
        public override byte PacketID { get { return 0x1E; } }
		
        public int EID { get; set; }
		
        public override string ToString()
        {
            return base.ToString() + ": " + EID;
        }
		
        public EntityPacket()
        {
        }
		
        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
        }
    }
}

