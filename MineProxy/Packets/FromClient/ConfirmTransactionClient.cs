using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class ConfirmTransactionClient : PacketFromClient
    {
        public const byte ID = 0x0F;

        public override byte PacketID { get { return ID; } }

        public byte WindowID { get; set; }

        public short ActionID { get; set; }

        public bool Accepted { get; set; }

        public override string ToString()
        {
            return string.Format("[Transaction: WindowID={1}, ActionID={2}, Accepted={3}]", PacketID, WindowID, ActionID, Accepted);
        }

        public ConfirmTransactionClient(WindowClick wc, bool accept)
        {
            this.WindowID = wc.WindowID;
            this.ActionID = wc.ActionID;
            this.Accepted = accept;
        }

        public ConfirmTransactionClient()
        {

        }

        protected override void Parse(EndianBinaryReader r)
        {
            WindowID = r.ReadByte();
            ActionID = r.ReadInt16();
            Accepted = r.ReadBoolean();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((byte)WindowID);
            w.Write((short)ActionID);
            w.Write((bool)Accepted);
        }
    }
}

