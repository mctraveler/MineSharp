using System;
using System.Collections.Generic;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class WindowItems : PacketFromServer
    {
        public const byte ID = 0x30;

        public override byte PacketID { get { return ID; } }

        public byte WindowID { get; set; }
		
        public SlotItem[] Items { get; set; }
		
        public WindowItems(byte windowID, int itemCount)
        {
            this.WindowID = windowID;
            this.Items = new SlotItem[itemCount];
        }
		
        public override string ToString()
        {
            return string.Format("[WindowItems: WindowID={1}, Items={2}]", PacketID, WindowID, Items.Length);
        }
		
        public WindowItems()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            WindowID = r.ReadByte();
            int count = r.ReadInt16();
            Items = new SlotItem [count];
            for (int n = 0; n < Items.Length; n++)
            {
                Items [n] = SlotItem.Read(r);
#if DEBUG
                //Console.WriteLine("Slot " + n + ": " + Items [n]);
#endif
            }
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write(WindowID);
            w.Write((short)Items.Length);
            for (int n = 0; n < Items.Length; n++)
            {
                SlotItem.Write(w, Items [n]);
            }
        }
		
    }
}

