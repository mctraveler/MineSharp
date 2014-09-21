using System;
using System.Collections.Generic;

namespace MineProxy.Control
{
    public partial class PlayersUpdate
    {
        public long MessageID { get; set; }

        public List<MineProxy.Control.Player> List { get; set; }
    }
}

