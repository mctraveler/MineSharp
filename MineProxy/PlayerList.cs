using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Net;
using MineProxy.Control;
using MineProxy.Packets;
using System.Diagnostics;
using MineProxy.Chatting;
using MineProxy.Worlds;

namespace MineProxy
{
    internal static class PlayerList
    {
        //static DateTime Launch = new DateTime (2011, 11, 18, 22, 00, 00);
        
        static void UpdateServerListPingReply()
        {
            string motd = MinecraftServer.PingReplyMessage ?? "Hi";
            //TimeSpan left = Launch - DateTime.Now;
            //if (left.Ticks > 0) {
            //  motd = left.ToString ("hh") + ":" + left.ToString ("mm") + ":" + left.ToString ("ss") + " until Minecon talk!";
            //}
            
            int count = 0;
            int max = MinecraftServer.MaxSlots;
            List<string> players = new List<string>();
            foreach (Client p in PlayerList.List)
            {
                if (p.MinecraftUsername == "Player")
                    continue;
                if (p.Settings.Cloaked != null)
                    continue;
                count++;
                players.Add(p.Name);
                if (p.Session is VanillaSession == false)
                    max++;
            }

            //Query replies
            Query.QueryListener.UpdateStat(motd, players);
        }

        public static void LoginPlayer(Client player)
        {
            if (player.EntityID == 0 || player.MinecraftUsername == null)
                throw new InvalidOperationException("Can't login");
            
            Client pl = null;
            lock (list)
            {
                foreach (Client p in list)
                {
                    if (p == player)
                        return;
                    //Already logged in
                    if (p.MinecraftUsername == player.MinecraftUsername)
                    {
                        list.Remove(p);
                        pl = p;
                        break;
                    }
                }
                
                list.Add(player);
                List = list.ToArray();
            }
            if (pl != null)
            {
                pl.Close("Duplicate connections");
                Log.WritePlayer(player, "Disposed, Connected with another session");
            }

            Log.WriteUsersLog();
            
            //IP correlate
            IPCorrelate(player, player.MinecraftUsername);

            Chat.ReadFile("motd.txt", "", player);

            try
            {
                Welcome(player);
            }
            catch (Exception e)
            {
                Log.Write(e, player);
            }

            //PlayerList.UpdateRefreshTabPlayers();
        }

        public static void IPCorrelate(Client player, string username)
        {
            lock (ipUsername)
            {
                
                //Add if new ip
                if (ipUsername.ContainsKey(player.RemoteEndPoint.Address) == false)
                    ipUsername.Add(player.RemoteEndPoint.Address, new List<string>());
                List<string > usernames = ipUsername[player.RemoteEndPoint.Address];
                //Add if new username
                if (usernames.Contains(username) == false)
                    usernames.Add(username);
                
                //Dont report banned players
                if (player.MinecraftUsername == null)
                    return;
                
                foreach (string u in usernames)
                {
                    if (u == player.MinecraftUsername)
                        continue;
                    BadPlayer b = Banned.GetBanHistory(u);
                    if (b == null)
                    {
                        Log.WritePlayer(player, "AKA " + u);
                        Chatting.Parser.TellAdmin(Chat.Purple + player.MinecraftUsername + Chat.Aqua + " AKA " + Chat.Yellow + u);
                    }
                    else
                    {
                        Log.WritePlayer(player, "AKA " + b);
                        if (b.BannedUntil < DateTime.Now)
                            Chatting.Parser.TellAdmin(Chat.Purple + player.MinecraftUsername + Chat.Aqua + " AKA " + Chat.DarkRed + u + Chat.White + " Expired: " + b.Reason);
                        else
                            Chatting.Parser.TellAdmin(Chat.Purple + player.MinecraftUsername + Chat.Aqua + " AKA " + Chat.Red + u + Chat.White + " " + b.Reason);

                    }
                }
            }
        }

