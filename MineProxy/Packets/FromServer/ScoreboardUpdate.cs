using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class ScoreboardUpdate : PacketFromServer
    {
        public const byte ID = 0x3C;

        public override byte PacketID { get { return ID; } }

        const int maxNameLength = 40;

        public string Name { get; set; }

        public ScoreAction Action { get; set; }

        public string Board { get; set; }

        public int Value { get; set; }

        public ScoreboardUpdate()
        {
        }


        protected override void Parse(EndianBinaryReader r)
        {
            Name = ReadString8(r);
            Action = (ScoreAction)r.ReadSByte();
            if (Action == ScoreAction.Remove)
                return;
            Board = ReadString8(r);
            Value = ReadVarInt(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            if (Name.Length > maxNameLength)
                WriteString8(w, Name.Substring(0, maxNameLength));
            else
                WriteString8(w, Name);

            w.Write((sbyte)Action);
            if (Action == ScoreAction.Remove)
                return;
            WriteString8(w, Board);
            WriteVarInt(w, Value);
        }
    }
}

