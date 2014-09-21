using System;
using MiscUtil.IO;
using System.IO;
using MiscUtil.Conversion;
using Newtonsoft.Json;

namespace MineProxy.Packets
{
    public class ServerStatusResponse : PacketFromServer
    {
        public const int ID = 0x00;

        public override byte PacketID { get { return ID; } }

        public ServerStatus Status { get; set; }

        public ServerStatusResponse(ServerStatus status)
        {
            this.Status = status;
            Prepare();
        }

        public override string ToString()
        {
            return string.Format("[ServerStatusResponse {0}]", Status);
        }

        protected override void Parse(EndianBinaryReader r)
        {
            throw new NotImplementedException();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteString8(w, Status.ToJson());
        }
    }
}

