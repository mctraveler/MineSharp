using System;
using MineProxy.Worlds;
using MineProxy.Packets;

namespace MineProxy.Commands
{
    public class Players
    {
        readonly CommandManager c;
        public Players(CommandManager c)
        {
            this.c = c;
            //CommandManager.AddTab(ParseTab, "help", "h");
            c.AddCommand(TheEnd, null, "theend");
            c.AddCommand(Position, "position", "pos", "coord", "coords", "cord");
            c.AddCommand(Time, null, "time", "timeset", "settime");
            c.AddCommand(Load, "load", "lag", "uptime", "l");
            c.AddCommand(Die, "kill", "suicide", "killmyself", "kil", "death", "die");
            c.AddCommand(Inbox.Read, "read");
            c.AddCommand(Stat, "stat");
            c.AddCommand(Donate, "donate", "donors", "donor", "doner", "donator", "dontae", "vip");
            c.AddCommand(Motd, "motd", "welcome", "info");
            c.AddCommand(Rules, "rulebook", "rules", "rule");
            c.AddCommand(Version, "version", "ver");
            c.AddCommand(Pardon, "pardon", "vote", "vot", "unban", "uban", "umbanned", "unbanned");
            c.AddCommand(ToGreenRoom, "greenroom", "wait");
            c.AddCommand(ToConstruct, "construct", "con", "cons");
            c.AddCommand(ToHell, "hell", "hel");
            c.AddCommand(ToRealWorld, "vanilla", "real", "back");
            c.AddCommand(ToIsland, "island");
            c.AddCommand(ToHardcore, "hardcore");
            c.AddCommand(ToWarp, "warp");
            c.AddCommand(LockChest, null, "lock", "lwc", "chest", "cprivate", "private", "lcpassword");
        }
    
        void ParseTab(Client player, string[] cmd, int iarg, TabComplete tab)
        {
            if (cmd.Length != 2)
                return;
        
            c.CompleteCommand(player, cmd [1], tab);
        }
    
        static void TheEnd(Client player, string[] cmd, int iarg)
        {
            player.Queue.Queue(new ChangeGameState(GameState.EndText));
            player.SetWorld(World.Void);
        }

        static void Position(Client player, string[] cmd, int iarg)
        {
            player.TellSystem(Chat.Yellow, "You are here: " + player.Session.Position);
        }

        static void Time(Client player, string[] cmd, int iarg)
        {
            player.TellSystem(Chat.Yellow, "Local Server Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            //int t = (int)((World.Real.Time.Time + 6000) % 24000);
            //player.Tell (Chat.Yellow, "Minecraft: " + (t / 1000) + ":" + ((t % 1000) * 60 / 1000).ToString ("00"));
        }

        static void Load(Client player, string[] cmd, int iarg)
        {

            player.TellSystem(Chat.Pink + "CPU: ", Chat.White + Lag.ServerLoad(true));
            var mu = Lag.ServerMemoryUsage();
            if (mu == null)
                player.TellSystem(Chat.Pink, "Error getting memory status");
            else
                player.TellSystem(Chat.Pink + "Memory: ", Chat.White + mu.Quota.ToString("0.0%") + " " + mu.UsedGB.ToString("0.000") + " / " + mu.TotalGB.ToString("0.000") + " GB");
        }
        static void Die(Client player, string[] cmd, int iarg)
        {
            VanillaSession real = player.Session as VanillaSession;
            if (real != null)
                real.Attacker = new Attacked(player);
            player.Session.Kill();
        }
        static void Stat(Client player, string[] cmd, int iarg)
        {
            if (cmd.Length == 1)
            {
                Chat.TellStatTo(player, player.Settings, player);
                player.TellSystem(Chat.Yellow + " ", Chat.Gold + Donors.Status(player.MinecraftUsername));
            }
            for (int n = 1; n < cmd.Length; n++)
            {
                ClientSettings s = null;
                Client p = PlayerList.GetPlayerByName(cmd [n]);
                if (p != null)
                    s = p.Settings;
                if (s == null || s.Cloaked != null)
                {
                    s = Client.LoadProxyPlayer(cmd [n]);
                    p = null;
                }
                if (s == null)
                    player.TellSystem(Chat.Red, cmd [n] + " is not found");
                else
                {
                    Chat.TellStatTo(p, s, player);
                    BadPlayer b = Banned.GetBanHistory(cmd [n]);
                    if (b != null)
                        player.TellSystem(Chat.Yellow + cmd [n] + " ", Chat.Red + b.ToString());
                }
            }
        }
        static void Donate(Client player, string[] cmd, int iarg)
        {
            Chat.ReadFile("donate.txt", Chat.Gold, player);
            player.TellSystem(Chat.Purple, Donors.Status(player.MinecraftUsername));
        }
        static void Motd(Client player, string[] cmd, int iarg)
        {
            Chat.ReadFile("motd.txt", "", player);
        }
        static void Rules(Client player, string[] cmd, int iarg)
        {
            RuleBook.Rules.GetRules(player);
        }
        static void Version(Client player, string[] cmd, int iarg)
        {
            player.TellSystem(Chat.Gray, "Proxy version: Latest");
            player.TellSystem(Chat.Gray, "Minecraft Vanilla: " + MinecraftServer.BackendVersion);
        }
        static void Pardon(Client player, string[] cmd, int iarg)
        {
            if (cmd.Length != 2)
                throw new ErrorException("Missing username");
                    
            if (player.Admin(Permissions.Ban) || Donors.IsDonor(player))
            {
                Banned.Pardon(player, cmd [1]);
                return;
            }
                    
            Banned.VotePardon(player, cmd [1]);
        }
        static void ToGreenRoom(Client player, string[] cmd, int iarg)
        {
            player.SetWorld(World.Wait);
        }
        static void ToConstruct(Client player, string[] cmd, int iarg)
        {
            player.SetWorld(World.Construct);
        }
        static void ToHell(Client player, string[] cmd, int iarg)
        {
            player.SetWorld(World.HellBanned);
        }
        static void ToRealWorld(Client player, string[] cmd, int iarg)
        {
            VanillaWorld w = World.Main;
            if (iarg < cmd.Length) //we got arguments
            {
                if (World.VanillaWorlds.ContainsKey(cmd [iarg]) == false)
                    throw new UsageException("No world named " + cmd [iarg]);
                w = World.VanillaWorlds [cmd [iarg]];
            }
            player.SetWorld(w);
        }
        static void ToIsland(Client player, string[] cmd, int iarg)
        {
            player.SetWorld(World.VanillaWorlds ["island"]);
        }
        static void ToHardcore(Client player, string[] cmd, int iarg)
        {
            player.SetWorld(World.VanillaWorlds ["hardcore"]);
        }
        static void ToWarp(Client player, string[] cmd, int iarg)
        {
            var vanilla = player.Session.World as VanillaWorld;
            if (vanilla == null || vanilla.ServerName != "old")
            {
                player.TellSystem("", "/warp only works in /vanilla main");
                return;
            }
            player.Session.World.Send("tp " + player.MinecraftUsername + " 150709 66 -150714");
            //old
            //player.SetWorld(World.Castles.Get("main"));
        }
        static void LockChest(Client player, string[] cmd, int iarg)
        {
            player.TellSystem(Chat.Gray, "All chests inside protected regions are, well protected");
            if (player.Uptime.TotalDays > 2)
                player.TellSystem(Chat.Gray, "Use /region start");
            else
                player.TellSystem(Chat.Gray, "Use \"/ticket region\" or /donate if you're out of tickes, or play for 48h then you can use /reg start.");
        }
    }
}

