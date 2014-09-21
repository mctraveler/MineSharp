using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
	public class PlayerPosition : PacketFromClient
	{
        public const byte ID = 0x04;

		public override byte PacketID { get { return ID; } }
				
        /// <summary>
        /// Feet position
        /// </summary>
		public CoordDouble Position { get; set; }

		public byte OnGround { get; set; }
	
        public PlayerPosition()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
			Position = new CoordDouble ();
			Position.X = r.ReadDouble ();
			Position.Y = r.ReadDouble ();
			Position.Z = r.ReadDouble ();
			OnGround = r.ReadByte ();
		}
		
		protected override void Prepare (MiscUtil.IO.EndianBinaryWriter w)
		{
			w.Write (Position.X);
			w.Write (Position.Y);
			w.Write (Position.Z);
			w.Write ((byte)OnGround);
		}
		
		public override string ToString ()
		{
			return base.ToString () + Position.X.ToString ("0.0") + ", " + Position.Y.ToString ("0.0") + ", " + Position.Z.ToString ("0.0");
		}
	}
}

