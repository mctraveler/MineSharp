using System;
using MineProxy.Chatting;
using MineProxy.Packets;

namespace MineProxy.Worlds
{
    public class AfkSession : WorldSession
    {
        /// <summary>
        /// Set in client.SetWorld
        /// </summary>
        public World OriginalWorld;
        
        public override World World
        {
            get { return OriginalWorld;}
        }

        public override string ShortWorldName { get { return "AFK"; } }

        public AfkSession(Client player) : base(player)
        {
            if (player.Settings.Cloaked == null && Player.MinecraftUsername != "Player")
                Chatting.Parser.Say(Chat.Gray, player.Name + " is now afk");
        }

        public override void Close(string message)
        {
            if (Player.Settings.Cloaked == null && Player.MinecraftUsername != "Player")
                Chatting.Parser.Say(Chat.Yellow, Player.Name + " is back!");
        }

        bool first = true;
        CoordDouble pos;

        void Back()
        {
            //Fix the ghosts after afk, chanign dimensions will clear it up
            var res = new Respawn();
            res.Dimension = Dimensions.Overworld;
            if (Dimension == Dimensions.Overworld)
                res.Dimension = Dimensions.Nether;
            res.Difficulty = 3;
            res.Mode = GameMode.Adventure;
            Player.SendToClient(res);

            Player.SetWorld(OriginalWorld);
        }

        void NewPos(CoordDouble p)
        {
            if (first)
            {
                pos = p;
                first = false;
            }
            if (pos.DistanceTo(p) > 0.1)
                Back();
        }

        public override void FromClient(PacketFromClient packet)
        {
            switch (packet.PacketID)
            {
                case UseEntity.ID:
                case PlayerBlockPlacement.ID:
                case PlayerDigging.ID:
                    Back();
                    return;

                case PlayerPositionLookClient.ID:
                    var ppl = (PlayerPositionLookClient)packet;
                    NewPos(ppl.Position);
                    return;
            }
        }

    }
}

