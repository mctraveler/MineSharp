using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class ClientStatus : PacketFromClient
    {
        public const byte ID = 0x16;

        public override byte PacketID { get { return ID; } }

        public Actions Action { get; set; }

        public enum Actions
        {
            Respawn = 0,
            RequestStats = 1,
            OpenAchievements = 2,
        }

        public override string ToString()
        {
            return string.Format("[ClientStatuses: {1}]", PacketID, Action);
        }

        public ClientStatus(Actions val)
        {
            Action = val;
        }

        public ClientStatus()
        {
            
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Action = (Actions)ReadVarInt(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, (int)Action);
        }
    }
}

