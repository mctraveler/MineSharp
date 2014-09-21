using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class CreativeInventory : PacketFromClient
    {
        public const byte ID = 0x10;

        public override byte PacketID { get { return ID; } }
		
        public short Slot { get; set; }
		
        public SlotItem Item { get; set; }
		
        public override string ToString()
        {
            return string.Format("[CreativeInventory: Slot={0}, Item={1}]", Slot, Item);
        }
		
        public CreativeInventory()
        {
        }
		
        protected override void Parse(EndianBinaryReader r)
        {
            Slot = r.ReadInt16();
            Item = SlotItem.Read(r);
        }
				
        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write(Slot);
            SlotItem.Write(w, Item);
        }
    }
}

