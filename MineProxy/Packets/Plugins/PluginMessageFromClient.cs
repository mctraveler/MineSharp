using System;
using MiscUtil.IO;
using System.Text;
using System.IO;

namespace MineProxy.Packets.Plugins
{
    public class PluginMessageFromClient : PacketFromClient
    {
        public const byte ID = 0x17;

        public override byte PacketID { get { return ID; } }

        public virtual string Channel { get; set; }

        public byte[] Data { get; set; }

        public override string ToString()
        {
            if (Data == null)
            {
                if (PacketBuffer != null)
                    return string.Format("[Plugin: {0} bytes]", PacketBuffer.Length);
                else
                    return "[Plugin: null]";
            } else
                return string.Format("[Plugin: {0}: {1}]", Channel, BitConverter.ToString(Data));
        }

        protected override void Parse(EndianBinaryReader r)
        {
            SkipParse(r);
        }

        /*
        public static PluginMessageFromClient ParseMessage(EndianBinaryReader r)
        {
            string channel = ReadString8(r);
            int length = r.ReadInt16();
            if (length > PluginMessageFromServer.MaxDataSize)
                throw new InvalidDataException("Plugin package payload size > " + PluginMessageFromServer.MaxDataSize + " bytes");
            byte[] data = r.ReadBytesOrThrow(length);
            switch (channel)
            {
                case MCBook.ChannelEdit:
                case MCBook.ChannelSign:
                    return new MCBook(channel, data);
                case "MC|AdvCdm":
                case "MC|Beacon":
                case "MC|TPack":
                case "MC|TrList":
                case "MC|TrSel":
                case "MC|Brand":
                    return new UnknownPluginMessageClient(channel, data);
                case MCItemName.ChannelID:
                    return new UnknownPluginMessageClient(channel, data);
                default:
#if DEBUG
                    throw new InvalidOperationException("New Plugin channel: " + channel);
#else
                    return new UnknownPluginMessageClient(channel, data);
#endif
            }
        }*/

        protected override void Prepare(EndianBinaryWriter w)
        {
            /*
            WriteString8(w, Channel);
            w.Write((short)Data.Length);
            w.Write(Data);               
            */
        }
    }
}

