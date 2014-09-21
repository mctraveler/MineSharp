using System;
using MiscUtil.IO;
using System.IO;
using MiscUtil.Conversion;

namespace MineProxy.Packets
{
    public class LoginSuccess : PacketFromServer
    {
        public const int ID = 0x02;

        public override byte PacketID { get { return ID; } }

        public Guid UUID { get; set; }

        public string Username { get; set; }

        public LoginSuccess(Guid uuid, string username)
        {
            this.UUID = uuid;
            this.Username = username;
            Prepare();
        }

        public LoginSuccess(byte[] buffer)
        {
            SetPacketBuffer(buffer);
            Parse();
        }

        protected override void Parse(EndianBinaryReader r)
        {
            UUID = new Guid(ReadString8(r));
            Username = ReadString8(r);
            DebugGotAll(r);
        }

        public override string ToString()
        {
            return string.Format("[LoginSuccess: {0}, {1}]", Username, UUID);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteString8(w, UUID.ToString());
            WriteString8(w, Username);
        }
    }
}

