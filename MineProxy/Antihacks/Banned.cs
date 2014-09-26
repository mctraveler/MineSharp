using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using MineProxy.Chatting;
using MineProxy.Worlds;
using Newtonsoft.Json;
using System.Text;

namespace MineProxy
{
    public static class Banned
    {
        const string path = "proxy/banned.json";
        static BlackList blacklist;

        static List<IPAddress> BannedIP = new List<IPAddress>();

        public static void LoadBanned()
        {
            if (File.Exists(path) == false)
            {
                blacklist = new BlackList();
                blacklist.List = new List<BadPlayer>();
                return;
            }
            blacklist = Json.Load<BlackList>(path);
        }
        
        public static void SaveBanned()
        {
            lock (blacklist)
            {
                Json.Save<BlackList>(path, blacklist);
            }
        }

        internal static bool IsBanned(IPAddress ip)
        {
            lock (BannedIP)
                return BannedIP.Contains(ip);
        }

        /// <summary>
        /// True if banned, false if already banned
        /// </summary>
        /// <param name='ip'>
        /// Ip.
        /// </param>
        internal static bool Ban(IPAddress ip)
        {
            lock (BannedIP)
            {
                if (BannedIP.Contains(ip))
                    return false;

                BannedIP.Add(ip);
                return true;
            }
        }
        

        internal static BadPlayer CheckBanned(Client player)
        {
            return CheckBanned(player.MinecraftUsername);
        }

        internal static BadPlayer CheckBanned(string unverifiedUsername)
        {
            BadPlayer b = GetBanHistory(unverifiedUsername);
            if (b == null)
                return null;
            if (b.BannedUntil < DateTime.Now)
                return null;
            return b;
        }

        internal static void MassPardon()
        {
            lock (blacklist)
            {
                var now = DateTime.Now;
                int pardoned = 0;
                foreach (BadPlayer b in blacklist.List)
                {
                    if (b.BannedUntil > now)
                    {
                        //Still banned, remove player items
                        string dat = "main/world/players/" + Path.GetFileName(b.Username) + ".dat";
                        string datbanned = dat + "-banned";
                        if (File.Exists(dat) == false)
                            continue;
                        if (File.Exists(datbanned))
                            File.Delete(datbanned);
                        File.Move(dat, datbanned);

                        pardoned += 1;
                    }

                    b.BannedUntil = DateTime.Now;
                }
                SaveBanned();
                Chatting.Parser.TellAdmin("Masspardon: pardoned " + pardoned + " players");
            }
        }

        internal static BadPlayer GetBanHistory(string unverifiedUsername)
        {
            lock (blacklist)
            {
                foreach (BadPlayer b in blacklist.List)
                {
                    if (b.Username == unverifiedUsername)
                        return b;
                }
            }
            return null;
        }

        public static void Ban(Client admin, string username, DateTime bannedUntil, string reason)
        {
            if (admin != null && admin.Admin() == false)
            {
                admin.TellSystem(Chat.Yellow, "Disabled");
                return;
            }
            
            if (Donors.IsDonor(username) && bannedUntil < DateTime.Now.AddMinutes(35))
            {
                Log.WritePlayer(username, "Donor not Banned: " + reason);
                if (admin != null)
                    admin.TellSystem(Chat.Gold, "Donor not banned for: " + reason);
                return;
            }

            bool newban = false;
            BadPlayer b = GetBanHistory(username);
            if (b == null)
            {
                //Make sure we spelled correctly
                if (admin != null)
                {
					if (File.Exists("proxy/players/" + Path.GetFileName(username) + ".json") == false)
                    {
                        admin.TellSystem(Chat.Red, "No such player: " + username);
                        return;
                    }
                }

                newban = true;
                b = new BadPlayer();
                b.Username = username;
                b.BannedUntil = bannedUntil;
                b.Reason = reason;
                lock (blacklist)
                    blacklist.List.Add(b);
            } else
            {
                if (b.BannedUntil < DateTime.Now)
                    newban = true;

                //Make sure longer bans are not removed by a shorter violation
                if (b.BannedUntil > bannedUntil) 
                    return;
                b.BannedUntil = bannedUntil;
                b.Reason = reason;
            }
            SaveBanned();

            //Console.WriteLine ("Banning " + b.Username + " for " + b.Reason);
            double banMinutes = (b.BannedUntil - DateTime.Now).TotalMinutes;
            string banlength = "forever";
            if (banMinutes < 24 * 60 * 30)
                banlength = banMinutes.ToString("0") + " minutes";

            if (admin != null)
                Log.WritePlayer(b.Username, "Banned " + banlength + " by " + admin.MinecraftUsername + ": " + reason);
            else
                Log.WritePlayer(b.Username, "Banned " + banlength + ": " + reason);

            Client pp = PlayerList.GetPlayerByUsernameOrName(username);
            if (pp != null)
            {
                if (newban)
                    Chatting.Parser.Say(Chat.Purple, pp.Name + " is banned " + banlength + ": " + reason);
                if (pp.Session is HellSession == false)
                    pp.SetWorld(World.HellBanned);
            } else
            {
                admin.TellSystem(Chat.Purple, username + " was banned(offline): " + reason);
            }
        }
        
