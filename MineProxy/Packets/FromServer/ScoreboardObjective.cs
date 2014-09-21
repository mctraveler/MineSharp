using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class ScoreboardObjective : PacketFromServer
    {
        public const byte ID = 0x3B;

        public override byte PacketID { get { return ID; } }

        /// <summary>
        /// Unique name for the board
        /// </summary>
        public string Board { get; set; }

        public ScoreAction Mode { get; set; }

        /// <summary>
        /// Text to display
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// "integer" or "hearts"
        /// </summary>
        public string Type { get; set; }

        public ScoreboardObjective()
        {
        }


        protected override void Parse(EndianBinaryReader r)
        {
            Board = ReadString8(r);
            Mode = (ScoreAction)r.ReadSByte();
            if (Mode == ScoreAction.Remove)
                return;
            Value = ReadString8(r);
            Type = ReadString8(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteString8(w, Board);
            w.Write((sbyte)Mode);
            if (Mode == ScoreAction.Remove)
                return;
            WriteString8(w, Value);
            WriteString8(w, Type);
        }
    }
}

