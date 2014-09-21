using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
	public class WindowCloseClient : PacketFromClient
	{
        public const byte ID = 0x0D;

		public override byte PacketID { get { return ID; } }
		
		public byte WindowID { get; set; }
		
		public override string ToString ()
		{
			return "[0x65 WindowClose: ID=" + WindowID + "]";
		}

		public WindowCloseClient (byte window)
		{
		}

        public WindowCloseClient()
        {

        }
		
        protected override void Parse(EndianBinaryReader r)
        {
			WindowID = r.ReadByte ();
		}
		
		protected override void Prepare (EndianBinaryWriter w)
		{
			w.Write (WindowID);
		}
	}
}

