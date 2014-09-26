using System;
using System.IO;
using MineProxy.Packets;

namespace MineProxy.Commands
{
    /// <summary>
    /// Help commands
    /// </summary>
    public class Help
    {
        readonly CommandManager c;

        public Help(CommandManager c)
        {
            this.c = c;
            c.AddTab(ParseTab, "help", "h");
            c.AddCommand(ParseCommandHelp, "help", "h");
            
            c.AddCommand(ParseCommandChatHelp, "chathelp");
        }

        void ParseTab(Client player, string[] cmd, int iarg, TabComplete tab)
        {
            if (cmd.Length <= iarg)
                return;
            
            c.CompleteCommand(player, cmd [iarg - 1], tab);
        }

        static void ParseCommandChatHelp(Client player, string[] cmd, int iarg)
        {
            player.Settings.Help = !player.Settings.Help;

            if (player.Settings.Help)
                player.TellSystem(Chat.Green, "Chat guide will show");
            else
                player.TellSystem(Chat.Green, "Chat guide is now hidden");
        }

        static void ParseCommandHelp(Client player, string[] cmd, int iarg)
        {
            if (cmd.Length < 2)
            {
                ShowHelp(player, "help", "help");
                return;
            }

            Help.ShowHelp(player, "help", cmd.JoinFrom(1));
        }

        public static void ShowHelp(Client player, string path, string command)
        {
            player.TellSystem(Chat.DarkGreen, "///////// " + Chat.Green + "help: /" + command);
            if (FindShowHelp(player, path, command) == false)
                player.TellSystem(Chat.Red, "Help/command not found");
            player.TellSystem(Chat.DarkGreen, "/////////");
        }

        static bool FindShowHelp(Client player, string path, string command)
        {
            if (command == "" || command == null)
                return false;

            if (player.Admin())
            {
                string apath = Path.Combine(Path.Combine(path, "admin"), Path.GetFileName(command));
                if (Chat.ReadFile(apath, Chat.Pink, player))
                    return true;
            }
            
            string filepath = Path.Combine(path, Path.GetFileName(command));
            if (Chat.ReadFile(filepath, Chat.Green, player))
                return true;

            //Try with one argument less
            int pos = command.LastIndexOf(' ');
            if (pos < 0)
                return false;
            return FindShowHelp(player, path, command.Substring(0, pos));
        }
    }
}

