using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    /// <summary>
    /// Response to server ping
    /// </summary>
    public class KeepAlivePong : PacketFromClient
    {
        public const byte ID = 0x00;

        public override byte PacketID { get { return ID; } }

        public int KeepAliveID { get; set; }

        public override string ToString()
        {
            return string.Format("[KeepAlive: {0}]", KeepAliveID);
        }

        public KeepAlivePong(int id)
        {
            this.KeepAliveID = id;
        }

        public KeepAlivePong()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            KeepAliveID = ReadVarInt(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, KeepAliveID);
        }
    }
}

