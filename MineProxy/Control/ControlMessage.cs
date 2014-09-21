using System;

namespace MineProxy.Control
{
    public partial class ControlMessage
    {
        public bool PlayerUpdate { get; set; }
        
        public Kick Kick { get; set; }
        
        public Ban Ban { get; set; }
        
        public Pardon Pardon { get; set; }
        
        public TpPlayer TP { get; set; }
    }
}

