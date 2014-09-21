using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MineProxy.Data;
using MineProxy.Chatting;

namespace MineProxy
{
    public static class Chat
    {
        public const string Black = "§0";
        public const string DarkBlue = "§1";
        public const string DarkGreen = "§2";
        public const string DarkAqua = "§3";
        public const string DarkRed = "§4";
        public const string Purple = "§5";
        public const string Gold = "§6";
        public const string Gray = "§7";
        public const string DarkGray = "§8";
        public const string Blue = "§9";
        public const string Green = "§a";
        public const string Aqua = "§b";
        public const string Red = "§c";
        public const string Pink = "§d";
        public const string Yellow = "§e";
        public const string White = "§f";
        public const string Random = "§k";
        public const string Bold = "§l";
        public const string Strike = "§m";
        public const string Underline = "§n";
        public const string Italic = "§o";
        public const string Reset = "§r";

        #region Chat settings
        
        public static void ResetChannel(Client player, string[] cmd, int iarg)
        {
            player.ChatChannel = null;
            TellChannel(player);
        }
        
        public static void SetChannel(Client player, string channel)
        {
            if (channel == "public")
            {
                player.ChatChannel = null;
            } else
            {
                channel = channel.Replace("§", "");
                player.ChatChannel = channel;
            }
            TellChannel(player);
        }

        public static void TellChannel(Client player)
        {
            if (player.ChatChannel == null)
            {
                player.TellSystem(Chat.Green, "You are talking to everyone");
            } else
            {
                player.TellSystem(Chat.Green, "You are talking in the \"" + Chat.DarkGreen + player.ChatChannel + Chat.Green + "\" channel");
                player.TellSystem(Chat.Green, "Reset using /reset");
            }
        }
        
        #endregion

        public static void TellStatTo(Client about, ClientSettings settings, Client toPlayer)
        {
            string name = "Offline";
            TimeSpan uptime = settings.Uptime;
            if (about != null)
            {
                uptime = about.Uptime;
                name = about.Name;
                if (about.Country != null)
                    toPlayer.TellSystem(Chat.Yellow + name + " ", "From: " + about.Country);
            }
            
            if (settings.FirstOnline.Ticks > 0)
                toPlayer.TellSystem(Chat.Yellow + name + " ", "First login: " + (DateTime.Now - settings.FirstOnline).TotalDays.ToString("0") + Chat.Blue + " days ago");
            if (uptime.TotalDays > 1)
                toPlayer.TellSystem(Chat.Yellow + name + " ", "Online: " + uptime.TotalDays.ToString("0.0") + Chat.Blue + " days");
            else
                toPlayer.TellSystem(Chat.Yellow + name + " ", "Online: " + uptime.TotalHours.ToString("0.0") + Chat.Blue + " hours");
            toPlayer.TellSystem(Chat.Yellow + name + " ", "Walked " + settings.WalkDistance.ToString("0") + Chat.Blue + " blocks");
            toPlayer.TellSystem(Chat.Yellow + name + " ", "Last Online: " + (DateTime.Now - settings.LastOnline).TotalHours.ToString("0.0") + " hours ago");
        }

        public static bool ReadFile(string path, string prefix, Client player)
        {
            path = Path.Combine("chat", path);
            if (File.Exists(path) == false)
            {
                if (path.Contains(" ") == false)
                    Log.WriteServer("File not found: " + path);
                return false;
            }
            
            using (TextReader reader = new StreamReader (path))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;
                    if (line.StartsWith("\t"))
                    {
                        prefix = line.Substring(1);
                        continue;
                    }
                    player.TellSystem(prefix, line);
                }
            }
            return true;
        }
        
    }
}

