using System;
using MiscUtil.IO;
using System.Text;
using System.IO;

namespace MineProxy.Packets.Plugins
{
	public class PluginMessageFromServer : PacketFromServer
	{
		public const byte ID = 0x3F;

		public override byte PacketID { get { return ID; } }

		public virtual string Channel { get; set; }

		public const int MaxDataSize = 1048576;

		public byte[] Data { get; set; }

		public override string ToString ()
		{
			if (PacketBuffer != null)
				return string.Format ("[Plugin: prepared {0} bytes]", PacketBuffer.Length);
			else
				return string.Format ("[Plugin: {0}: {1}]", Channel, BitConverter.ToString (Data));
		}

		protected override void Parse (EndianBinaryReader r)
		{
			/*
            string channel = ReadString8(r);
            Varint int length = r.ReadInt16();
            if (length > MaxDataSize)
                throw new InvalidDataException("Plugin package payload size > " + MaxDataSize + " bytes");
            byte[] data = r.ReadBytesOrThrow(length);
            switch (channel)
            {
                case MCItemName.ChannelID:
                    return new MCItemName(data);
                case "MC|AdvCdm":
                case "MC|Beacon":
                case "MC|TPack":
                case "MC|TrList":
                case "MC|TrSel":
                case "MC|Brand":
                    return new UnknownPluginMessageServer(channel, data);
                default:
#if DEBUG
                    throw new InvalidOperationException("New Plugin channel: " + channel);
#else
                    return new UnknownPluginMessageServer(channel, data);
#endif
            }
*/
		}

		protected override void Prepare (EndianBinaryWriter w)
		{
            throw new NotImplementedException();
			/*
            WriteString8(w, Channel);
            Varint w.Write((short)Data.Length);
            w.Write(Data);          */
		}
	}
}

