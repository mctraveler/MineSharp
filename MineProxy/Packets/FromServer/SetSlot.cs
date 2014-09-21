using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class SetSlot : PacketFromServer
    {
        public const byte ID = 0x2F;

        public override byte PacketID { get { return ID; } }
		
        public byte WindowID { get; set; }

        public short Slot { get; set; }

        public SlotItem Item;
		
        public override string ToString()
        {
            return string.Format("[SetSlot: WindowID={0}, Slot={1}, Item={2}]", WindowID, Slot, Item);
        }
		
        public SetSlot(byte window, short slot, SlotItem item)
        {
            this.WindowID = window;
            this.Slot = slot;
            this.Item = item;
        }
		
        public SetSlot()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            WindowID = r.ReadByte();
            Slot = r.ReadInt16();
            Item = SlotItem.Read(r);
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {			
            w.Write((byte)WindowID);
            w.Write((short)Slot);
            SlotItem.Write(w, Item);
        }
    }
}

