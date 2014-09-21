using System;

namespace MineProxy.Control
{
    public partial class TpPlayer
    {
        public string Username { get; set; }

        public MineProxy.CoordDouble Position { get; set; }

        public int Dimension { get; set; }

        public string ToUsername { get; set; }
    }
}

