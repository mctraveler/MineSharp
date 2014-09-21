using System;
using MineProxy.Data;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class AnimationClient : PacketFromClient
    {
        public const byte ID = 0x0A;

        public override byte PacketID { get { return ID; } }
		
        public override string ToString()
        {
            return string.Format("[AnimationClient: PacketID={0}]", PacketID);
        }

        public AnimationClient()
        {

        }

        protected override void Parse(EndianBinaryReader r)
        {
        }
        
        protected override void Prepare(EndianBinaryWriter w)
        {
        }
    }
}

