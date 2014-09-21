using System;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using MineProxy.Chatting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using MiscUtil.IO;

namespace MineProxy.Packets
{
	public partial class ChatMessageServer : PacketFromServer
	{
		public const byte ID = 0x02;

		public override byte PacketID { get { return ID; } }

		public ChatJson Json { get; set; }

        public ChatPosition Position { get; set; }

		public override string ToString ()
		{
			return string.Format ("[ChatMessageServer: PacketID={0}, Json={1}, Position={2}]", PacketID, Json, Position);
		}

		public ChatMessageServer ()
		{
		}

        public static ChatMessageServer CreateText (string message, ChatPosition pos = ChatPosition.ChatBox)
		{
			if (message.Length > 119)
				throw new ArgumentException ("Too long >119");

			var c = new ChatMessageServer ();
			c.Json = new ChatJson ();
			c.Json.Text = message;
            c.Position = pos;
			return c;
		}

		protected override void Parse (EndianBinaryReader r)
		{
			string raw = ReadString8 (r);
			//Debug.WriteLine("Chat RAW: " + raw);
			try {
                Json = ChatJson.Parse(raw);
                Position = (ChatPosition)r.ReadByte ();
			} 
            #if !DEBUG
            catch (Exception ex) {
				Log.WriteServer (ex);
			}
            #endif
            finally {
			}
		}

		protected override void Prepare (EndianBinaryWriter w)
		{
            WriteString8 (w, Json.Serialize());
			w.Write ((byte)Position);
		}
	}
}

