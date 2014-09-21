using System;
using MineProxy.Packets;

namespace MineProxy.Chatting
{
    public class ServerParser
    {
        /// <summary>
        /// Return true to block message
        /// </summary>
        public static bool ParseServer(Client player, ChatMessageServer chat)
        {
            chat.SetPacketBuffer(null);
            var cm = chat.Json;
            string trans = cm.Translate;
            
            if (trans == "tile.bed.noSleep")
            {
                player.TellSystem(Chat.DarkBlue, "No sleep during daytime");
                return true;
            }
            
            var args = cm.With;
            if (trans == null || args == null)
                return false;

            //ignore Joined/Left messages, generated in proxy instead
            if (trans.StartsWith("multiplayer.player"))
                return true;

            switch (trans)
            {
            //ignore some messages
                case "commands.weather.clear":
                case "commands.weather.rain":
                case "commands.weather.thunder":
                case "commands.gamemode.success.other":
                case "commands.save.success":
                case "chat.type.admin":
                    return true;
            }

            Debug.WriteLine("ServerChat: " + cm.Serialize());

            if (trans == "chat.type.announcement")
            {
            /*
                if (args.Count == 2)
                    player.Tell(Chat.Purple, "[" + args [0] + "] " + args [1]);
                else
                {
                    string t = "";
                    foreach (var a in args)
                        t += " " + a.Text;
                    player.Tell(Chat.Purple, t);
                }*/
                return false;
            }

            //Herobrine never dies
            for (int n = 0; n < args.Count; n++)
            {
                if (args [n].Text == "Player")
                    return true;
            }

            //Allow all other
            if (trans.StartsWith("death.") == false)
                return false;

            //Block all other usernames
            if (args [0].Text != player.MinecraftUsername)
                return true;
        
            //not sure but better require preparation again
            chat.SetPacketBuffer(null);

            //Adjust player name
            if (args [0].Text != null)
                args [0].Text = args [0].Text.Replace(player.MinecraftUsername, player.Name);
                    
            if (args [0].ClickEvent != null)
                args [0].ClickEvent.Value = args [0].ClickEvent.Value.Replace(player.MinecraftUsername, player.Name);
            
            //Adjust attacker name
            if (args.Count > 1)
            {
                string byWho = args [1].Text;
                Client pp = PlayerList.GetPlayerByUsername(byWho);
                if (pp != null)
                {
                    Log.WriteAction(player, new Attacked(pp), true);

                    if (pp.Settings.Cloaked == MobType.None.ToString())
                    {
                        trans = "death.attack.mob";
                        cm.Translate = trans;
                        args [1].Text = "Skeleton";
                    } else if (pp.Settings.Cloaked != null)
                    {
                        trans = "death.attack.mob";
                        cm.Translate = trans;
                        args [1].Text = pp.Settings.Cloaked;
                    } else
                    {
                        //Stats
                        player.Settings.Stats.Murdered += 1;
                        pp.Settings.Stats.Kills += 1;
                    }
                }
            }

            if (!Cause.Alternatives.ContainsKey(trans))
            {
                Parser.TellNuxas(Chat.Aqua, "Unhandled cause of death: " + trans);
                return false;
            }

            /*var list = Cause.Alternatives [trans];
            Random r = new Random();
            cm.Translate = list [r.Next(list.Length)];
            */
            cm.Color = "red";
            player.Session.World.SendToAll(chat);

            player.Settings.Stats.NaturalDeath += 1;
            return true;
        }
    }
}

