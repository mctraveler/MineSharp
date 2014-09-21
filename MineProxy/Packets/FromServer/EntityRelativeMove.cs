using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class EntityRelativeMove : PacketFromServer, IEntity
    {
        public const byte ID = 0x15;

        public override byte PacketID { get { return ID; } }

        public int EID { get; set; }

        public CoordDouble Delta { get; set; }

        public bool OnGround { get; set; }

        public override string ToString()
        {
            return string.Format("[EntityRelativeMove: EID={1}, Delta={2}]", PacketID, EID, Delta.ToString("0.0"));
        }

        public EntityRelativeMove()
        {
        }


        const int scale = 32;

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Delta = new CoordDouble();
            Delta.X = ((double)r.ReadSByte()) / scale;
            Delta.Y = ((double)r.ReadSByte()) / scale;
            Delta.Z = ((double)r.ReadSByte()) / scale;
            OnGround = r.ReadBoolean();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {			
            WriteVarInt(w, EID);
            w.Write((sbyte)(Delta.X * scale));
            w.Write((sbyte)(Delta.Y * scale));
            w.Write((sbyte)(Delta.Z * scale));
            w.Write((bool)OnGround);
        }
    }
	
}

