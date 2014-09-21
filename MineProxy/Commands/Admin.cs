using System;
using System.IO;
using System.Collections.Generic;
using MineProxy.Worlds;
using MineProxy.Packets;
using MineProxy.Chatting;

namespace MineProxy.Commands
{
    public class Admin
    {
        public Admin(CommandManager c)
        {
            c.AddTab(GameRuleComplete, "gamerule");
            c.AddAdminCommand(MassPardon, "masspardon");
            c.AddAdminCommand(KillJava, "killjava");
            c.AddAdminCommand(NickCommand, "nick");
            c.AddAdminCommand(Say, "say");
            c.AddAdminCommand(Say2, "say2");
            c.AddAdminCommand(Kick, "kick");
            c.AddAdminCommand(Ban, "ban");
            c.AddAdminCommand(BanIP, "ban-ip", "banip");
            c.AddAdminCommand(CleanBannedRegions, "cleanregionsbanned");
            c.AddAdminCommand(VanillaCommands, "give");
            c.AddAdminCommand(VanillaCommands, "enchant");
            c.AddAdminCommand(VanillaCommands, "difficulty");
            c.AddAdminCommand(VanillaCommands, "gamerule");
            c.AddAdminCommand(VanillaCommands, "spawnpoint");
            c.AddAdminCommand(VanillaCommands, "clear");
            c.AddAdminCommand(VanillaCommands, "gamemode");
            c.AddAdminCommand(VanillaCommands, "effect");
            c.AddAdminCommand(VanillaCommands, "scoreboard");
            c.AddAdminCommand(VanillaCommands, "weather");
            c.AddAdminCommand(VanillaCommands, "toggledownfall");
            c.AddAdminCommand(VanillaCommands, "setidletimeout");
            c.AddAdminCommand(VanillaCommands, "op", "deop");

            c.AddAdminCommand(VanillaCommands, "achievement");
            c.AddAdminCommand(VanillaCommands, "blockdata");
            c.AddAdminCommand(VanillaCommands, "clone");
            c.AddAdminCommand(VanillaCommands, "defaultgamemode");
            c.AddAdminCommand(VanillaCommands, "execute");
            c.AddAdminCommand(VanillaCommands, "fill");
            c.AddAdminCommand(VanillaCommands, "particle");
            c.AddAdminCommand(VanillaCommands, "playsound");
            c.AddAdminCommand(VanillaCommands, "setblock");
            c.AddAdminCommand(VanillaCommands, "summon");
            c.AddAdminCommand(VanillaCommands, "title");

            c.AddAdminCommand(VanillaLoad, "vanillaload");
            c.AddAdminCommand(VanillaUnload, "vanillaunload");
            c.AddAdminCommand(VanillaStop, "vanillarestart");
            c.AddAdminCommand(AllBack, "allback");
            c.AddAdminCommand(OldRestart, null, "restart", "shutdown", "stop");
            c.AddAdminCommand(ProxyStop, "proxystop");
            c.AddAdminCommand((player, cmd, iarg) =>
            {
                Threads.DebugThreads();
            }, "debugthreads");
            c.AddAdminCommand((player, cmd, iarg) =>
            {
                player.TellSystem(Chat.Pink, "New Usage: /ban 30 username Reason for ban");
            }, null, "ban30");
            
            c.AddAdminCommand(VanillaSuspend, "vanillastop");
            c.AddAdminCommand(VanillaResume, "vanillastart");
            c.AddAdminCommand(Possess, "possess", "poss");
            c.AddAdminCommand(Slots, "slots");
            c.AddAdminCommand(Flush, "flush");
            c.AddAdminCommand(Mode, "mode", "gm", "m");
            c.AddAdminCommand(Teleport, "tp", "go");
            c.AddAdminCommand(Crash, "crash");
            
            c.AddAdminCommand((player, cmd, offset) =>
            {
                if (cmd.Length != 4)
                    throw new UsageException("/ee effectID amplification duration");
                
                var ee = new EntityEffect(player.EntityID, (PlayerEffects)int.Parse(cmd [1]), int.Parse(cmd [2]), int.Parse(cmd [3]));
                player.Queue.Queue(ee);
                player.TellSystem(Chat.Purple, ee.ToString());
            }, "ee");

#if DEBUG
            c.AddCommand((player, cmd, offset) =>
            {
                var steer = new Steer();
                steer.Action = Steer.Actions.Unmount;
                player.FromClient(steer);

            }, "steer");
#endif
        }

        void GameRuleComplete(Client player, string[] cmd, int iarg, TabComplete tab)
        {
            if (cmd.Length != 2)
                return;

            string[] rules = new string[]
            {
                "doFireTick",
                "mobGriefing",
                "keepInventory",
                "doMobSpawning",
                "doMobLoot",
                "doTileDrops",
            };
            CommandManager.Complete(rules, cmd [1], tab);
        }

