using System;
using Newtonsoft.Json;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using MineProxy.Chatting;

namespace MineProxy.Packets
{
    /// <summary>
    /// https://gist.github.com/thinkofdeath/6927216
    /// </summary>
    public class ServerStatus
    {
        [JsonProperty("version")]
        public VersionPair Version { get; set; }

        [JsonProperty("players")]
        public PlayersStatus Players { get; set; }

        [JsonProperty("description")]
        public ChatJson Description { get; set; }

        [JsonProperty("favicon")]
        public string Favicon { get; set; }

        public ServerStatus()
        {
            Version = new VersionPair(MinecraftServer.FrontendVersion);
            Players = new PlayersStatus();
            Description = ChatMessageServer.CreateText(MinecraftServer.PingReplyMessage).Json;
            
            //Favicon - nothing seem to work but the gist says it should
            /*Favicon = "http://mctraveler.eu/icon.png";
            //http://status.mctraveler.eu/logo.png";

            Bitmap b = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(b))
            {
                g.FillEllipse(Brushes.Red, 1, 2, 5, 6); 
            }
            using (var ms = new MemoryStream())
            {
                b.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                Favicon = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
            }*/
            string faviconPath = "icon.png";
            if (File.Exists(faviconPath))
                Favicon = "data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(faviconPath));
        }

        public class VersionPair
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("protocol")]
            public int Protocol { get; set; }

            public VersionPair(ProtocolVersion ver)
            {
                Name = ver.ToText();
                Protocol = (int)ver;
            }
        }

        public class PlayersStatus
        {
            [JsonProperty("max")]
            public int Max { get; set; }

            [JsonProperty("online")]
            public int Online { get; set; }

            [JsonProperty("sample")]
            public List<Sample> Sample { get; set; }

            public PlayersStatus()
            {
                Max = MinecraftServer.MaxSlots;
                var list = PlayerList.List;
                Online = list.Length;
                if (list.Length > 0)
                {
                    Sample = new List<ServerStatus.Sample>();
                    foreach (var p in list)
                        Sample.Add(new ServerStatus.Sample(p));
                }
            }
        }

        public class Sample
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("id")]
            public string ID { get; set; }

            public Sample(Client p)
            {
                Name = p.MinecraftUsername;
                ID = "";
            }
        }

        static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings();

        static ServerStatus()
        {
            jsonSettings.Formatting = Formatting.Indented;
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
        }

        public string ToJson()
        {
            string json = JsonConvert.SerializeObject(this, jsonSettings);
            return json;
        }
    }
}

