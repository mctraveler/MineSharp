using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
	/// <summary>
	/// Should be sent at leaste every 1200 game ticks(20 ticks/s) once a minute
	/// </summary>
	public class KeepAlivePing : PacketFromServer
	{
        public const byte ID = 0x00;
        public override byte PacketID { get { return ID; } }
		
		public int KeepAliveID { get; set; }
		
		public override string ToString ()
		{
			return string.Format ("[KeepAlive: {0}]", KeepAliveID);
		}
		
		public KeepAlivePing (int id)
		{
			this.KeepAliveID = id;
		}

        public KeepAlivePing()
        {
            
        }
		
        protected override void Parse(EndianBinaryReader r)
        {
            KeepAliveID = ReadVarInt(r);
		}
		
		protected override void Prepare (EndianBinaryWriter w)
		{
            WriteVarInt(w, KeepAliveID);
		}
	}
}

