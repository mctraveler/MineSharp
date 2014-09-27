using System;
using System.IO;
using MineProxy.Commands;
using MineProxy.Worlds;
using System.Collections.Generic;
using MineProxy.Packets;

namespace MineProxy.Chatting
{
    public static class Parser
    {
        /// <summary>
        /// Pure chat completion
        /// </summary>
        public static void ParseClientChat(Client player, TabCompleteClient tab)
        {
            if (tab.Command.StartsWith("."))
            {
                Client p = PlayerList.GetPlayerByName(tab.Command.Substring(1));
                if (p != null)
                {
                    player.SendToClient(TabComplete.Single("." + p.Name + " "));       
                }
                return;
            }
            
            string last = tab.Command;
            
            if (last.StartsWith("co"))
            {
                player.SendToClient(TabComplete.Single(last + " " + player.Session.Position.ToString("0")));
                return;
            }
            
            {
                Client p = PlayerList.GetPlayerByName(last);
                if (p != null)
                {
                    player.Queue.Queue(TabComplete.Single(p.Name));       
                    return;
                }
            }
            
            player.SendToClient(TabComplete.Single(last + " /love " + player.Name + " "));
        }

        static bool TalkingToResident(Client fr, Client to)
        {
            WorldSession s = fr.Session;
            if (s == null)
                return false;
            WorldRegion r = s.CurrentRegion;
            if (r == null)
                return false;
            return r.IsResident(to);
        }

        /// <summary>
        /// If a warzone send chat to everyone inside and block it to everyone else.
        /// Otherwise return false.
        /// </summary>
        static bool WarChat(Client player, string message)
        {
            WorldSession s = player.Session;
            if (s == null)
                return false;
            WorldRegion r = s.CurrentRegion;
            if (r == null)
                return false;
            if (r.Type != WarZone.Type)
                return false;
            
            if (r.TellAll(Chat.Red + player.Name + " " + Chat.White, message) <= 1)
                player.TellSystem(Chat.Red, "You are alone in this zone");
            return true;
        }

        public static void Say(string prefix, string message, ChatPosition pos = ChatPosition.SystemMessage)
        {
            foreach (Client p in PlayerList.List)
            {
                string msg = Format.ReceiverMacros(p, message);
                var msgs = Format.Split(prefix, msg, pos);
                foreach (var m in msgs)
                    p.Queue.Queue(m);
            }
        }

        /// <summary>
        /// Tell everyone listening to the firehose
        /// </summary>
        public static void SayFirehose(string prefix, string message)
        {
            foreach (Client p in  PlayerList.List)
            {
                if (p.Settings.Firehose)
                    p.TellSystem(prefix, message);
            }
        }

        public static void TellNuxas(string prefix, string message)
        {
            Client p = PlayerList.GetPlayerByUsername("nuxas");
            if (p != null)
                p.TellSystem(prefix, message);
            p = PlayerList.GetPlayerByUsername("Player");
            if (p != null)
                p.TellSystem(prefix, message);
        }

        public static void TellAdmin(string message)
        {
            foreach (var username in MinecraftServer.Admins)
            {
                Client p = PlayerList.GetPlayerByUsername(username);
                if (p == null)
                    continue;
                p.TellSystem(Chat.Purple + "Admin: " + Chat.White, message);
            }
        }

        public const int DistanceClose = 20;
        public const int DistanceFar = 100;
        public const int DistanceMax = 500;

