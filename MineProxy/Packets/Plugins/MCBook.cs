using System;
using MineProxy.NBT;
using System.IO;
using MineProxy.Packets;

namespace MineProxy.Packets.Plugins
{
    public class MCBook : PluginMessageFromClient
    {
        public const string ChannelEdit = "MC|BEdit";
        public const string ChannelSign = "MC|BSign";

        readonly string channel;
        public override string Channel { get { return channel; } }

        public Tag Content { get; set; }

        public MCBook(string channel, byte[] data)
        {
            this.channel = channel;
            this.Data = data;

            switch (channel)
            {
                case "MC|BEdit":
                case "MC|BSign":
                    break;
                default:
                    throw new NotImplementedException();
            }

            if ((Data [5] << 8) + Data [6] + 7 != Data.Length)
                throw new NotImplementedException();
            
            using (MemoryStream ms = new MemoryStream (Data, 7, Data.Length - 7, false))
                Content = Tag.ReadTag(ms);

            Debug.WriteLine(Content);
        }
    }
}

