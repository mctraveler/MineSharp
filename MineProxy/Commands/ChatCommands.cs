using System;
using MineProxy.Chatting;

namespace MineProxy.Commands
{
    public class ChatCommands
    {
        public ChatCommands(CommandManager c)
        {
            c.AddCommand(TellAdmin, "telladmin", "admin");
            c.AddCommand(Channel, "channel", "ch", "c");
            c.AddCommand(Shout, "shout");
            c.AddCommand(Prod, "push", "prod", "slap");
            c.AddCommand(Firehose, "firehose");
            c.AddCommand(Tell, "tell", "msg");
            c.AddCommand(Timestamp, "timestamp");
            c.AddCommand(Me, null, "me");
            c.AddCommand(Chat.ResetChannel, "reset");
        }

        static void TellAdmin(Client player, string[] cmd, int iarg)
        {
            string msg = cmd.JoinFrom(1).Trim();
            if (msg == "")
                throw new ShowHelpException();
                    
            Chatting.Parser.TellAdmin(Permissions.AnyAdmin, player.Name + ": " + msg);
            bool admin = false;
            foreach (Client a in PlayerList.List)
            {
                if (a.Admin(Permissions.AnyAdmin) && a.Settings.Cloaked == null)
                {
                    admin = true;
                    break;
                }
            }
            if (admin)
                player.TellSystem(Chat.Purple, ">Admin: " + msg);
            else
                player.TellSystem(Chat.Purple, "No admin online");
        }

        static void Me(Client player, string[] cmd, int iarg)
        {
            Parser.ParseClientChat(player, cmd.JoinFrom(1));
        }

        static void Channel(Client player, string[] cmd, int iarg)
        {
            if (cmd.Length == 2)
                Chat.SetChannel(player, cmd [1]);
            else
                Chat.TellChannel(player);
        }

        static void Shout(Client player, string[] cmd, int iarg)
        {
            Parser.ParseClientChat(player, "!" + cmd.JoinFrom(1));
        }

        static void Prod(Client player, string[] cmd, int iarg)
        {
            if (Banned.CheckBanned(player) != null)
                return;
                    
            if (cmd.Length != 2)
            {
                player.TellSystem(Chat.Purple, "Who?");
                return;
            }
            Client p = PlayerList.GetPlayerByName(cmd [1]);
            if (p == null)
            {
                player.TellSystem(Chat.Red, "Player not found");
                return;
            }
            PlayerInteraction.Prod(player, p);
        }

        static void Firehose(Client player, string[] cmd, int iarg)
        {
            player.Settings.Firehose = !player.Settings.Firehose;
            if (player.Settings.Firehose)
                player.TellSystem(Chat.Green, "Firehose is on" + Chat.Gray + " - you now hear everything");
            else
                player.TellSystem(Chat.Red, "Firehose is off" + Chat.Gray + "(default) - you hear within 500 blocks");
        }

        static void Timestamp(Client player, string[] cmd, int iarg)
        {
            player.Settings.ShowTimestamp = !player.Settings.ShowTimestamp;
            if (player.Settings.ShowTimestamp)
                player.TellSystem(Chat.Green, "Timestamp is on");
            else
                player.TellSystem(Chat.Red, "Timestamp is off");
        }

        static void Tell(Client player, string[] cmd, int iarg)
        {
            if (cmd.Length <= 2)
                throw new UsageException("/tell <who> <message>");

            Parser.SendPrivateMessage(player, cmd [1], cmd.JoinFrom(2));
        }
    }
}