        void VanillaSuspend(Client player, string[] cmd, int iarg)
        {
            if (player.Admin(Permissions.Server) == false)
                throw new ErrorException("Disabled");

            VanillaWorld w = player.Session.World as VanillaWorld;
            if (iarg < cmd.Length)
                w = World.VanillaWorlds [cmd [iarg]];

            if (w == null)
                throw new UsageException("No vanilla world with the name " + cmd [iarg]);

            w.Suspended = true;
            w.StopBackend();

            Log.WriteChat(player, null, -1, "[Suspend]");
        }

        void VanillaResume(Client player, string[] cmd, int iarg)
        {
            if (player.Admin(Permissions.Server) == false)
                throw new ErrorException("Disabled");

            VanillaWorld w = player.Session.World as VanillaWorld;
            if (iarg < cmd.Length)
                w = World.VanillaWorlds [cmd [iarg]];

            if (w == null)
                throw new UsageException("No vanilla world with the name " + cmd [iarg]);

            w.Suspended = false;
            w.StartBackend();
            Log.WriteChat(player, null, -1, "[Resume]");
        }

        void Possess(Client player, string[] cmd, int iarg)
        {
            if (player.Admin(Permissions.Cloak) == false)
                throw new ErrorException("Disabled");
            if (cmd.Length != 2)
                throw new UsageException("Usage: /possess username");

            var victim = PlayerList.GetPlayerByUsernameOrName(cmd [1]);
            if (victim == null)
                throw new ErrorException("User not found " + cmd [1]);

            player.SetSession(new PossessSession(player, victim));
            player.TellAbove(Chat.Yellow, "Possessing " + victim.Name);
        }

        void Slots(Client player, string[] cmd, int iarg)
        {
            int maxSlots;
            if (cmd.Length != 2)
                throw new UsageException("/slots " + MinecraftServer.MaxSlots);
            if (false == int.TryParse(cmd [1], out maxSlots))
                throw new ErrorException("Invalid number");
            MinecraftServer.MaxSlots = maxSlots;
            player.TellSystem(Chat.Purple, "Slots set to " + MinecraftServer.MaxSlots);
        }

        void Flush(Client player, string[] cmd, int iarg)
        {
            Log.Flush();
            player.TellAbove(Chat.Purple, "Flushed logs");
        }

        void Mode(Client player, string[] cmd, int iarg)
        {
            if (!player.AdminAny(Permissions.CreativeBuild | Permissions.CreativeFly))
                throw new ErrorException("Disabled");
            
            if (cmd.Length == 2)
            {
                player.Session.World.Send("gamemode " + cmd [1] + " " + player.MinecraftUsername);
                player.TellAbove(Chat.Yellow, "You are now mode " + cmd [1]);
            } else
                player.TellSystem(Chat.Yellow, "/m  [0/1/2]");
        }

        void Teleport(Client player, string[] cmd, int iarg)
        {
            if (!player.AdminAny(Permissions.CreativeBuild | Permissions.CreativeFly))
                throw new ErrorException("Disabled");
            
            if (cmd.Length < 2)
                throw new ShowHelpException();
            
            if (cmd.Length == 2)
            {
                
                Client toPlayer = PlayerList.GetPlayerByUsernameOrName(cmd [1]);
                if (toPlayer == null)
                    throw new ErrorException(cmd [1] + " not found");

                if (player.Session.Dimension != toPlayer.Session.Dimension)
                {
                    //Does not work in vanilla yet, make warp
                    player.Warp(toPlayer.Session.Position, toPlayer.Session.Dimension, toPlayer.Session.World);
                } else
                    player.Session.World.Send("tp " + player.MinecraftUsername + " " + toPlayer.MinecraftUsername);
                return;
            }
            
            
            if (cmd.Length == 3)
            {
                
                Client fromPlayer = PlayerList.GetPlayerByUsernameOrName(cmd [1]);
                Client toPlayer = PlayerList.GetPlayerByUsernameOrName(cmd [2]);
                if (toPlayer == null || fromPlayer == null)
                {
                    if (fromPlayer == null)
                        player.TellSystem(Chat.Red, cmd [1] + " not found");
                    if (toPlayer == null)
                        player.TellSystem(Chat.Red, cmd [2] + " not found");
                    return;
                }
                
                if (fromPlayer.Session.Dimension != toPlayer.Session.Dimension)
                {
                    //Does not work in vanilla yet, make warp
                    fromPlayer.Warp(toPlayer.Session.Position, toPlayer.Session.Dimension, toPlayer.Session.World);
                } else
                    player.Session.World.Send("tp " + fromPlayer.MinecraftUsername + " " + toPlayer.MinecraftUsername);
                
                return;
            }
            
            //Here we have at least 3 args(cmd.length >= 4)
            if (cmd.Length < 4)
                throw new UsageException("Missing: /tp x y z [dim]");
            
            CoordDouble pos = new CoordDouble(cmd [1], cmd [2], cmd [3]);
            Dimensions dimension = player.Session.Dimension;
            if (cmd.Length > 4)
                dimension = (Dimensions)int.Parse(cmd [4]);
            player.TellSystem(Chat.Purple, "warping to " + pos);
            player.Warp(pos, dimension, player.Session.World);
        }

