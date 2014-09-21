using System;
using MineProxy.Data;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class ResourcePackSend : PacketFromServer
    {
        public const byte ID = 0x48;

        public override byte PacketID { get { return ID; } }

        public string URL { get; set; }

        /// <summary>
        /// If it's not a 40 character hexadecimal string, the client will not use it for hash verification and likely waste bandwidth - but it will still treat it as a unique id 
        /// </summary>
        public string Hash { get; set; }

        public override string ToString()
        {
            return string.Format("[ResourcePackSend: {0}, {1}]", URL, Hash);
        }

        protected override void Parse(EndianBinaryReader r)
        {
            URL = ReadString8(r);
            Hash = ReadString8(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteString8(w, URL);
            WriteString8(w, Hash);
        }
    }
}

