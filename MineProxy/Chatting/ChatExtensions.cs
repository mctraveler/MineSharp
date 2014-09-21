using System;
using MineProxy.Chatting;
using MineProxy.Worlds;
using MineProxy.Packets;

namespace MineProxy
{
    public static class ChatExtensions
    {
        public static void TellSystem(this WorldSession session, string prefix, string message)
        {
            if (session == null)
                return;
            session.Player.TellSystem(prefix, message);
        }

        public static void Say(this World world, string prefix, string message, ChatPosition pos = ChatPosition.SystemMessage)
        {
            foreach (Client p in PlayerList.List)
            {
                if (p.Session.World != world)
                    continue;

                string msg = Format.ReceiverMacros(p, message);
                var msgs = Format.Split(prefix, msg, pos);
                foreach (var m in msgs)
                    p.Queue.Queue(m);
            }
        }

        public static void TellChat(this Client player, string prefix, string message)
        {
            if (player == null)
                return;

            if (player.Settings.ShowTimestamp)
                prefix = Chat.DarkGray + DateTime.UtcNow.ToString("HH:mm") + " " + prefix;

            //macros
            message = Format.ReceiverMacros(player, message);

            foreach (string line in message.Split(new char[]{'\n'}))
                foreach (var m in Format.Split(prefix, line, ChatPosition.ChatBox))
                    player.Queue.Queue(m);
        }

        public static void TellSystem(this Client player, string prefix, string message)
        {
            if (player == null)
                return;

            if (player.Settings.ShowTimestamp)
                prefix = Chat.DarkGray + DateTime.UtcNow.ToString("HH:mm") + " " + prefix;

            //macros
            message = Format.ReceiverMacros(player, message);

            foreach (string line in message.Split(new char[]{'\n'}))
                foreach (var m in Format.Split(prefix, line, ChatPosition.SystemMessage))
                    player.Queue.Queue(m);
        }

        public static void TellAbove(this Client player, string prefix, string message)
        {
            if (player == null)
                return;

            //macros
            message = Format.ReceiverMacros(player, message);

            ChatMessageServer cm = ChatMessageServer.CreateText(prefix + message);
            cm.Position = ChatPosition.AboveActionBar;
            player.Queue.Queue(cm);
        }

    }
}

