using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class Respawn : PacketFromServer
    {
        public const byte ID = 0x07;

        public override byte PacketID { get { return ID; } }

        public Dimensions Dimension { get; set; }

        public byte Difficulty { get; set; }

        public GameMode Mode { get; set; }

        public string LevelType { get; set; }

        public override string ToString()
        {
            return string.Format("[Respawn: Dim={1}, Diff={2}, {3}]", PacketID, Dimension, Difficulty, Mode);
        }

        public Respawn()
        {
            LevelType = "default";
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Dimension = (Dimensions)r.ReadInt32();
            Difficulty = r.ReadByte();
            Mode = (GameMode)r.ReadByte();
#if DEBUG
            if (Dimension.ToString() == ((int)Dimension).ToString())
                throw new NotImplementedException(Dimension.ToString());
            if (Mode.ToString() == ((int)Mode).ToString())
                throw new NotImplementedException(Mode.ToString());
#endif
            LevelType = ReadString8(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((int)Dimension);
            w.Write((byte)Difficulty);
            w.Write((byte)Mode);
            WriteString8(w, LevelType);
        }
    }
}

