using System;
using MiscUtil.IO;
using System.Collections.Generic;

namespace MineProxy.Packets
{
    public class WindowOpen : PacketFromServer
    {
        public const byte ID = 0x2D;

        public override byte PacketID { get { return ID; } }

        public byte WindowID { get; set; }

        #if DEBUG
        static List<string> InventoryTypes = new List<string>()
        {
            "minecraft:enchanting_table",
            "minecraft:container",
            "minecraft:villager",
            "minecraft:crafting_table",
            "minecraft:chest",
            "minecraft:furnace",
            "minecraft:anvil",
        };
        #endif

        public string InventoryType { get; set; }

        public string WindowTitle { get; set; }

        public byte NumberOfSlots { get; set; }

        public int Unknown { get; set; }

        public override string ToString()
        {
            return string.Format("[WindowOpen: PacketID={0}, WindowID={1}, InventoryType={2}, WindowTitle={3}, NumberOfSlots={4}, Unknown={5}]", PacketID, WindowID, InventoryType, WindowTitle, NumberOfSlots, Unknown);
        }

        public WindowOpen()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            WindowID = r.ReadByte();
            InventoryType = ReadString8(r);
            WindowTitle = ReadString8(r);
            NumberOfSlots = r.ReadByte();
            if (InventoryType == "EntityHorse")
                Unknown = r.ReadInt32();
#if DEBUG
            Console.WriteLine("Horse Window Unknown: " + Unknown);
            if (InventoryTypes.Contains(InventoryType) == false)
            {
                Console.WriteLine("Unknown InventoryType: " + InventoryType);
                throw new NotImplementedException(InventoryType);
            }
#endif
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((byte)WindowID);
            WriteString8(w, InventoryType);
            WriteString8(w, WindowTitle);
            w.Write(NumberOfSlots);
            if (InventoryType == "EntityHorse")
                w.Write((int)Unknown);
        }
    }
}

