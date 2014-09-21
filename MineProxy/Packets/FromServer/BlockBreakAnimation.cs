using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class BlockBreakAnimation : PacketFromServer, IEntity
    {
        public const byte ID = 0x25;

        public override byte PacketID { get { return ID; } }

        public int EID { get; set; }

        public CoordInt Position { get; set; }

        /// <summary>
        /// 0 - 9
        /// </summary>
        public byte DestroyStage { get; set; }

        public override string ToString()
        {
            return string.Format("[BlockAnimation: {0}, {1}]", Position, DestroyStage);
        }

        public BlockBreakAnimation()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Position = CoordInt.Read(r);
            DestroyStage = r.ReadByte();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            Position.Write(w);
            w.Write((byte)DestroyStage);
        }
    }
}

