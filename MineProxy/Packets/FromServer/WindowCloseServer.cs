using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
	public class WindowCloseServer : PacketFromServer
	{
        public const byte ID = 0x2E;

		public override byte PacketID { get { return ID; } }
		
		public byte WindowID { get; set; }
		
		public override string ToString ()
		{
			return "[0x65 WindowClose: ID=" + WindowID + "]";
		}

		public WindowCloseServer (byte window)
		{
		}
		
        public WindowCloseServer()
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

