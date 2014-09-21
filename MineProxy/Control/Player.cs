using System;
using MineProxy;
using MineProxy.Worlds;

namespace MineProxy.Control
{
    public partial class Player
    {
        public string Username { get; set; }
        
        public MineProxy.CoordDouble Position { get; set; }
        
        public int Dimension { get; set; }
        
        public string Session { get; set; }
        
        public int AttachedTo { get; set; }
        
        public MineProxy.Control.ChatEntry Chat { get; set; }
        
        public TimeSpan Uptime { get; set; }
        
        public DateTime BannedUntil { get; set; }
        
        public Player()
        {
			
        }
		
        internal Player(Client p)
        {
            Username = p.MinecraftUsername;
            Uptime = p.Uptime;
            if (p.Session.Position != null)
            {
                Position = p.Session.Position;
            }
            Dimension = (int)p.Session.Dimension;
			
            Session = p.Session.GetType().Name;

            AttachedTo = p.Session.AttachedEntity;
            if (p.ChatEntry != null)
            {
                Chat = new ChatEntry();
                Chat.Channel = p.ChatEntry.Channel;
                Chat.Message = p.ChatEntry.Message;
                Chat.Timestamp = p.ChatEntry.TimeStamp;
            }
        }
    }
}