        void Crash(Client player, string[] cmd, int iarg)
        {
            if (!player.AdminAny(Permissions.Ban))
                throw new ErrorException("Disabled");
            
            if (cmd.Length != iarg + 2)
                throw new ShowHelpException();

            Client c = PlayerList.GetPlayerByUsernameOrName(cmd [iarg]);
            if (c == null)
                throw new ErrorException("player not found: " + cmd [iarg]);
                
            switch (cmd [iarg + 1])
            {
                case "creeper":
                        //Slows down the player
                    player.TellSystem(Chat.Green, "Sending 5000 Creepers to " + c.Name);
                    var msl = new List<PacketFromServer>();
                    for (int n = 1; n < 5000; n++)
                    {
                        SpawnMob ms = new SpawnMob(MobType.Creeper);
                        ms.Pos = c.Session.Position;
                        ms.EID = n;
                        //TODO: fix valid metadata or else this only becomes a quick crash
                        msl.Add(ms);
                    }
                    c.Queue.Queue(msl);
                    return;
                        
                case "zombie":
                    SpawnMob sm = new SpawnMob(MineProxy.MobType.Zombie);
                    sm.Metadata = new Metadata();
                    c.SendToClient(sm);
                    c.Close("Internal Server Error");
                    return;
            }
        }

        void MassPardon(Client player, string[] cmd, int iarg)
        {
            Banned.MassPardon();
            player.TellSystem(Chat.Purple, "Pardoned all players");
        }

        void KillJava(Client player, string[] cmd, int iarg)
        {
            if (!player.AdminAny(Permissions.Server))
                throw new ErrorException("Disabled");
            
            player.TellSystem(Chat.Purple, "kill -9, take that java");
            System.Diagnostics.Process.Start("pkill", "-kill java");
        }

        void NickCommand(Client player, string[] cmd, int iarg)
        {
            //Only nuxas for now
            if (player.MinecraftUsername != "Player" &&
                player.MinecraftUsername != "nuxas")
                throw new ErrorException("Disabled");
            
            if (cmd.Length <= 1)
            {
                player.TellSystem(Chat.Purple, Commands.Nick.Get(player));
                return;
            }

            Client p = PlayerList.GetPlayerByUsername(cmd [1]);
            if (p == null)
            {
                player.TellSystem(Chat.Red, "Player not found: " + cmd [1]);
                return;
            }
            if (cmd.Length == 2)
            {
                player.TellSystem(Chat.Purple, Nick.Get(p));
                return;
            }
            if (cmd.Length == 3)
            {
                string nick = cmd [2].Replace("%", "§");
                if (nick.Length > 16)
                    nick = nick.Substring(0, 16);
                    
                Nick.Set(p, nick);
                Log.WritePlayer(player, "Changed nick for " + p.MinecraftUsername + " to " + nick);
                player.TellSystem(Chat.Purple, Nick.Get(p));
                if (p != player)
                    p.TellSystem(Chat.Purple, Nick.Get(p));
                p.SetWorld(p.Session.World);
                return;
            }
            player.TellSystem(Chat.Red, "too many arguments");
            throw new ShowHelpException();
        }

        void Say(Client player, string[] cmd, int iarg)
        {
            Chatting.Parser.Say(Chat.Pink + "[Server] ", Chatting.Spelling.SpellFormat(cmd.JoinFrom(1)));
            Log.WriteChat(player, null, -1, "[Server] " + cmd.JoinFrom(1));
        }

        void Say2(Client player, string[] cmd, int iarg)
        {
            Chatting.Parser.Say(Chat.Pink + "[Server] ", Chatting.Spelling.SpellFormat(cmd.JoinFrom(1)), ChatPosition.AboveActionBar);
            Log.WriteChat(player, null, -1, "[Server] " + cmd.JoinFrom(1));
        }

