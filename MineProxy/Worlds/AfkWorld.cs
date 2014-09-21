using System;

namespace MineProxy.Worlds
{
    public class AfkWorld : World
    {
        public static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

        public override WorldSession Join(Client player)
        {
            AfkSession session = new AfkSession(player);
            return session;
        }
    }
}

