using System;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace MineProxy.Packets
{
    public class PrecompiledPacketStream : IDisposable
    {
        MemoryStream ms;
        EndianBinaryWriter w;

        /// <summary>
        /// Add more packets using Add, but not after packet has been sent
        /// </summary>
        public PrecompiledPacketStream()
        {
            ms = new MemoryStream();
            w = new EndianBinaryWriter(EndianBitConverter.Big, ms);
        }

        public void Add(PacketFromServer m)
        {
            //Buffer to memory stream
            if (m.PacketBuffer == null)
                m.Prepare();
            Packet.WriteVarInt(w, m.PacketBuffer.Length);
            w.Write(m.PacketBuffer, 0, m.PacketBuffer.Length);
        }

        /// <summary>
        /// Return null if no data
        /// </summary>
        public PrecompiledPacket Compile()
        {
            w.Flush();
            byte[] data = ms.ToArray();
            if (data.Length == 0)
                return null;
            return new PrecompiledPacket(data);
        }

        public void Dispose()
        {
            w.Dispose();
            ms.Dispose();
        }

    }
}

