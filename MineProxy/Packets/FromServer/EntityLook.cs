using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class EntityLook : PacketFromServer, IEntity
    {
        public const byte ID = 0x16;

        public override byte PacketID { get { return ID; } }

        public int EID { get; set; }

        public double Yaw { get; set; }

        public double Pitch { get; set; }

        public bool Unknown { get; set; }

        public EntityLook()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Yaw = r.ReadSByte() * Math.PI / 128;
            Pitch = r.ReadSByte() * Math.PI / 128;
            Unknown = r.ReadBoolean();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            w.Write((sbyte)(Yaw * 128 / Math.PI));
            w.Write((sbyte)(Pitch * 128 / Math.PI));
            w.Write((bool)Unknown);
        }
    }
	
}

