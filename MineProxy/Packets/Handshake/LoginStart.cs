using System;
using MiscUtil.IO;
using System.IO;
using MiscUtil.Conversion;

namespace MineProxy.Packets
{
    public class LoginStart : PacketFromClient
    {
        public const int ID = 0x00;

        public override byte PacketID { get { return ID; } }

        public string Name { get; set; }

        public LoginStart(string name)
        {
            Name = name;
            Prepare();
        }

        public override string ToString()
        {
            return string.Format("[LoginStart: {0}]", Name);
        }

        public LoginStart(byte[] buffer)
        {
            SetPacketBuffer(buffer);
            Parse();
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Name = ReadString8(r);

            DebugGotAll(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteString8(w, Name);
        }
    }
}

