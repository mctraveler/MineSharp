using System;

namespace MineProxy.Worlds
{
    public class GreenSession : ConstructSession
    {
        public override World World { get { return World.Wait; } }
		
        public override string ShortWorldName { get { return "Waiting"; } }

        public GreenSession(Client player) : base (player)
        {
        }
    }
}

