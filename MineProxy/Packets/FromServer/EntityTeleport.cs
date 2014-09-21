using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class EntityTeleport : PacketFromServer, IEntity
    {
        public const byte ID = 0x18;

        public override byte PacketID { get { return ID; } }
		
        public int EID { get; set; }
		
        public CoordDouble Position { get; set; }

        public double Yaw { get; set; }
		
        public double Pitch { get; set; }
		
        public bool Unknown { get; set; }

        public EntityTeleport()
        {
        }

        public EntityTeleport(int eid, CoordDouble pos)
        {
            this.EID = eid;
            this.Position = pos;
            this.Yaw = Math.PI / 2;
            this.Pitch = 0;
        }
		
        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Position = ReadAbsInt(r);
            Yaw = r.ReadSByte() * Math.PI / 128;
            Pitch = r.ReadSByte() * Math.PI / 128;
            Unknown = r.ReadBoolean();
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            WriteAbsInt(w, Position);
            w.Write((sbyte)(Yaw * 128 / Math.PI));
            w.Write((sbyte)(Pitch * 128 / Math.PI));
            w.Write((bool)Unknown);
        }
    }
}

