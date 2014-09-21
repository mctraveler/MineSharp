using System;
using System.Collections.Generic;

namespace MineProxy.Worlds
{
    public class PossessWorld : World
    {
        public static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

        public override WorldSession Join(Client player)
        {
            throw new NotImplementedException();
        }

        public PossessSession Join(Client player, Client victim)
        {
            PossessSession session = new PossessSession(player, victim);
            return session;
        }
    }
}

