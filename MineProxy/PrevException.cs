using System;
using MineProxy.Packets;

namespace MineProxy
{
    /// <summary>
    /// Previous package leading up to an exception
    /// </summary>
    public class PrevException : Exception
    {
        public readonly Packet Packet;

        public PrevException(Packet p) : base(p == null?"No Error":p.ToString())
        {
            Packet = p;
        }
    }
}

