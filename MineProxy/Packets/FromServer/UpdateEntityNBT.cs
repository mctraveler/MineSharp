using System;
using MineProxy.Data;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class UpdateEntityNBT : PacketFromServer
    {
        public const byte ID = 0x49;

        public override byte PacketID { get { return ID; } }

        protected override void Parse(EndianBinaryReader r)
        {
            throw new NotImplementedException();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            throw new NotImplementedException();
        }
    }
}

