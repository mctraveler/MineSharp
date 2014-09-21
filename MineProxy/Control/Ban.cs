using System;

namespace MineProxy.Control
{
public partial class Ban
    {
        public string Username { get; set; }
        
        public string Reason { get; set; }
        
        public DateTime BannedUntil { get; set; }
        
    }
}

