using System;
using MineProxy.Misc;
using MineProxy.Packets;

namespace MineProxy.Commands
{
    class MainCommands : CommandManager
    {
        static CommandManager Instance;

        public static void Init()
        {
            Instance = new MainCommands();
        }

        public static void ParseClientTab(Client player, TabCompleteClient tab)
        {
            string[] cmd = tab.Command.Substring(1).Replace("  ", " ").Split(' ');
            cmd [0] = cmd [0].Trim().Trim('/');

            var tc = new TabComplete();
                
            var commander = player.Session.World.Commands;
            if (commander != null)
            {
                player.Session.World.Commands.ParseTab(player, cmd, 1, tc);
                if (tc.Alternatives.Count > 0)
                {
                    player.Queue.Queue(tc);
                    return;
                }
            }
            
            Instance.ParseTab(player, cmd, 1, tc);
            if (tc.Alternatives.Count > 0)
            {
                player.Queue.Queue(tc);
                return;
            }

            if (cmd.Length > 1)
            {
                Client p;
                if (player.Admin(Permissions.AnyAdmin))
                    p = PlayerList.GetPlayerByUsernameOrName(cmd [cmd.Length - 1]);
                else
                    p = PlayerList.GetPlayerByName(cmd [cmd.Length - 1]);
                if (p != null)
                {
                    player.Queue.Queue(TabComplete.Single(p.Name));
                }
            }
        }


        public static void ParseClientCommand(Client player, string message)
        {
            string [] cmd = null;
            try
            {
                cmd = message.Replace("  ", " ").Split(' ');
                cmd [0] = cmd [0].Trim().Trim('/').ToLowerInvariant();
                string command = cmd [0];
                
                if (command == "")
                    return;

                CommandManager cm = player.Session.World.Commands;
                if (cm != null && cm.ParseCommand(player, cmd, 1))
                    return;
                
                if (Instance.ParseCommand(player, cmd, 1))
                    return;
                
                Help.ShowHelp(player, "help", cmd.JoinFrom(0));
                
            } catch (FormatException fe)
            {
                player.TellSystem(Chat.Red, fe.Message);
                return;
            } catch (ErrorException ee)
            {
                player.TellSystem(Chat.Red + "Error: ", ee.Message);
                return;
            } catch (UsageException ue)
            {
                player.TellSystem(Chat.Yellow + "Usage: ", ue.Message);
                return;
            } catch (ShowHelpException)
            {
                Help.ShowHelp(player, "help", cmd.JoinFrom(0));
            }
        }

        public MainCommands()
        {
            try
            {
                new Help(this);
                new RegionCommands(this);
                new Admin(this);
                Cloak.Init(this);
                new Players(this);
                new ChatCommands(this);
                TexturePackSetter.Init(this);
                Chatting.Translator.Init(this);
            } catch (Exception e)
            {
                Log.WriteServer(e);
            }
        }
    }
}

