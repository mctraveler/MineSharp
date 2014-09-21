using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class HeldItemServer : PacketFromServer
    {
        public const byte ID = 0x09;

        public override byte PacketID { get { return ID; } }
	
        /// <summary>
        /// 0 - 8 for the lower bar slots
        /// </summary>
        public sbyte SlotID { get; set; }
		
        public HeldItemServer()
        {           

        }
		
        public HeldItemServer(sbyte slot)
        {
            SlotID = slot;
        }

        protected override void Parse(EndianBinaryReader r)
        {
            SlotID = r.ReadSByte();

            if (SlotID < 0 && SlotID > 8)
                throw new InvalidOperationException("Invalid holding slot id: " + SlotID);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write(SlotID);
        }

    }
	
}

