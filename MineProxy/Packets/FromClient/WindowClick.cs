using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public partial class WindowClick : PacketFromClient
    {
        public const byte ID = 0x0E;

        public override byte PacketID { get { return ID; } }
		
        public byte WindowID { get; set; }
		
        public int Slot { get; set; }
		
        public sbyte RightClick { get; set; }
		
        public short ActionID { get; set; }
		
        public sbyte Shift { get; set; }
		
        public SlotItem Item;
		
        public override string ToString()
        {
            return string.Format("[WindowClick: ID={0}, Right={1}, Slot={2}, Item={3}]", ActionID, RightClick, Slot, Item);
        }

        public WindowClick()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            WindowID = r.ReadByte();
            Slot = r.ReadInt16();
            RightClick = r.ReadSByte();
            ActionID = r.ReadInt16();
            Shift = r.ReadSByte();
            Item = SlotItem.Read(r);
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((byte)WindowID);
            w.Write((short)Slot);
            w.Write((sbyte)RightClick);
            w.Write((short)ActionID);
            w.Write((sbyte)Shift);
            SlotItem.Write(w, Item);
        }
		
    }
}

