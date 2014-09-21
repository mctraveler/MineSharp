using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Diagnostics;
using MineProxy.Packets;
using MineProxy.Commands;

namespace MineProxy
{
    public delegate void CommandTab(Client player,string[] cmd,int offset,TabComplete tab);
    public class CommandManager
    {
        readonly Dictionary<string, Action<Client,string[],int>> CommandParse = new Dictionary<string, Action<Client,string[],int>>();
        readonly Dictionary<string, Action<Client,string[],int>> AdminCommandParse = new Dictionary<string, Action<Client,string[],int>>();
        readonly Dictionary<string, CommandTab> CommandTabs = new Dictionary<string, CommandTab>();
        /// <summary>
        /// These strings are used for matching tab-completion
        /// </summary>
        readonly List<string> CommandStrings = new List<string>(){ };
        /// <summary>
        /// Used for admin tab-completion
        /// </summary>
        readonly List<string> AdminStrings = new List<string>(){ };

        public void AddCommand(Action<Client,string[],int> action, params string[] commands)
        {
            if (commands [0] != null)
                CommandStrings.Add(commands [0]);
            foreach (string c in commands)
            {
                if (c == null)
                    continue;
                CommandParse.Add(c, action);
            }
        }

        public void AddAdminCommand(Action<Client,string[],int> action, params string[] commands)
        {
            if (commands [0] != null)
                AdminStrings.Add(commands [0]);
            foreach (string c in commands)
            {
                if (c == null)
                    continue;
                AdminCommandParse.Add(c, action);
            }
        }

        /// <summary>
        /// Adds tab completion for a specific command
        /// </summary>
        public void AddTab(CommandTab action, params string[] commands)
        {
            foreach (string c in commands)
                CommandTabs.Add(c, action);
        }

        public static void Complete(IEnumerable<string> list, string cmd, TabComplete tab)
        {
            if (cmd == "")
            {
                foreach (string s in list)
                {
                    tab.Alternatives.Add("/" + s);
                }
            } else
            {
                foreach (string s in list)
                {
                    if (s.StartsWith(cmd))
                        tab.Alternatives.Add("/" + s);
                }
            }
        }

        /// <summary>
        /// Completes the command.
        /// </summary>
        /// <returns>The completion or null if no match was found.</returns>
        /// <param name="player">Player.</param>
        /// <param name="c">command without the leading /</param>
        public void CompleteCommand(Client player, string c, TabComplete tab)
        {
            if (player.Admin(Permissions.AnyAdmin))
                Complete(AdminStrings, c, tab);
            Complete(CommandStrings, c, tab);
        }

        public virtual void ParseTab(Client player, string[] cmd, int iarg, TabComplete tab)
        {
            if (cmd.Length < iarg) //Show all commands
            {
                CompleteCommand(player, "", tab);
                return;
            }
            
            if (cmd.Length == iarg) //Complete command
            {
                cmd [iarg - 1] = cmd [iarg - 1].ToLowerInvariant();
                CompleteCommand(player, cmd [iarg - 1], tab);
            }

            if (CommandTabs.ContainsKey(cmd [iarg - 1]))
            {
                //Debug.WriteLine("TC: " + cmd.JoinFrom(0));
                CommandTabs [cmd [iarg - 1]](player, cmd, iarg, tab);
            }
        }

        public virtual bool ParseCommand(Client player, string[] cmd, int iarg)
        {
            string command = cmd [iarg - 1];
            if (player.Admin(Permissions.AnyAdmin))
            {
                if (AdminCommandParse.ContainsKey(command))
                {
                    AdminCommandParse [command](player, cmd, iarg);
                    return true;
                }
            }

            if (CommandParse.ContainsKey(command))
            {
                CommandParse [command](player, cmd, iarg);
                return true;
            }
            return false;
        }
    }
}