        public static void Kick(string username, string reason)
        {
            Client pp = PlayerList.GetPlayerByUsernameOrName(username);
            if (pp != null)
            {
                pp.Kick(reason);
                return;
            }
        }
        
        public static void Pardon(Client admin, string username)
        {
            BadPlayer pardoned = null;
            lock (blacklist)
            {
                foreach (BadPlayer b in blacklist.List)
                {
                    if (b.Username.ToLowerInvariant() != username.ToLowerInvariant())
                        continue;

                    //Remove history
                    if (admin.Admin() && b.BannedUntil < DateTime.Now)
                    {
                        blacklist.List.Remove(b);
                        SaveBanned();
                        admin.TellSystem(Chat.Purple, "Ban record removed");
                        return;
                    }

                    pardoned = b;
                    break;
                }
            }
            if (pardoned != null)
            {
                if (admin != null && admin.Admin() == false && pardoned.BannedUntil > DateTime.Now.AddMinutes(30))
                {
                    admin.TellSystem(Chat.Purple, "Sorry players banned for longer periods of time cannot be pardoned");
                    return;
                }
                if (admin == null || admin.Settings.Cloaked != null)
                    PardonBad(pardoned, "Server");
                else
                    PardonBad(pardoned, admin.Name);
            } else
            {
                if (admin != null)
                    admin.TellSystem(Chat.Red, username + " not found");
            }
        }
        
        static void PardonBad(BadPlayer bad, string who)
        {
            bad.BannedUntil = DateTime.Now;
            SaveBanned();
            
            Client c = PlayerList.GetPlayerByName(bad.Username);
            if (c != null)
                c.SetWorld(World.Main);

            Log.WritePlayer(bad.Username, " pardoned by " + who);
            Chatting.Parser.Say(Chat.Purple, bad.Username + " unbanned by " + who);
        }
        
        public static void VotePardon(Client player, string username)
        {
            if (player.Uptime < TimeSpan.FromHours(8))
            {
                player.TellSystem(Chat.Red, "You need 8 hours gametime befor you can vote.");
                return;
            }
                
            BadPlayer b = CheckBanned(username);
            if (b == null)
            {
                player.TellSystem(Chat.Red, "Player not banned: " + username);
                return;
            }
                
            //Block permanent bans
            if (b.BannedUntil > DateTime.Now.AddMinutes(30))
            {
                player.TellSystem(Chat.Yellow, b.Username + " is banned by admin");
                player.TellSystem(Chat.Yellow, "Contact " + MinecraftServer.AdminEmail + " to request unban.");
                return;
            }
                
            lock (b.UnbanVote)
            {
                if (b.UnbanVote.ContainsKey(player.MinecraftUsername))
                {
                    player.TellSystem(Chat.Yellow, "Already voted for unbanning " + b.Username);
                    return;
                }
                    
                b.UnbanVote.Add(player.MinecraftUsername, player);
                Chatting.Parser.Say(Chat.Purple, player.Name + " voted to unban " + b.Username);
                Log.WritePlayer(player, "Voted unban " + b.Username);
                    
                //Check if enough votes
                int votesLeft = 3 - b.UnbanVote.Count;
                TimeSpan voteTimeLeft = TimeSpan.FromDays(3);
                foreach (Client v in b.UnbanVote.Values)
                {
                    voteTimeLeft -= v.Uptime;
                }
                    
                if (votesLeft <= 0 && voteTimeLeft.Ticks < 0)
                {
                    //Unban
                    string who = "";
                    foreach (var ben in b.UnbanVote.Values)
                        who += ben.Name + ",";
                    who = who.TrimEnd(',', ' ');
                    PardonBad(b, "vote from " + who);
                    return;
                } else
                {
                    string m = b.Username + " need ";
                    if (votesLeft > 0)
                        m += votesLeft + " votes";
                    if (voteTimeLeft.Ticks > 0)
                    {
                        if (votesLeft > 0)
                            m += " and ";
                        m += voteTimeLeft.TotalHours.ToString("0,0") + " gametime votes";
                    }
                    Chatting.Parser.Say(Chat.Purple, m);
                    return;
                }
            }
        }
    }
}

