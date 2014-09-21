using System;
using MineProxy.Data;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class ResourcePackStatus : PacketFromClient
    {
        public const byte ID = 0x19;

        public override byte PacketID { get { return ID; } }
		
        public override string ToString()
        {
            return string.Format("[ResourcePackResult: PacketID={0}]", PacketID);
        }

        protected override void Parse(EndianBinaryReader r)
        {
        }
        
        protected override void Prepare(EndianBinaryWriter w)
        {
        }
    }
}