        void Kick(Client player, string[] cmd, int iarg)
        {
            if (!player.AdminAny(Permissions.Ban))
                throw new ErrorException("Disabled");
            if (cmd.Length < 3)
                throw new UsageException("Missing argument, use: /kick username reason for being kicked  ");
            
            Client pp = PlayerList.GetPlayerByUsernameOrName(cmd [1]);
            if (pp == null)
                player.TellSystem(Chat.Yellow, cmd [1] + Chat.Blue + " is not online");
            else
                pp.Kick(cmd.JoinFrom(2));
        }

        void Ban(Client player, string[] cmd, int iarg)
        {
            if (cmd.Length < 3)
                throw new UsageException("Usage: /ban [minutes] username Reason for ban");
            
            string username;
            string reason;
            DateTime expire;
            int minutes;
            if (int.TryParse(cmd [1], out minutes))
            {
                expire = DateTime.Now.AddMinutes(minutes);
                username = cmd [2];
                reason = cmd.JoinFrom(3);
            } else
            {
                expire = DateTime.MaxValue;
                username = cmd [1];
                reason = cmd.JoinFrom(2);
            }
            
            Banned.Ban(player, username, expire, reason);
        }

        void BanIP(Client player, string[] cmd, int iarg)
        {
            if (!player.AdminAny(Permissions.Ban))
                throw new ErrorException("Disabled");
            if (cmd.Length != 2)
                throw new ShowHelpException();

            var bip = PlayerList.GetPlayerByUsernameOrName(cmd [1]);
            if (bip == null)
                throw new ErrorException("Player offline, no IP");

            if (Banned.Ban(bip.RemoteEndPoint.Address))
                player.TellSystem(Chat.Red, "Banned IP for " + bip.Name);
            else
                player.TellSystem(Chat.Red, "IP already banned for " + bip.Name);
        }

        void CleanBannedRegions(Client player, string[] cmd, int iarg)
        {
            if (player.Admin(Permissions.Region) == false)
                throw new ErrorException("Disabled");
            RegionLoader.CleanBanned(player.Session.World.Regions);
        }

        void VanillaCommands(Client player, string[] cmd, int iarg)
        {
            if (player.AdminAny(Permissions.CreativeBuild | Permissions.Ban) == false)
                throw new ErrorException("Disabled");
            player.TellSystem(Chat.Purple, "Sending command to vanilla backend");
            player.TellSystem(Chat.Purple, cmd.JoinFrom(0));
            player.Session.World.Send(cmd.JoinFrom(0));
        }

        void VanillaStop(Client player, string[] cmd, int iarg)
        {
            if (player.Admin(Permissions.Server) == false)
                throw new ErrorException("Disabled");
            player.TellSystem(Chat.Purple, "Sending stop command to vanilla backend");
            player.Session.World.Send("stop");
            BackupProxy.LastVanillaRestart = DateTime.Now;
        }

        void VanillaLoad(Client player, string[] cmd, int iarg)
        {
            if (player.Admin(Permissions.Server) == false)
                throw new ErrorException("Disabled");

            string name = cmd [iarg];

            if (World.VanillaWorlds.ContainsKey(name))
                throw new ErrorException("World already loaded");

            try
            {
                VanillaWorld w = new VanillaWorld(name);
                World.VanillaWorlds.Add(name, w);
            } catch (FileNotFoundException)
            {
                throw new ErrorException("No such world: " + name);
            }
            player.TellSystem(Chat.Purple, "Loaded world " + name);
        }

        void VanillaUnload(Client player, string[] cmd, int iarg)
        {
            if (player.Admin(Permissions.Server) == false)
                throw new ErrorException("Disabled");

            string name = cmd [iarg];

            if (World.VanillaWorlds.ContainsKey(name) == false)
                throw new ErrorException("World not loaded");

            VanillaWorld w = World.VanillaWorlds [name];
            World.VanillaWorlds.Remove(name);

            w.Suspended = true;
            w.StopBackend();
            player.TellSystem(Chat.Purple, "Unloaded world " + name);
        }

        void AllBack(Client player, string[] cmd, int iarg)
        {
            player.TellSystem(Chat.Purple, "Bringin back all from the /con");
            PlayerList.SetRealWorldForAllInConstruct();
        }

        void OldRestart(Client player, string[] cmd, int iarg)
        {
            if (player.Admin(Permissions.Server) == false)
                throw new ErrorException("Disabled");
            player.TellSystem(Chat.Yellow, "Changed these commands to /vanillastop, /proxystop");
        }

        void ProxyStop(Client player, string[] cmd, int iarg)
        {
            if (player.Admin(Permissions.Server) == false)
                throw new ErrorException("Disabled");
            string bye = cmd.JoinFrom(1);
            Chatting.Parser.Say(Chat.Pink, "[Restarting] " + bye);
            MainClass.Shutdown(bye);
            Log.WriteChat(player, null, -1, "[Restarting] " + bye);
        }
       
    }
}

