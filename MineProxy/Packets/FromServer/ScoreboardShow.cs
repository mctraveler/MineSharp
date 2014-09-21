using System;
using MiscUtil.IO;
using MineProxy.Data;

namespace MineProxy.Packets
{
    public class ScoreboardShow : PacketFromServer
    {
        public const byte ID = 0x3D;
        
        public override byte PacketID { get { return ID; } }

        public ScreenPosition Position { get; set; }

        public string Board { get; set; }

        public ScoreboardShow(string board, ScreenPosition pos)
        {
            this.Board = board;
            this.Position = pos;
        }

        public ScoreboardShow()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Position = (ScreenPosition)r.ReadByte();
            Board = ReadString8(r);
        }
        
        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((byte)Position);
            WriteString8(w, Board);
        }
    }
}

