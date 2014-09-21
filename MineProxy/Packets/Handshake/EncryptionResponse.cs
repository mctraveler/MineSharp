using System;
using MineProxy.Network;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class EncryptionResponse : PacketFromClient
    {
        public const int ID = 0x01;

        public override byte PacketID { get { return ID; } }

        public byte[] SharedKey { get; set; }

        public byte[] Test { get; set; }

        public override string ToString()
        {
            return string.Format("[EncryptionResponse: SharedKey={0}]", SharedKey.Length);
        }

        [Obsolete("Used?")]
        public EncryptionResponse(CryptoMC crypto)
        {
            this.SharedKey = crypto.SharedKeyEncrypted;
            this.Test = crypto.TestEncrypted;
        }

        public EncryptionResponse(byte[] buffer)
        {
            SetPacketBuffer(buffer);
            Parse();
        }

        protected override void Parse(EndianBinaryReader r)
        {
            int length = ReadVarInt(r);
            if(length != 256)
                throw new ProtocolException("Expected 256 bytes key got " + length);
            SharedKey = r.ReadBytesOrThrow(length);
            length = ReadVarInt(r);
            if(length != 256)
                throw new ProtocolException("Expected 256 bytes test got " + length);
            Test = r.ReadBytesOrThrow(length);
            
            DebugGotAll(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, SharedKey.Length);
            w.Write(SharedKey);         
            WriteVarInt(w, Test.Length);
            w.Write(Test);               
        }
    }
}

