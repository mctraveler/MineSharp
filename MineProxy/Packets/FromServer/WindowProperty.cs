using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class WindowProperty : PacketFromServer
    {
        public const byte ID = 0x31;

        public override byte PacketID { get { return ID; } }

        public byte WindowID { get; set; }

        public short Property { get; set; }

        public short Value { get; set; }

        public override string ToString()
        {
            return string.Format("[WindowProperty: WindowID={1}, ProgressBar={2}, Value={3}]", PacketID, WindowID, Property, Value);
        }

        public WindowProperty()
        {
        }


        protected override void Parse(EndianBinaryReader r)
        {
            WindowID = r.ReadByte();
            Property = r.ReadInt16();
            Value = r.ReadInt16();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write(WindowID);
            w.Write(Property);
            w.Write(Value);
        }
    }
}

