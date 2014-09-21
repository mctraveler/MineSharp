using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class SpawnExperienceOrb : PacketFromServer,IEntity
    {
        public const byte ID = 0x11;

        public override byte PacketID { get { return ID; } }

        public int EID { get; set; }

        public CoordDouble Position;
        public short Count;

        public override string ToString()
        {
            return string.Format("[ExperienceOrb: {0}]", Count);
        }

        public SpawnExperienceOrb()
        {
            Position = new CoordDouble();
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Position = ReadAbsInt(r);
            Count = r.ReadInt16();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            WriteAbsInt(w, Position);
            w.Write(Count);
        }
    }
}
