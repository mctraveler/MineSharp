using System;

namespace MineProxy
{
	public class LoginRequestClient : Packet
	{
		public override byte PacketID { get { return 1; } }
		
		public int ProtocolVersion { get; set; }

		public string Username { get; set; }

		public long Unused1 { get; set; }
		
		public int Unused2 { get; set; }
		
		public sbyte Unused3 { get; set; }
		
		public sbyte Unused4 { get; set; }
		
		public sbyte Unused5 { get; set; }
		
		public sbyte Unused6 { get; set; }
		
		public LoginRequestClient ()
		{
			//1.8 => 17
			//1.9pre3 => 19
			ProtocolVersion = 17;
		}
		
		public override string ToString ()
		{
			return string.Format ("[LoginRequestClient: v{0}, {1}]", ProtocolVersion, Username);
		}
		
		public LoginRequestClient (MiscUtil.IO.EndianBinaryReader reader)
		{
			ProtocolVersion = reader.ReadInt32 ();
			Username = ReadString16 (reader);
			Unused1 = reader.ReadInt64 ();
			Unused2 = reader.ReadInt32 ();
			Unused3 = reader.ReadSByte ();
			Unused4 = reader.ReadSByte ();
			Unused5 = reader.ReadSByte ();
			Unused6 = reader.ReadSByte ();
		}
		
		protected override void Send (MiscUtil.IO.EndianBinaryWriter writer)
		{
			writer.Write ((int)ProtocolVersion);
			WriteString16 (writer, Username);
			writer.Write ((long)Unused1);
			writer.Write ((int)Unused2);
			writer.Write ((sbyte)Unused3);
			writer.Write ((sbyte)Unused4);
			writer.Write ((sbyte)Unused5);
			writer.Write ((sbyte)Unused6);
		}
	}
}

