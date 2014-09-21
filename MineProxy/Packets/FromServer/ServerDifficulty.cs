using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class ServerDifficulty : PacketFromServer
    {
        public const byte ID = 0x41;

        public override byte PacketID { get { return ID; } }
		
        /// <summary>
        /// 0:PEACEFUL, 1:EASY, 2:NORMAL, 3: HARD
        /// </summary>
        public byte Difficulty { get; set; }

        public override string ToString()
        {
            return string.Format("[ServerDifficulty: {0}]", Difficulty);
        }
		
        protected override void Parse(EndianBinaryReader r)
        {
            Difficulty = r.ReadByte();
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {			
            w.Write((byte)Difficulty);
        }
    }
}

