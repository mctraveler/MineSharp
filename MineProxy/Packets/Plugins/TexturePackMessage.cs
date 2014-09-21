using System;
using System.Text;
using MineProxy.Chatting;

namespace MineProxy.Packets.Plugins
{
    /// <summary>
    /// Unknown plugin data
    /// </summary>
    public class TexturePackMessage : PluginMessageFromServer
    {
        public const string ChannelName = "MC|TPack";

        public override string Channel{ get { return ChannelName; } }

        public string Url { get; private set; }

        public TexturePackMessage(string url)
        {
            var u = url.Split(new char[]{'\t'});
            if (u.Length > 2)
                throw new InvalidOperationException("too many tabs in texturepack");

            if (u.Length == 1)
            {
                this.Url = url;
                this.Data = Encoding.ASCII.GetBytes(url + "\0" + "16");
            } else
            {
                this.Url = u [0];
                this.Data = Encoding.ASCII.GetBytes(u [0] + "\0" + u [1]);
            }
        }
    }
}

