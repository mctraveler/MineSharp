using System;
using System.Collections.Generic;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace MineProxy.Packets
{
    /// <summary>
    /// Buffer with one or several precompiled packets
    /// </summary>
    public class PrecompiledPacket : PacketFromServer
    {
        public PrecompiledPacket(byte[] data)
        {
            if (data.Length == 0)
                throw new ArgumentException("Empty list");

            SetPacketBuffer(data, 0);
        }

        public PrecompiledPacket(List<MultiBlockChange> packets)
        {
            if (packets.Count == 0)
                throw new ArgumentException("Empty list");

            using (var ms = new MemoryStream())
            using (var w = new EndianBinaryWriter(EndianBitConverter.Big, ms))
            {
                foreach (var m in packets)
                {
                    //Buffer to memory stream
                    if (m.PacketBuffer == null)
                        m.Prepare();
                    Packet.WriteVarInt(w, m.PacketBuffer.Length);
                    w.Write(m.PacketBuffer);
                }
                w.Flush();
                SetPacketBuffer(ms.ToArray());
            }
            if (PacketBuffer.Length == 0)
                throw new ArgumentException("Empty compiled packet");
        }

        public PrecompiledPacket(List<PacketFromServer> packets)
        {
            if (packets.Count == 0)
                throw new ArgumentException("Empty list");

            using (var ms = new MemoryStream())
            using (var w = new EndianBinaryWriter(EndianBitConverter.Big, ms))
            {
                foreach (var m in packets)
                {
                    //Buffer to memory stream
                    if (m.PacketBuffer == null)
                        m.Prepare();
                    Packet.WriteVarInt(w, m.PacketBuffer.Length);
                    w.Write(m.PacketBuffer);
                }
                w.Flush();
                SetPacketBuffer(ms.ToArray());
            }
            if (PacketBuffer.Length == 0)
                throw new ArgumentException("Empty compiled packet");
        }

        public override byte PacketID
        {
            get
            {
                return PacketBuffer [0];
            }
        }

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

