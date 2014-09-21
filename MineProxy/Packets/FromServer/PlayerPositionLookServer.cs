using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class PlayerPositionLookServer : PacketFromServer
    {
        public const byte ID = 0x08;
        public override byte PacketID { get { return ID; } }
		
        public CoordDouble Position { get; set; }

        /// <summary>
        /// The unit circle of yaw on the XZ-plane starts at (0, 1) and turns counterclockwise, with 90 at (-1, 0), 180 at (0,-1) and 270 at (1, 0).
        /// </summary>
        public double Yaw { get; set; }
		
        /// <summary>
        /// Pitch is measured in degrees, where 0 is looking straight ahead, -90 is looking straight up, and 90 is looking straight down.
        /// </summary>
        public double Pitch { get; set; }

        /// <summary>
        /// It's a bitfield, X/Y/Z/Y_ROT/X_ROT. If X is set, the x value is relative and not absolute.
        /// </summary>
        public byte Relative { get; set; }
		
        public override string ToString()
        {
            return string.Format("[PlayerPositionLookServer: PacketID={0}, Position={1}, Yaw={2}, Pitch={3}, Relative={4}]", PacketID, Position, Yaw, Pitch, Relative);
        }
		
        public PlayerPositionLookServer()
        {           

        }

        public PlayerPositionLookServer(CoordDouble pos)
        {
            Position = pos;
            Yaw = Math.PI / 2;
        }
        
        public PlayerPositionLookServer(CoordDouble pos, double pitch, double yaw)
        {
            Position = pos;
            Pitch = pitch;
            Yaw = yaw;
        }
        
        protected override void Parse(EndianBinaryReader r)
        {
            Position = new CoordDouble();
            Position.X = r.ReadDouble();
            Position.Y = r.ReadDouble();
            Position.Z = r.ReadDouble();
            Yaw = r.ReadSingle() * Math.PI / 180;
            Pitch = r.ReadSingle() * Math.PI / 180;
            Relative = r.ReadByte();

            //Console.WriteLine("Parsed: " + this);
            //Debug.WriteLine("Parsed: " + this);
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((double)Position.X);
            w.Write((double)Position.Y);
            w.Write((double)Position.Z);
            w.Write((float)(Yaw * 180 / Math.PI));
            w.Write((float)(Pitch * 180 / Math.PI));
            w.Write((byte)Relative);
        }
    }
}

