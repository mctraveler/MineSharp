using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class PlayerPositionLookClient : PacketFromClient
    {
        public const byte ID = 0x06;
        public override byte PacketID { get { return ID; } }
		
        /// <summary>
        /// Foot position
        /// </summary>
        public CoordDouble Position { get; set; }

        public double Yaw { get; set; }
		
        public double Pitch { get; set; }

        public bool OnGround { get; set; }
		
        public override string ToString()
        {
            return string.Format("[PlayerPositionLook: Position={1}, Yaw={2}, Pitch={3}, OnGround={4}]", PacketID, Position, Yaw, Pitch, OnGround);
        }
		
        public PlayerPositionLookClient()
        {
        }
        
        protected override void Parse(EndianBinaryReader r)
        {
            Position = new CoordDouble();
            Position.X = r.ReadDouble();
            Position.Y = r.ReadDouble();
            Position.Z = r.ReadDouble();
            Yaw = r.ReadSingle() * Math.PI / 180;
            Pitch = r.ReadSingle() * Math.PI / 180;
            OnGround = r.ReadBoolean();
#if DEBUG
            //Console.WriteLine("PPL: " + Pitch);
#endif
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((double)Position.X);
            w.Write((double)Position.Y);
            w.Write((double)Position.Z);
            w.Write((float)(Yaw * 180 / Math.PI));
            w.Write((float)(Pitch * 180 / Math.PI));
            w.Write(OnGround);
        }
    }
}

