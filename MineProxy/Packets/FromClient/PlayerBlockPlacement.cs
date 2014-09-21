using System;
using MiscUtil.IO;
using System.IO;

namespace MineProxy.Packets
{
    public partial class PlayerBlockPlacement : PacketFromClient
    {
        public const byte ID = 0x08;

        public override byte PacketID { get { return ID; } }

        public CoordInt BlockPosition { get; set; }

        public Face FaceDirection { get; set; }

        public MineProxy.SlotItem Item { get; private set; }

        public byte CursorX { get; set; }

        public byte CursorY { get; set; }

        public byte CursorZ { get; set; }

        public override string ToString()
        {
            return base.ToString() + ": " + BlockPosition.X + ", " + BlockPosition.Y + "," + BlockPosition.Z + " " + FaceDirection + " " + Item;
        }

        public PlayerBlockPlacement()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            BlockPosition = CoordInt.Read(r);
            FaceDirection = (Face)r.ReadByte();
            Item = SlotItem.Read(r);
            CursorX = r.ReadByte();
            CursorY = r.ReadByte();
            CursorZ = r.ReadByte();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            BlockPosition.Write(w);
            w.Write((byte)FaceDirection);
            SlotItem.Write(w, this.Item);
            w.Write((byte)CursorX);
            w.Write((byte)CursorY);
            w.Write((byte)CursorZ);
        }
    }
}

