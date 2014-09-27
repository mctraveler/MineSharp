using System;
using MiscUtil.IO;
using System.Text;
using System.IO;
using MiscUtil.Conversion;
using System.Diagnostics;
using MineProxy.Packets.Plugins;
using Ionic.Zlib;

namespace MineProxy.Packets
{
    public abstract class Packet
    {
        public abstract byte PacketID { get; }

        public byte[] PacketBuffer { get; private set; }

        public int PacketBufferUncompressedSize { get; private set; }

        public void SetPacketBuffer(byte[] buffer, int uncompressedLength)
        {
            this.PacketBuffer = buffer;
            this.PacketBufferUncompressedSize = uncompressedLength;
        }

        public void SetPacketBuffer(byte[] buffer)
        {
            this.PacketBuffer = buffer;
            this.PacketBufferUncompressedSize = 0;
        }

        /// <summary>
        /// Parse the buffer into class parameters
        /// </summary>
        protected abstract void Parse(EndianBinaryReader r);

        /// <summary>
        /// Parse the buffer data into the packet properties
        /// </summary>
        public void Parse()
        {
            if (PacketBufferUncompressedSize != 0)
            {
                PacketBuffer = Compression.Decompress(PacketBuffer, PacketBufferUncompressedSize);
                PacketBufferUncompressedSize = 0;
            }

            var packetStream = new MemoryStream(PacketBuffer);
            int parsedType = ReadVarInt(packetStream);
            if (parsedType != PacketID)
                throw new InvalidOperationException("parsed PacketID mismatch, got " + parsedType + " expected " + PacketID);
            var pr = new EndianBinaryReader(EndianBitConverter.Big, packetStream);
            Parse(pr);
            #if DEBUGPACKET
            if (pr.BaseStream.Position != PacketBuffer.Length)
            {
                switch (PacketID)
                {
                    case PlayerListItem.ID:
                        break;
                    default:
                        if (this is PacketFromServer)
                        {
                            throw new InvalidProgramException("Did not read from server " + this.GetType().Name + " all the way");
                        }
                        else if (this is PacketFromClient)
                        {
                            throw new InvalidProgramException("Did not read from client " + PacketID.ToString("X") + " all the way");
                        }
                        else
                            throw new NotImplementedException();
                }
            }
            #endif
        }

        protected void SkipParse(EndianBinaryReader r)
        {
            r.BaseStream.Seek(0, SeekOrigin.End);
        }

        protected void SkipPrepare(EndianBinaryWriter w)
        {
            w.Write(PacketBuffer, 1, PacketBuffer.Length - 1);
        }

        protected abstract void Prepare(EndianBinaryWriter w);

        /// <summary>
        /// Prepare the buffer for sending
        /// </summary>
        public void Prepare()
        {
            var stream = new MemoryStream();
            var w = new EndianBinaryWriter(EndianBitConverter.Big, stream);
            WriteVarInt(w, PacketID);
            Prepare(w);
            PacketBuffer = stream.ToArray();
            PacketBufferUncompressedSize = 0;
        }

        public override string ToString()
        {
            return "[0x" + PacketID.ToString("X2") + "] " + GetType().Name;
        }

        #region VarInt

        public static int ReadVarInt(EndianBinaryReader r)
        {
            int val = 0;
            for (int n = 0; n < 5; n++)
            {
                byte b = r.ReadByte();

                //Check that it fits in 32 bits
                if ((n == 4) && (b & 0xF0) != 0)
                    throw new InvalidDataException("Too big for 32bits");
                //End of check

                if ((b & 0x80) == 0)
                    return val | b << (7 * n);

                val |= (b & 0x7F) << (7 * n);
            }

            throw new InvalidDataException("Too big");
        }

        public static long ReadVarLong(EndianBinaryReader r)
        {
            long val = 0;
            for (int n = 0; n < 10; n++)
            {
                byte b = r.ReadByte();

                //Check that it fits in 32 bits
                if ((n == 9) && (b & 0xFE) != 0)
                    throw new InvalidDataException("Too big for 64bits");
                //End of check

                if ((b & 0x80) == 0)
                    return val | (long)b << (7 * n);

                val |= ((long)b & 0x7F) << (7 * n);
            }

            throw new InvalidDataException("Too big");
        }

