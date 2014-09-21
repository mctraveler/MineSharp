using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class AttachEntity : PacketFromServer, IEntity
    {
        public const byte ID = 0x1B;

        public override byte PacketID { get { return ID; } }
		
        public int EID { get; set; }
		
        public int VehicleID { get; set; }
		
        public bool Leash { get; set; }

        public override string ToString()
        {
            return string.Format("[AttachEntity: EID={1}, Vehicle={2}, Leash={3}]", PacketID, EID, VehicleID, Leash);
        }
	
        public AttachEntity()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = r.ReadInt32();
            VehicleID = r.ReadInt32();
            Leash = r.ReadBoolean();
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((int)EID);
            w.Write((int)VehicleID);
            w.Write((bool)Leash);
        }
    }
}

