using System;
using MiscUtil.IO;
using System.IO;
using MiscUtil.Conversion;

namespace MineProxy.Packets
{
    public class EncryptionRequest : PacketFromServer
    {
        public const int ID = 0x01;

        public override byte PacketID { get { return ID; } }

        public string ServerID { get; set; }

        public byte[] PublicKey { get; set; }

        public byte[] VerifyToken { get; set; }

        public override string ToString()
        {
            return string.Format("[EncryptionRequest: PublicKey={0}, VerifyToken={1}]", PublicKey.Length, VerifyToken.Length);
        }

        public EncryptionRequest(string serverID, byte[] pubKey)
        {
            this.ServerID = serverID;   
            this.PublicKey = pubKey;
            this.VerifyToken = new byte[4];
            Prepare();
        }

        public EncryptionRequest(byte[] buffer)
        {
            SetPacketBuffer(buffer);
            Parse();
        }

        protected override void Parse(EndianBinaryReader r)
        {
            ServerID = ReadString8(r);
            int length = ReadVarInt(r);
            if (length != 294)
            {
                throw new InvalidDataException("Public key size mismatch");
            }
            PublicKey = r.ReadBytesOrThrow(length);
            length = ReadVarInt(r);
            if(length != 4)
                throw new InvalidDataException("Public key size mismatch");
            VerifyToken = r.ReadBytesOrThrow(length);

            DebugGotAll(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteString8(w, ServerID);
            WriteVarInt(w, PublicKey.Length);
            w.Write(PublicKey);	
            WriteVarInt(w, VerifyToken.Length);
            w.Write(VerifyToken);         
        }
    }
}

