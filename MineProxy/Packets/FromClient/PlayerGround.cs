using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
	public class PlayerGround : PacketFromClient
	{
        public const byte ID = 0x03;

		public override byte PacketID { get { return ID; } }
		
		public bool OnGround { get; set; }
		
		public override string ToString ()
		{
			return string.Format ("[PlayerGround: OnGround={0}]", OnGround);
		}
	
        public PlayerGround()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
			OnGround = r.ReadBoolean ();
		}
		
		protected override void Prepare (EndianBinaryWriter w)
		{
			w.Write (OnGround);
		}
	}
}

