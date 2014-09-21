using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class Explosion : PacketFromServer
    {
        public const byte ID = 0x27;
        
        public override byte PacketID { get { return ID; } }
        
        public CoordDouble Position { get; set; }
        
        public float Radius { get; set; }
        
        public class Record
        {           
            public sbyte X { get; set; }

            public sbyte Y { get; set; }

            public sbyte Z { get; set; }
        }
        
        public Record[] Records { get; set; }
        
        /// <summary>
        /// As pushed away
        /// </summary>
        /// <value>The player motion.</value>
        public float[] PlayerMotion { get; set; }

        public Explosion()
        {
        }

        public Explosion(CoordDouble pos)
        {
            Position = pos;
            Radius = 2;
            Records = new Record[0];
            PlayerMotion = new float[3];
        }
        
        protected override void Parse(EndianBinaryReader r)
        {
            Position = new CoordDouble();
            Position.X = r.ReadSingle();
            Position.Y = r.ReadSingle();
            Position.Z = r.ReadSingle();
            Radius = r.ReadSingle();
            int count = r.ReadInt32();
            Records = new Record[count];
            for (int n = 0; n < count; n++)
            {
                Records [n] = new Record();
                Records [n].X = r.ReadSByte();
                Records [n].Y = r.ReadSByte();
                Records [n].Z = r.ReadSByte();
            }
            PlayerMotion = new float[3];
            PlayerMotion [0] = r.ReadSingle();
            PlayerMotion [1] = r.ReadSingle();
            PlayerMotion [2] = r.ReadSingle();
        }
        
        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((float)Position.X);
            w.Write((float)Position.Y);
            w.Write((float)Position.Z);
            w.Write((float)Radius);
            if (Records == null)
                w.Write((int)0);
            else
            {
                w.Write((int)Records.Length);
                for (int n = 0; n < Records.Length; n++)
                {
                    w.Write(Records [n].X);
                    w.Write(Records [n].Y);
                    w.Write(Records [n].Z);
                }
            }
            w.Write((float)PlayerMotion [0]);
            w.Write((float)PlayerMotion [1]);
            w.Write((float)PlayerMotion [2]);
        }
    }
}

