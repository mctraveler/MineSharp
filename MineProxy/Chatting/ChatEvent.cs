using System;
using Newtonsoft.Json;

namespace MineProxy.Chatting
{
    /// <summary>
    /// event in a chat message
    /// </summary>
    public class ChatEvent
    {
        /// <summary>
        /// open_url
        /// open_file
        /// run_command
        /// suggest_command
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        ChatEvent()
        {
            
        }

        public static ChatEvent ClickOpenUrl(string url)
        {
            return new ChatEvent()
            {
                Action = "open_url",
                Value = url,
            };
        }

        public static ChatEvent ClickOpenFile(string path)
        {
            return new ChatEvent()
            {
                Action = "open_file",
                Value = path,
            };
        }

        public static ChatEvent ClickRunCommand(string command)
        {
            return new ChatEvent()
            {
                Action = "run_command",
                Value = command,
            };
        }

        public static ChatEvent ClickSuggestCommand(string command)
        {
            return new ChatEvent()
            {
                Action = "suggest_command",
                Value = command,
            };
        }

        public static ChatEvent HoverShowText(string text)
        {
            return new ChatEvent()
            {
                Action = "show_text",
                Value = text,
            };
        }

        public static ChatEvent HoverShowAchievement(string name)
        {
            return new ChatEvent()
            {
                Action = "show_achievement",
                Value = name,
            };
        }

        public static ChatEvent HoverShowItem(SlotItem item)
        {
            return new ChatEvent()
            {
                Action = "show_item",
                Value = "{id:" + item.ItemID + ",Damage:" + item.Uses + ",Count:" + item.Count + "}",
            };
        }
    }
}

