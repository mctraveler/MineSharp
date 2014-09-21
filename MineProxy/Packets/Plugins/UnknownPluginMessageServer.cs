using System;
using MineProxy.Packets;

namespace MineProxy.Packets.Plugins
{
    /// <summary>
    /// Unknown plugin data
    /// </summary>
    public class UnknownPluginMessageServer : PluginMessageFromServer
    {
        public override string Channel{ get { return c; } }
        readonly string c;

        public UnknownPluginMessageServer(string channel, byte[] data)
        {
            c = channel;
            this.Data = data;
        }
    }
}