        /// <summary>
        /// Read packet length
        /// </summary>
        /// <returns>The variable int.</returns>
        /// <param name="s">S.</param>
        public static int ReadVarInt(Stream s)
        {
            int val = 0;
            for (int n = 0; n < 5; n++)
            {
                int b = s.ReadByte();
                if (b < 0)
                    throw new EndOfStreamException("Stream ended too early");

                //Check that it fits in 32 bits
                if ((n == 4) && (b & 0xF0) != 0)
                    throw new InvalidDataException("Too big for 32bits");
                //End of check

                if ((b & 0x80) == 0)
                    return val | b << (7 * n);

                val |= (b & 0x7F) << (7 * n);
            }

            throw new InvalidDataException("Too big");
        }

        public static void WriteVarInt(EndianBinaryWriter w, int ival)
        {
            byte[] buffer = new byte[5];
            int count = 0;

            uint val = (uint)ival;
            while (true)
            {
                buffer[count] = (byte)(val & 0x7F);
                val = val >> 7;
                if (val == 0)
                    break;

                buffer[count] |= 0x80;

                count += 1;
            }

            w.Write(buffer, 0, count + 1);
        }

        /// <summary>
        /// Return the size of the int encoded in varint
        /// </summary>
        public static int VarIntSize(int ival)
        {
            uint val = (uint)ival;

            int size = 0;
            while (true)
            {
                size += 1;
                val = val >> 7;
                if (val == 0)
                    return size;
            }
        }

        public static void WriteVarInt(Stream w, int ival)
        {
            byte[] buffer = new byte[5];
            int count = 0;

            uint val = (uint)ival;
            while (true)
            {
                buffer[count] = (byte)(val & 0x7F);
                val = val >> 7;
                if (val == 0)
                    break;

                buffer[count] |= 0x80;

                count += 1;
            }

            w.Write(buffer, 0, count + 1);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Minecraft protocol encoded string
        /// </summary>
        public static string ReadString8(EndianBinaryReader r)
        {
            int length = ReadVarInt(r);
            byte[] buffer = r.ReadBytesOrThrow(length);
            string message = Encoding.UTF8.GetString(buffer);
            return message;
        }

        /// <summary>
        /// Minecraft protocol encoded string
        /// </summary>
        public static void WriteString8(EndianBinaryWriter writer, string message)
        {   
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            WriteVarInt(writer, buffer.Length);
            writer.Write(buffer);
        }

        protected static CoordDouble ReadAbsInt(EndianBinaryReader r)
        {
            CoordDouble Position = new CoordDouble();
            Position.X = ((double)r.ReadInt32()) / 32;
            Position.Y = ((double)r.ReadInt32()) / 32;
            Position.Z = ((double)r.ReadInt32()) / 32;
            return Position;
        }

        protected static void WriteAbsInt(EndianBinaryWriter writer, CoordDouble Position)
        {
            writer.Write((int)(Position.X * 32));
            writer.Write((int)(Position.Y * 32));
            writer.Write((int)(Position.Z * 32));
        }

        protected static CoordDouble ReadInt8(EndianBinaryReader r)
        {
            CoordDouble Position = new CoordDouble();
            Position.X = ((double)r.ReadInt32()) / 8;
            Position.Y = ((double)r.ReadInt32()) / 8;
            Position.Z = ((double)r.ReadInt32()) / 8;
            return Position;
        }

        protected static void WriteInt8(EndianBinaryWriter writer, CoordDouble Position)
        {
            writer.Write((int)(Position.X * 8));
            writer.Write((int)(Position.Y * 8));
            writer.Write((int)(Position.Z * 8));
        }

        #endregion

        #region Debuggers

        /// <summary>
        /// Throw an exception if not all data was received
        /// </summary>
        [Conditional("DEBUG")]
        public static void DebugGotAll(EndianBinaryReader r)
        {
            #if DEBUG
            var s = r.BaseStream;
            if (s.Length != s.Position)
                throw new InvalidOperationException();
            #endif
        }

        #endregion

    }
}

