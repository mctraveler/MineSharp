using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class CollectItem : PacketFromServer, IEntity
    {
        public const byte ID = 0x0D;

        public override byte PacketID { get { return ID; } }

        public int ItemEID { get; set; }

        public int EID { get; set; }

        public override string ToString()
        {
            return string.Format("[CollectItem: ItemEID={1}, EID={2}]", PacketID, ItemEID, EID);
        }

        public CollectItem()
        {
        }

        public CollectItem(int item, int entity)
        {
            this.ItemEID = item;
            this.EID = entity;
        }

        protected override void Parse(EndianBinaryReader r)
        {
            ItemEID = ReadVarInt(r);
            EID = ReadVarInt(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, ItemEID);
            WriteVarInt(w, EID);
        }
    }
}

