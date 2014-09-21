using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    /// <summary>
    /// Coordinate of bed position, the location the compass points at
    /// </summary>
    public class SpawnPosition : PacketFromServer
    {
        public const byte ID = 0x05;

        public override byte PacketID { get { return ID; } }

        public CoordInt Position { get; set; }

        public override string ToString()
        {
            return string.Format("[SpawnPosition: {0}]", Position);
        }

        public SpawnPosition()
        {

        }

        public SpawnPosition(CoordInt pos)
        {
            this.Position = pos;
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Position = CoordInt.Read(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            Position.Write(w);
        }
    }
}

