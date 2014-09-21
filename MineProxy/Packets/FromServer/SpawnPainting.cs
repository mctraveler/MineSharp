using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class SpawnPainting : PacketFromServer, IEntity
    {
        public const byte ID = 0x10;

        public override byte PacketID { get { return ID; } }

        public int EID { get; set; }

        public string Title { get; set; }

        public CoordInt Position { get; set; }

        public Face FaceDirection { get; set; }

        public enum Face
        {
            NegZ = 0,
            NegX = 1,
            PosZ = 2,
            PosX = 3,
        }

        public SpawnPainting()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Title = ReadString8(r);
            Position = CoordInt.Read(r);
            FaceDirection = (Face)r.ReadByte();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            WriteString8(w, Title);
            Position.Write(w);
            w.Write((byte)FaceDirection);
        }
    }
}

