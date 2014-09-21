using System;
using MiscUtil.IO;
using System.IO;

namespace MineProxy.Packets
{
    public class Maps : PacketFromServer
    {
        public const int ID = 0x34;
        public override byte PacketID { get { return ID; } }
		
        public byte[] Data { get; set; }
		
        protected override void Parse(EndianBinaryReader r)
        {
            Data = r.ReadBytes((int)(r.BaseStream.Length - r.BaseStream.Position));
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write(Data);
        }
    }
}