        public static void SendPrivateMessage(Client player, string name, string msg)
        {
            if (Banned.CheckBanned(player) != null)
            {
                player.TellSystem(Chat.Red, "No PM in Hell, wait for someone to join");
                return;
            }

            Client p = PlayerList.GetPlayerByName(name);

            //Make sure the player ever has been online
            if (p == null)
            {
				if (File.Exists("proxy/players/" + Path.GetFileName(name) + ".json") == false)
                {
                    player.TellSystem(Chat.Red, "No such player: " + Format.StripFormat(name));
                    return;
                }
                Inbox.Write(player, name, msg);   
                
                Log.WriteChat(player, name, 1, msg);
            } else
                Log.WriteChat(player, p.MinecraftUsername, 1, msg);

            string trans = Translator.TranslateFromPlayer(player, msg);
            if (trans != null)
                msg = trans;
            
            player.LastOutTell = name.ToLowerInvariant();
            
            string chatRecipient = Chat.Yellow + player.Name;
            
            //Sender display
            if (p == null || p.Settings.Cloaked != null && p.LastOutTell != player.Name.ToLowerInvariant())
            {
                //Offline/hidden
                player.TellSystem("> " + Chat.Yellow + name + Chat.Gray + (player.Settings.Help ? " [private] " : " "), msg);
                player.TellSystem(Chat.Red, name + " is offline, message saved in their inbox");
                
                chatRecipient += Chat.Red + " (hidden)";
            } else
            {
                //Online, visible or started conversation
                player.TellSystem("> " + Chat.Yellow + p.Name + Chat.Gray + (player.Settings.Help ? " [private] " : " "), msg);
                player.LastOutTell = p.Name.ToLowerInvariant();
            }
            
            if (p == null)
                return;
            
            //Recipient display
            if (player.Session.Position.DistanceTo(p.Session.Position) < Parser.DistanceClose)
                chatRecipient += Chat.White + (p.Settings.Help ? " [private] " : " ");
            else
                chatRecipient += Chat.Gray + (p.Settings.Help ? " [private] " : " ");
            
            p.TellSystem(chatRecipient, msg);
            p.LastInTell = player.Name;
        }

        /// <summary>
        /// Pure chat messages
        /// </summary>
        public static void ParseClientChat(Client player, string message)
        {
            //Block unknown prevent mistype
            if (message.StartsWith(",") || message.StartsWith(">"))
            {
                player.TellSystem(Chat.Red, "unknown start symbol, begin with a space to ignore");
                return;
            }
            
            //remove links from new players
            /*
            if (player.Uptime.TotalDays < 2)
            {
                player.Tell(Chat.Purple, "New players can't send links");
                message = message.Replace("http://", "");
                message = message.Replace("https://", "");
                var mp = message.Split();
                foreach(var m in mp)
                {
                    if(m.StartsWith("http://") ||  m.StartsWith("https://"))
                }
            }*/

            //Short /tell
            if (message.StartsWith("."))
            {
                string[] parts = message.Substring(1).Split(' ');
                if (parts.Length < 2)
                {
                    player.TellSystem(Chat.Red, "Usage: .username message");
                    return;
                }
                message = FormatSpell(player, parts.JoinFrom(1));
                Chatting.Parser.SendPrivateMessage(player, parts [0], message);
                return;
            }
            
            //Short reply
            if (message.StartsWith("<"))
            {
                if (player.LastInTell == null)
                {
                    player.TellSystem(Chat.Red, "You must have received a private message before replying");
                    return;
                }
                message = FormatSpell(player, message.Substring(1));
                Chatting.Parser.SendPrivateMessage(player, player.LastInTell, message);
                return;
            }
            
            //Short /t
            if (message.StartsWith("-"))
            {
                if (player.LastOutTell == null)
                {
                    player.TellSystem(Chat.Red, "Use \".username message\" one time first");
                    return;
                }
                message = FormatSpell(player, message.Substring(1));
                Chatting.Parser.SendPrivateMessage(player, player.LastOutTell, message);
                return;
            }

            //CAPS block
            if (message.Length > 3)
            {
                string caps = message.ToUpperInvariant();
                if (caps == message)
                    message = message.ToLowerInvariant();
            }
            
            //Shout !
            if (message == "!")
                message = "!!";
            bool shout = message.StartsWith("!");
            if (shout)
                message = message.Substring(1);
            if (player.Session.CurrentRegion != null && player.Session.CurrentRegion.Type == "spawn" && player.ChatChannel == null)
                shout = true;
            
            int receivers = 0;
            
            //No shouting for banned
            if (shout && Banned.CheckBanned(player) != null)
            {
                player.TellSystem(Chat.Red, "No shouting for the banned!");
                return;
            }

            //Format Translate Spell for all other
            message = FormatSpell(player, message);

            //War chat
            if (!shout)
            {
                if (WarChat(player, message))
                    return;
            }
            if (player.Settings.Cloaked != null || player.Session is PossessSession)
            {
                player.TellSystem(Chat.Red, "no chat while cloaked, pm still works");
                return;
            }
            
            
            foreach (Client p in  PlayerList.List)
            {
                if (shout)
                {
                    p.TellSystem(Chat.Gold + player.Name + " " + Chat.White + (p.Settings.Help ? "[shout] " : ""), message);
                    receivers += 1;
                    continue;
                }
                
                if (player.ChatChannel != null)
                {
                    //Channel chat
                    if (player.ChatChannel == p.ChatChannel)
                    {
                        p.TellSystem(Chat.Blue + player.Name + " " + Chat.Green + "[" + p.ChatChannel + "] ", message);
                        receivers += 1;
                    }
                    continue;
                }
                
                double distance = player.Session.Position.DistanceTo(p.Session.Position);
                
                //residents always hear
                if (distance >= DistanceMax && TalkingToResident(player, p))
                {
                    p.TellSystem(Chat.Blue + player.Name + " " + Chat.Aqua + "[" + player.Session.CurrentRegion.Name + "] " + Chat.Gray, message);
                    receivers += 1;
                    continue;
                }
                
                if (p.Settings.Firehose == false)
                {
                    if (player.Session.World != p.Session.World)
                        continue;
                    if (player.Session.Dimension != p.Session.Dimension)
                        continue;
                } else
                {
                    if (player.Session.World != p.Session.World)
                        distance = DistanceMax + 1;
                    if (player.Session.Dimension != p.Session.Dimension)
                        distance = DistanceMax + 1;
                    
                    //firehose does not hear banned
                    if ((p.Session is HellSession == false) && Banned.CheckBanned(player) != null)
                        continue;
                }

                var chatText = new ChatJson()
                {
                    Text = message,
                    Color = "gray",
                    //HoverEvent = ChatEvent.HoverShowText("that's what " + player.Name + " said"),
                };
                var c = new ChatJson();
                c.Translate = "%1$s %2$s";
                c.With = new List<ChatJson>()
                {
                    new ChatJson()
                    {
                        Text = player.Name,
                        Color = "blue",
                        ClickEvent = ChatEvent.ClickSuggestCommand("." + player.Name + " "),
                        HoverEvent = ChatEvent.HoverShowText("send private message"),
                    },
                    chatText
                };

                if (distance < DistanceClose && p != player)
                    chatText.Color = "yellow";
                else if (distance < DistanceFar || p.Settings.Firehose && distance < DistanceMax)
                    chatText.Color = "white";
                else if (distance < DistanceMax || p.Settings.Firehose)
                    chatText.Color = "gray";
                else
                    continue;

                var packet = new ChatMessageServer();
                packet.Json = c;

                //Send message
                p.Queue.Queue(packet);

                if (p.Settings.Cloaked != null) //Dont count cloaked players
                    continue;
                
                if (p != player)
                    receivers += 1;
            }
            
            if (receivers == 0)
                player.TellSystem(Chat.Blue, "No one heard you, " + Chat.Gray + "see /help");
            
            Log.WriteChat(player, player.ChatChannel, receivers, message);
            
            player.ChatEntry = new Entry(player.ChatChannel, message);
        }

