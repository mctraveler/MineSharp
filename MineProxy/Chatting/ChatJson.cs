using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using MineProxy.Chatting;

namespace MineProxy.Chatting
{
    /// <summary>
    /// Minecraft protocol encoding
    /// </summary>
    public class ChatJson
    {
        /// <summary>
        /// Only Text is used and serialized as "Text" only
        /// </summary>
        [JsonIgnore]
        public bool Raw { get; set; }

        [JsonProperty("insertion")]
        public string Insertion { get; set; }

        [JsonProperty("italic")]
        public bool Italic { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("translate")]
        public string Translate { get; set; }

        [JsonProperty("with")]
        public List<ChatJson> With { get; set; }

        [JsonProperty("clickEvent")]
        public ChatEvent ClickEvent { get; set; }

        [JsonProperty("hoverEvent")]
        public ChatEvent HoverEvent { get; set; }

        [JsonProperty("extra")]
        public List<ChatJson> Extra { get; set; }

        static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings();

        static ChatJson()
        {
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonSettings.Converters.Add(new ChatJsonConverter());
        }

        public static ChatJson Parse(string raw)
        {
            var json = JsonConvert.DeserializeObject<ChatJson>(raw, jsonSettings);
            #if DEBUG
            var serialized = json.Serialize();
            if (serialized != raw)
            {
                Debug.WriteLine("Chat RAW: " + raw);
                Debug.WriteLine("ChatJSON: " + serialized ?? "null");
                Console.WriteLine();
                //throw new NotImplementedException();
            }
            #endif
            return json;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, jsonSettings);
        }
    }
}

