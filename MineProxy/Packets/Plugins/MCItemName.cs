using System;
using System.Text;
using MineProxy.Packets;

namespace MineProxy.Packets.Plugins
{
    public class MCItemName : PluginMessageFromServer
    {
        public const string ChannelID = "MC|ItemName";

        public override string Channel { get { return ChannelID; } }

        public string Name { get; set; }

        public override string ToString()
        {
            return string.Format("[MCItemName: {0}]", Name);
        }

        public MCItemName(byte[] data)
        {
            this.Data = data;
            Name = Encoding.UTF8.GetString(data);
            if (Name.Length > 30)
#if DEBUG
                throw new InvalidOperationException("Name > 30");
#else
                Name = Name.Substring(0, 30);
#endif
        }
    }
}

