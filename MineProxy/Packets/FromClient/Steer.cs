using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class Steer : PacketFromClient
    {
        public const byte ID = 0x0C;

        public override byte PacketID { get { return ID; } }

        public float SideWays { get; set; }
        public float Forward { get; set; }
        /// <summary>
        /// 1 = jump, 2 = unmount
        /// </summary>
        public Actions Action { get; set; }

        public enum Actions
        {
            Jump = 1,
            Unmount = 2,
        }

        public override string ToString()
        {
            return string.Format("[Steer: PacketID={0}, SideWays={1}, Forward={2}, Action={3}]", PacketID, SideWays, Forward, Action);
        }

        public Steer()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            SideWays = r.ReadSingle();
            Forward = r.ReadSingle();
            Action = (Actions)r.ReadByte();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write(SideWays);
            w.Write(Forward);
            w.Write((byte)Action);
        }

    }
}

