using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    /// <summary>
    /// This packet is only sent by the server but we accept it from clients too
    /// because the HTTP request "GET " starts with it.
    /// </summary>
    public class SpawnGlobalEntity : PacketFromServer, IEntity
    {
        public const byte ID = 0x2C;

        public override byte PacketID { get { return ID; } }

        /// <summary>
        /// Entity being hit
        /// </summary>
        public int EID { get; set; }

        /// <summary>
        /// 1 == thunderbolt
        /// </summary>
        public sbyte Type { get; set; }

        public CoordDouble Position { get; set; }

        public override string ToString()
        {
            return string.Format("[Thunderbolt: EID={0}, Pos={1}]", EID, Position);
        }

        public SpawnGlobalEntity(CoordDouble pos)
        {
            Position = pos;
            Type = 1;
        }

        public SpawnGlobalEntity()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Type = r.ReadSByte();
            Position = new CoordDouble();
            Position.X = ((double)r.ReadInt32()) / 32;
            Position.Y = ((double)r.ReadInt32()) / 32;
            Position.Z = ((double)r.ReadInt32()) / 32;
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            w.Write((sbyte)Type);
            w.Write((int)(Position.X * 32));
            w.Write((int)(Position.Y * 32));
            w.Write((int)(Position.Z * 32));
        }
    }
}

