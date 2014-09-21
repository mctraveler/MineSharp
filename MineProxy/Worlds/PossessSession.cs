using System;
using MineProxy.Chatting;
using MineProxy.Packets;

namespace MineProxy.Worlds
{
    public class PossessSession : WorldSession
    {
        public override string ShortWorldName { get { return "Vanilla"; } }

        Client victim;

        public PossessSession(Client player, Client victim) : base(player)
        {
            player.Queue.Queue(ChangeGameState.ChangeGameMode(GameMode.Creative));
            var et = new PlayerPositionLookServer(victim.Session.Position);
            et.Yaw = victim.Session.Yaw;
            et.Pitch = victim.Session.Pitch;
            player.Queue.Queue(et);

            this.victim = victim;
            victim.Possess(this);
        }

        public override void Close(string message)
        {
            Player.TellAbove(Chat.Yellow, "EXORCISED!!!");
            victim.Exorcise(this);
        }

        public override void FromClient(PacketFromClient packet)
        {
        }

        public override World World
        {
            get
            {
                //We say real here so all chat works as normal
                return World.Main;
            }
        }
    }
}

