using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class SpawnMob : PacketFromServer
    {
        public const byte ID = 0x0F;

        public override byte PacketID { get { return ID; } }
        
        public int EID;
        
        public MobType Type { get; set; }

        public CoordDouble Pos { get; set; }

        public double Yaw { get; set; }
        
        public double Pitch { get; set; }
        
        public byte HeadYaw { get; set; }

        public short UX { get; set; }

        public short UY { get; set; }

        public short UZ { get; set; }

        public Metadata Metadata { get; set; }
        
        public override string ToString()
        {
            return string.Format("[SpawnMob: {0}[{1}] at ({2}, {3}, {4}), {5}, {6}]", Type, EID, Pos.X.ToString("0"), Pos.Y.ToString("0"), Pos.Z.ToString("0"), HeadYaw, Metadata);
        }

        public SpawnMob()
        {
        }

        public SpawnMob(MobType type)
        {
            Type = type;
            Pos = new CoordDouble();
            Metadata = new Metadata();
            Metadata.SetByte(17, 0);
            Metadata.SetByte(0, 0);
            Metadata.SetByte(16, -1);
            Metadata.SetShort(1, 300);
            Metadata.SetFloat(6, 20);
            Metadata.SetInt(7, 0);
            Metadata.SetByte(8, 0);
            Metadata.SetByte(9, 0);
            Metadata.SetString(10, "");
            Metadata.SetByte(11, 0);
        }
        
        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Type = (MobType)r.ReadByte();
            Pos = ReadAbsInt(r);
            Yaw = r.ReadSByte() * Math.PI / 128;
            Pitch = r.ReadSByte() * Math.PI / 128;
            HeadYaw = r.ReadByte();
            UX = r.ReadInt16();
            UY = r.ReadInt16();
            UZ = r.ReadInt16();
            Metadata = Metadata.Read(r);
#if DEBUG
            if (Type.ToString() == ((int)Type).ToString())
                throw new NotImplementedException(Type.ToString());
#endif
        }
        
        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            w.Write((byte)Type);

            WriteAbsInt(w, Pos);
            w.Write((sbyte)(Yaw * 128 / Math.PI));
            w.Write((sbyte)(Pitch * 128 / Math.PI));
            w.Write((byte)HeadYaw);
            w.Write((short)UX);
            w.Write((short)UY);
            w.Write((short)UZ);
            Metadata.Write(w, this.Metadata);
        }
    }
}

