using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class UseEntity : PacketFromClient
    {
        public const byte ID = 0x02;

        public override byte PacketID { get { return ID; } }

        public int Target { get; set; }

        public enum Types
        {
            Interact = 0,
            Attack = 1,
            InteractAt = 2,
        }

        public Types Type { get; set; }

        public CoordDouble TargetPos { get; set; }

        public override string ToString()
        {
            return string.Format("[UseEntity: Target={0}, LeftClick={1}]", Target, Type);
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Target = ReadVarInt(r);
            Type = (Types)ReadVarInt(r);
            if (Type == Types.InteractAt)
            {
                TargetPos = new CoordDouble();
                TargetPos.X = r.ReadSingle();
                TargetPos.Y = r.ReadSingle();
                TargetPos.Z = r.ReadSingle();
            }
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, Target);
            WriteVarInt(w, (int)Type);
            if (Type == Types.InteractAt)
            {
                w.Write((float)TargetPos.X);
                w.Write((float)TargetPos.Y);
                w.Write((float)TargetPos.Z);
            }
        }
    }
}

