using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class Camera : PacketFromServer
    {
        public const byte ID = 0x43;

        public override byte PacketID { get { return ID; } }

        public int CameraID { get; set; }

        public override string ToString()
        {
            return string.Format("[Camera: PacketID={0}, CameraID={1}]", PacketID, CameraID);
        }

        protected override void Parse(EndianBinaryReader r)
        {
            CameraID = ReadVarInt(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {			
            WriteVarInt(w, CameraID);
        }
    }
}