        static void Welcome(Client player)
        {   
            var header = new PlayerListHeaderFooter();
            header.Header = new ChatJson();
            header.Header.Translate = "%1$s\n%2$s";
            header.Header.With = new List<ChatJson>()
            {
                new ChatJson()
                {
                    Text = " ==== MCTraveler.eu ==== ",
                    Color = "yellow",
                },
                new ChatJson()
                {
                    Text = " ======================= ",
                    Color = "gray",
                },
            };
            header.Footer = new ChatJson()
            {
                Text = " ======================= ",
                Color = "gray",
            };
            player.Queue.Queue(header);

            SendTabHeader(player);
//          if (player.ClientVersion < MinecraftServer.Version)
//              player.Tell(Chat.Purple, "You should upgrade to " + MinecraftServer.Version.ToString().Replace('_', '.'));

            /*
            int count = 0;
            foreach (Client p in PlayerList.List)
                if (p.Cloaked == null && p.MinecraftUsername != "Player")
                    count ++;
            if (count == 1)
                player.Tell(Chat.Yellow, "Welcome, " + player.Name + " you are the first one here");
            else if (count == 2)
                player.Tell(Chat.Yellow, "Welcome, " + player.Name + " there is one other player here");
            else
                player.Tell(Chat.Yellow, "Welcome, " + player.Name + " there are " + (count - 1) + " other players here");
            */

            //Tell donor status
            //player.Tell(Donors.Status(player.MinecraftUsername));
            
            string version = "";
            if (player.ClientVersion < MinecraftServer.FrontendVersion)
                version = Chat.Gray + "(" + player.ClientVersion.ToText() + ")";
            if (player.Settings.Cloaked == null && Banned.CheckBanned(player) == null && player.MinecraftUsername != "Player")
            {
                if (player.Uptime.TotalMinutes < 1)
                {
                    Log.WritePlayer(player, "Login FirstTime");
                    Chatting.Parser.Say(Chat.Purple, "Welcome " + Chat.Yellow + player.Name + Chat.Purple + " from " + player.Country + version);
                }
                else
                {
                    if (Donors.IsDonor(player))
                        Chatting.Parser.Say(Chat.Gold, player.Name + " (donor!) joined from " + player.Country + version);
                    else
                        Chatting.Parser.Say(Chat.Yellow, player.Name + " joined from " + player.Country + version);
                }

                //Report old bans to admin
                BadPlayer bp = Banned.GetBanHistory(player.MinecraftUsername);
                if (bp != null)
                    Chatting.Parser.TellAdmin("BanHistory: " + bp.BannedUntil.ToString("yyyy-MM-dd") + " " + bp.Reason);

            }

        }

        public static void LogoutPlayer(Client pp)
        {
            bool removed = false;
            lock (list)
            {
                removed = list.Contains(pp);
                list.Remove(pp);
                List = list.ToArray();
            }
            if (removed)
                QueueToAll(PlayerListItem.RemovePlayer(pp));
            //PlayerList.UpdateRefreshTabPlayers();
                        
            Log.WriteUsersLog();
            
            if (removed && pp.MinecraftUsername != null && pp.Settings.Cloaked == null && Banned.CheckBanned(pp) == null && pp.MinecraftUsername != "Player")
                Chatting.Parser.Say(Chat.Gray, pp.Name + " left the game");
        }

        static List<Client> list = new List<Client>();
        static Dictionary<IPAddress, List<string>> ipUsername = new Dictionary<IPAddress, List<string>>();
        public static Client[] List = new Client[0];

        public static void QueueToAll(PacketFromServer packet)
        {
            foreach (Client p in PlayerList.List)
            {
                p.Queue.Queue(packet);
            }
        }

        public static void QueueToAll(List<PacketFromServer> packet)
        {
            foreach (Client p in PlayerList.List)
            {
                p.Queue.Queue(packet);
            }
        }

        public static Client GetPlayerByUsername(string username)
        {
            return GetPlayerByUsernameOrName(username);
        }

        public static Client GetPlayerByUUID(Guid id)
        {
            foreach (Client p in List)
            {
                if (p.UUID == id)
                    return p;
            }
            return null;
        }

