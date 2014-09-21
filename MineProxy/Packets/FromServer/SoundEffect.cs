using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class SoundEffect : PacketFromServer
    {
        public const byte ID = 0x29;

        public override byte PacketID { get { return ID; } }

        /// <summary>
        /// Play sound found in .minecraft/resources/
        /// </summary>
        public string Name { get; set; }

        public CoordDouble Position { get; set; }

        public float Volume { get; set; }

        /// <summary>
        /// 63 == 100% can be more
        /// </summary>
        public byte Pitch { get; set; }

/*
        public SoundCategoy Category { get; set; }

        public enum SoundCategoy : byte
        {
            MASTER = 0,
            MUSIC = 1,
            RECORDS = 2,
            WEATHER = 3,
            BLOCKS = 4,
            MOBS = 5,
            ANIMALS = 6,
            PLAYERS = 7,
        }
*/
        public override string ToString()
        {
            return string.Format("[SoundEffectNamed: {1}, {2}, Vol={3}, Pitch={4}]", PacketID, Name, Position, Volume.ToString("0.0"), Pitch);
        }

        public SoundEffect()
        {
        }

        public SoundEffect(string name, CoordDouble pos)
        {
            this.Name = name;
            this.Position = pos;
            this.Volume = 1.0f;
            this.Pitch = 63;
        }

        /// <param name='volume'>
        /// 1.0 == full
        /// </param>
        /// <param name='pitch'>
        /// 63 == 100% speed
        /// </param>
        public SoundEffect(string name, CoordDouble pos, double volume, int pitch)
        {
            this.Name = name;
            this.Position = pos;
            this.Volume = (float)volume;
            this.Pitch = (byte)pitch;
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Name = ReadString8(r);
            Position = ReadInt8(r);
            Volume = r.ReadSingle();
            Pitch = r.ReadByte();
            //Category = (SoundCategoy)r.ReadByte();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteString8(w, Name);
            WriteInt8(w, Position);
            w.Write((float)Volume);
            w.Write((byte)Pitch);
            //w.Write((byte)Category);
        }
    }
}

