using System;

namespace MineProxy.Worlds
{
    public class TheConstructSession : ConstructSession
    {
        public override World World { get { return World.Construct; } }

        public override string ShortWorldName { get { return "Construct"; } }

        public TheConstructSession(Client player) : base (player)
        {
            Mode = GameMode.Creative;
        }
    }
}

