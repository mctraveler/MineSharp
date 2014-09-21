using System;
using MineProxy.Data;
using MiscUtil.IO;
using MineProxy.Chatting;

namespace MineProxy.Packets
{
    /// <summary>
    /// Special packet that will not try to parse and pass bytes as they are sent
    /// </summary>
    public class PassThroughClient : PacketFromClient
    {
        public override byte PacketID { get { return PassThrough.ID; } }

        public override string ToString()
        {
            return string.Format("[PassThrough: 0x{0:X2}, {1} bytes]", PacketBuffer[0], PacketBuffer.Length);
        }

        protected override void Parse(EndianBinaryReader r)
        {
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            throw new InvalidOperationException();
        }
    }
}