        public static Client GetPlayerByVanillaUUID(Guid id)
        {
            foreach (Client p in List)
            {
                var vs = p.Session as VanillaSession;
                if (vs == null)
                    continue;
                if (vs.UUID == id)
                    return p;
            }
            return null;
        }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public static Client GetPlayerByUsernameOrName(string username)
        {
            username = username.ToLowerInvariant();
            //First run, exact match
            foreach (Client p in List)
            {
                if (p.MinecraftUsername.ToLowerInvariant() == username ||
                    p.Name.ToLowerInvariant() == username)
                    return p;
            }
            //Second run start of name
            List<Client > match = new List<Client>();
            foreach (Client p in List)
            {
                if (p.MinecraftUsername.ToLowerInvariant().StartsWith(username) ||
                    p.Name.ToLowerInvariant().StartsWith(username))
                {
                    match.Add(p);
                }
            }
            if (match.Count == 1)
                return match[0];
            return null;
        }

        /// <summary>
        /// Gets the active player by name only NOT username.
        /// </summary>
        public static Client GetPlayerByName(string name)
        {
            name = name.ToLowerInvariant();
            //First run, exact match
            foreach (Client p in List)
            {
                if (p.Name.ToLowerInvariant() == name)
                    return p;
            }
            //Second run start of name
            List<Client> match = new List<Client>();
            foreach (Client p in List)
            {
                if (p.Name.ToLowerInvariant().StartsWith(name))
                    match.Add(p);
            }
            if (match.Count == 1)
                return match[0];
            else
                return null;
        }

        #region Tab list updater

        static Timer updater;

