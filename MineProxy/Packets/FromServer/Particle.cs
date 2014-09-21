using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class Particle : PacketFromServer
    {
        public override byte PacketID { get { return ID; } }

        public const byte ID = 0x2A;
		
        ///<summary>
        ///<para>hugeexplosion</para>
        ///<para>largeexplode</para>
        ///<para>fireworksSpark</para>
        ///<para>bubble</para>
        ///<para>suspended</para>
        ///<para>depthsuspend</para>
        ///<para>townaura</para>
        ///<para>crit</para>
        ///<para>magicCrit</para>
        ///<para>smoke</para>
        ///<para>mobSpell</para>
        ///<para>mobSpellAmbient</para>
        ///<para>spell</para>
        ///<para>instantSpell</para>
        ///<para>witchMagic</para>
        ///<para>note</para>
        ///<para>portal</para>
        ///<para>enchantmenttable</para>
        ///<para>explode</para>
        ///<para>flame</para>
        ///<para>lava</para>
        ///<para>footstep</para>
        ///<para>splash</para>
        ///<para>largesmoke</para>
        ///<para>cloud</para>
        ///<para>reddust</para>
        ///<para>snowballpoof</para>
        ///<para>dripWater</para>
        ///<para>dripLava</para>
        ///<para>snowshovel</para>
        ///<para>slime</para>
        ///<para>heart</para>
        ///<para>angryVillager</para>
        ///<para>happyVillager</para>
        ///<para>iconcrack_*</para>
        ///<para>tilecrack_*_*</para>
        ///</summary>
        public int ParticleID { get; set; }
        public CoordDouble Position { get; set; }
        public CoordDouble Size { get; set; }
        public float Speed { get; set; }
        public int Count { get; set; }
        /// <summary>
        /// If true, particle distance increases from 256 to 65536.
        /// </summary>
        /// <value><c>true</c> if long distance; otherwise, <c>false</c>.</value>
        public bool LongDistance { get; set; }

        public override string ToString()
        {
            return string.Format("[Particle: PacketID={0}, ParticleID={1}, Position={2}, Size={3}, Count={4}]", PacketID, ParticleID, Position, Size, Count);
        }

        protected override void Parse(EndianBinaryReader r)
        {
            ParticleID = r.ReadInt32();
            LongDistance = r.ReadBoolean();
            Position = new CoordDouble();
            Position.X = r.ReadSingle();
            Position.Y = r.ReadSingle();
            Position.Z = r.ReadSingle();
            Size = new CoordDouble();
            Size.X = r.ReadSingle();
            Size.Y = r.ReadSingle();
            Size.Z = r.ReadSingle();
            Speed = r.ReadSingle();
            Count = r.ReadInt32();
            Debug.WriteLine(this);
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((int)ParticleID);
            w.Write((bool)LongDistance);
            w.Write((float)Position.X);
            w.Write((float)Position.Y);
            w.Write((float)Position.Z);
            w.Write((float)Size.X);
            w.Write((float)Size.Y);
            w.Write((float)Size.Z);
            w.Write((float)Speed);
            w.Write((int)Count);
        }
    }
}