        static string FormatSpell(Client player, string message)
        {
            //Translate & spelling
            if (Banned.CheckBanned(player) == null)
            {
                string trans = Translator.TranslateFromPlayer(player, message);
                if (trans == null)
                    return Spelling.SpellFormat(message);
                else
                    return trans; //Don't spell correct translations
            } else
                return Spelling.SpellFormat(message);
        }

        public static void ParseClient(Client player, ChatMessageClient chat)
        {
            string message = chat.Text;
            
            if (message.Contains("§"))
            {
                Log.WritePlayer(player, "Illegal chat: " + message);
                return;
            }

            //Flood detection
            if (player.ChatFloodNextReset < DateTime.Now)
            {
                player.ChatFloodCount = 1;
                player.ChatFloodNextReset = DateTime.Now.AddSeconds(5);
            } else
            {
                player.ChatFloodCount += 1;
                if (player.ChatFloodCount > 10)
                {
                    player.ChatFloodCount = 0;
                    player.BanByServer(DateTime.Now.AddMinutes(5), "Chat Flood");
                    return;
                }
            }

            if (chat.Text.StartsWith("/"))
                MainCommands.ParseClientCommand(player, message);
            else
                ParseClientChat(player, message);
        }

        public static void ParseClient(Client player, TabCompleteClient tab)
        {
            if (tab.Command.StartsWith("/"))
                MainCommands.ParseClientTab(player, tab);
            else
                ParseClientChat(player, tab);
        }
    }
}

