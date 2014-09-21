using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class EntityVelocity : PacketFromServer, IEntity
    {
        public const byte ID = 0x12;

        public override byte PacketID { get { return ID; } }

        public int EID { get; set; }

        /// <summary>
        /// Blocks per second
        /// </summary>
        public CoordDouble Velocity { get; set; }

        public override string ToString()
        {
            return string.Format("[EntityVelocity: {0}: {1}]", EID, Velocity);
        }

        public EntityVelocity()
        {
        }

        /// <summary>
        /// 1/8000 block per 50ms
        /// </summary>
        const double scale = 8000 / (1000 / 50);

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Velocity = new CoordDouble();
            Velocity.X = ((double)r.ReadInt16()) / scale;
            Velocity.Y = ((double)r.ReadInt16()) / scale;
            Velocity.Z = ((double)r.ReadInt16()) / scale;
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            w.Write((short)(Velocity.X * scale));
            w.Write((short)(Velocity.Y * scale));
            w.Write((short)(Velocity.Z * scale));
        }
    }
}

