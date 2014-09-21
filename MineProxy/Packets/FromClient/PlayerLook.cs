using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
	public class PlayerLook : PacketFromClient
	{
        public const byte ID = 0x05;

		public override byte PacketID { get { return ID; } }
		
		public double Yaw { get; set; }

		public double Pitch { get; set; }

		public bool OnGround { get; set; }
	
        public PlayerLook()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
			Yaw = r.ReadSingle () * Math.PI / 180;
			Pitch = r.ReadSingle () * Math.PI / 180;
			OnGround = r.ReadBoolean ();
#if DEBUG
			//Console.WriteLine("P L: " + Pitch);
#endif
		}
		
		protected override void Prepare (MiscUtil.IO.EndianBinaryWriter w)
		{
			w.Write ((float)(Yaw * 180 / Math.PI));
			w.Write ((float)(Pitch * 180 / Math.PI));
			w.Write (OnGround);
		}
	}
}

