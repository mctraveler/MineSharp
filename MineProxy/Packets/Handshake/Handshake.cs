using System;
using MiscUtil.IO;
using System.IO;
using MiscUtil.Conversion;

namespace MineProxy.Packets
{
    public class Handshake : PacketFromClient
    {
        public const int ID = 0x00;

        public override byte PacketID { get { return ID; } }

        public ProtocolVersion Version { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public HandshakeState State { get; set; }        

        /// <summary>
        /// Sent to backend
        /// </summary>
        public Handshake(string host, int port, HandshakeState state)
        {
            Version = MinecraftServer.BackendVersion;
            Host = host;
            Port = port;
            State = state;
            Prepare();
        }

        //Received from client frontend
        public Handshake(byte[] buffer)
        {
            SetPacketBuffer(buffer);
            Parse();
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Version = (ProtocolVersion)ReadVarInt(r);
            Host = ReadString8(r);
            Port = r.ReadUInt16();
            State = (HandshakeState)ReadVarInt(r);
            
            DebugGotAll(r);
        }

        public override string ToString()
        {
            return string.Format("[Handshake: {0}]", Version);
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, (int)Version);
            WriteString8(w, Host);
            w.Write((ushort)Port);
            WriteVarInt(w, (int)State);
        }
    }
}

