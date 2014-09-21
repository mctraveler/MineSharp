using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class EntityEquipment : PacketFromServer, IEntity
    {
        public const byte ID = 0x04;

        public override byte PacketID { get { return ID; } }
        
        public int EID { get; set; }
        
        /// <summary>
        /// Held=0, 1-4=Armor slots, 1=boot, 2=leg, 3=chest, 4=head
        /// </summary>
        public short Slot { get; set; }

        /// <summary>
        /// No item == -1
        /// </summary>
        public SlotItem Item { get; set; }

        public override string ToString()
        {
            return string.Format("[EntityEquipment: Slot {0}, {1}]", Slot, Item);
        }

        /// <param name='slot'>
        /// Hand=0, Foot=1 ... Head=4
        /// </param>
        public EntityEquipment(int eid, int slot, SlotItem item)
        {
            this.EID = eid;
            this.Slot = (short)slot;
            this.Item = item;
        }

        public EntityEquipment()
        {

        }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Slot = r.ReadInt16();
            Item = SlotItem.Read(r);
        }
        
        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            w.Write((short)Slot);
            SlotItem.Write(w, Item);
        }
    }
}

