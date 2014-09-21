using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public partial class PlayerDigging : PacketFromClient
    {
        public const byte ID = 0x07;

        public override byte PacketID { get { return ID; } }

        public enum StatusEnum
        {
            StartedDigging = 0,
            Unknown1 = 1,
            FinishedDigging = 2,
            DropStack = 3,
            DropItem = 4,
            /// <summary>
            /// and finish eating
            /// </summary>
            ShootArrow = 5,
        }

        public StatusEnum Status { get; set; }

        public CoordInt Position { get; set; }

        public Face Face { get; set; }

        public override string ToString()
        {
            return string.Format("[PlayerDigging: {1}, {2}, {3}]", PacketID, Status, Position, Face);
        }

        public PlayerDigging()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Status = (StatusEnum)r.ReadByte();
            Position = CoordInt.Read(r);
            Face = (Face)r.ReadByte();

#if DEBUG
            if (Status.ToString() == ((int)Status).ToString())
                throw new NotImplementedException(Status.ToString());
            if (Face.ToString() == ((int)Face).ToString())
                throw new NotImplementedException(Face.ToString());
#endif
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((byte)Status);
            Position.Write(w);
            w.Write((sbyte)Face);
        }
    }
}

