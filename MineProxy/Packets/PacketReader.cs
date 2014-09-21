using System;
using MiscUtil.IO;
using System.IO;
using MiscUtil.Conversion;
using MineProxy.Network;

namespace MineProxy.Packets
{
    public static class PacketReader
    {
        /// <summary>
        /// Reads the handshake.
        /// Return null if we received an old server status request
        /// </summary>
        public static byte[] ReadFirstPackage(Stream s)
        {
            int first = s.ReadByte();
            Debug.WriteLine("First byte: " + first);
            if (first < 0)
                throw new EndOfStreamException();
            if (first == 0xFE)
                return null;

            int length;
            if (first < 0x80)
                length = first;
            else
                length = (first & 0x7f) | (Packet.ReadVarInt(s) << 7);
            //Not used for Handshake packet
            //int uncompressedLength = Packet.ReadVarInt(s);
            //if (uncompressedLength != 0)
            //    throw new NotImplementedException();
            byte[] packetData = ReadBytes(s, length);
            return packetData;
        }

        /// <summary>
        /// Read without reading compressed header
        /// </summary>
        public static byte[] ReadHandshake(Stream s)
        {
            int length = Packet.ReadVarInt(s);
            var packetData = ReadBytes(s, length);
            return packetData;
        }

        public static byte[] Read(Stream s)
        {
            int compressedLength = Packet.ReadVarInt(s);
            int uncompressedLength = Packet.ReadVarInt(s);
            var packetData = ReadBytes(s, compressedLength - Packet.VarIntSize(uncompressedLength)); //Minus uncompressed varint
            return packetData;
        }

        public static byte[] Read(Stream s, out int uncompressedLength)
        {
            int compressedLength = Packet.ReadVarInt(s);
            uncompressedLength = Packet.ReadVarInt(s);
            var packetData = ReadBytes(s, compressedLength - Packet.VarIntSize(uncompressedLength)); //Minus uncompressed varint
            return packetData;
        }

        static byte[] ReadBytes(Stream s, int length)
        {
            byte[] buffer = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                    int r = s.Read(buffer, offset, length - offset);
                if (r <= 0)
                    throw new EndOfStreamException();
                offset += r;
            }
            return buffer;
        }
    }
}