        public static void StartUpdater()
        {
            updater = new Timer(RunUpdater, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        public static void RunUpdater(object unusedTimerState)
        {
            try
            {
                if (MainClass.Active == false)
                {
                    updater.Dispose();
                    return;
                }
                
                //Debug.WriteLine("Player list Updater Thread Run");
                #if !DEBUG
                Threads.WatchdogCheck();
                #endif
                
                //Update external replies
                UpdateServerListPingReply();
                UpdateTabPlayers();
                
            }
            catch (Exception e)
            {
                Log.WriteServer(e);
            }

            try
            {
                //Write player count
                int rp = World.Main.Players.Length;
                int op = List.Length - rp;
                File.WriteAllText("playercount", MinecraftServer.MaxSlots + "\n" + rp + "\n" + op);

            }
            catch // (Exception e)
            {
                //Log.WriteServer(e);
            }
        }

        /// <summary>
        /// previously posted items so we know what we must remove
        /// </summary>
        static Dictionary<Client,string> prevList = new Dictionary<Client, string>();

        public static void UpdateTabPlayers()
        {
            try
            {
                var next = new Dictionary<Client, string>();
            
                var additem = new PlayerListItem(PlayerListItem.Actions.AddPlayer);
                var update = new PlayerListItem(PlayerListItem.Actions.UpdateLatency);

                //Active players
                foreach (Client a in List)
                {
                    //Send players own bold status
                    //var own = new PlayerListItem(PlayerListItem.Actions.AddPlayer);
                    //own.AddPlayer(header11, Chat.Bold + a.MinecraftUsername, (int)a.Ping);
                    //a.Queue.Queue(own);

                    string s = PlayerName(a);
                
                    if (a.Settings.Cloaked != null || a.MinecraftUsername == "Player")
                        continue;

                    if (prevList.ContainsKey(a))
                    {
                        string old = prevList[a];
                        if (old == s)
                        {
                            update.AddPlayer(a.UUID, s, (int)a.Ping);
                            prevList.Remove(a);
                        }
                        else
                            additem.AddPlayer(a.UUID, s, (int)a.Ping);
                    }
                    else
                    {
                        additem.AddPlayer(a.UUID, s, (int)a.Ping);
                    }

                    next.Add(a, s);
                }

                //Remove gone players
                if (prevList.Count > 0)
                {
                    var remove = new PlayerListItem(PlayerListItem.Actions.RemovePlayer);
                    foreach (var n in prevList.Keys)
                        remove.RemovePlayer(n.UUID);
                    QueueToAll(remove);
                }

                if (additem.Players.Count > 0)
                    QueueToAll(additem);
                if (update.Players.Count > 0)
                    QueueToAll(update);

                prevList = next;
            }
            catch (Exception e)
            {
                Log.WriteServer(e);
            }
        }

        /// <summary>
        /// The player representation in the player list
        /// </summary>
        static string PlayerName(Client a)
        {
            string s = a.Name;
            if (a.Session is VanillaSession)
            {
                if (a.Session.Mode == GameMode.Creative)
                    s = Chat.Italic + a.Name;
                else if (a.Session.Sleeping)
                    s = a.Name + Chat.DarkBlue + " zzZ";

            }
            else if (a.Session.World is GreenRoom)
                s = Chat.DarkGreen + a.Name;
            else if (a.Session.World is Hell)
                s = Chat.DarkRed + a.Name;
            else if (a.Session.World is TheConstruct)
                s = Chat.DarkGray + a.Name;
            else if (a.Session is AfkSession)
                s = Chat.DarkGray + a.Name + " AFK";
            else if (a.Session.World is MineProxy.Worlds.Void)
                s = Chat.Black + a.Name;

            var donor = Donors.GetDonor(a.Name);
            if (donor != null && donor.Expire > DateTime.Now)
            {
                string color = "";
                if (donor.Slots == 1)
                    color = Chat.Yellow;
                if (donor.Slots == 2)
                    color = Chat.Gold;
                if (donor.Slots >= 3)
                    color = Chat.Green;

                if (a.Session is VanillaSession)
                    s = color + s;
                else
                    s += color + ":)";
            }
            return s;
        }

        static void SendTabHeader(Client p)
        {
            //Donor donor = Donors.GetDonor(p.MinecraftUsername);

            PlayerListItem list = new PlayerListItem(PlayerListItem.Actions.AddPlayer);

            //Username ping is always sent
            //list.AddPlayer(header11, Chat.Bold + p.MinecraftUsername, 100);

            //Line 1, other 2 columns
            /*            if (donor == null || donor.ExpireLegacy < DateTime.Now)
                list.AddPlayer(header12, "You go " + Chat.Gold + "/donate");
            else
                list.AddPlayer(header12, Chat.Gold + "Donor");
            */

            /*if (p.Inbox == 0)
                list.AddPlayer(header13, Chat.Gray + "no mail", -1);
            else
                list.AddPlayer(header13, Chat.Red + p.Inbox + " mail /read", 0);
            */

            //Line 2
            //list.AddPlayer(header21, Chat.Gray + p.Country);

            /*if (p.Inbox == 0)
                list.AddPlayer(header23, "2");
            else
                list.AddPlayer(header23, Chat.Gray + "Read: " + Chat.Red + "/read");
            */

            //Line 3
            //list.AddPlayer(header31, "3");
            //list.AddPlayer(header32, "4");
            //list.AddPlayer(header33, "5");
            
            //list.AddPlayer(header41, Chat.Gray + Chat.Underline + "Players");
            //list.AddPlayer(header42, "6");
            //list.AddPlayer(header43, "7");

            //Full list of players
            foreach (var i in List)
            {
                list.AddPlayer(i.UUID, PlayerName(i), (int)i.Ping);
                //list.AddPlayer(i.UUID, i.MinecraftUsername, (int)i.Ping);
            }

            p.Queue.Queue(list);
        }

        #endregion

        /// <summary>
        /// Set world for all players
        /// </summary>
        /// <param name='real'>
        /// Real.
        /// </param>
        public static void SetRealWorldForAllInConstruct()
        {
            foreach (Client p in List)
            {
                if (p.Session.World != World.Construct)
                    continue;

                try
                {
                    p.SetWorld(World.Main);
                }
                catch (Exception e)
                {
                    Log.Write(e, p);
                }
            }
        }
    }
}

