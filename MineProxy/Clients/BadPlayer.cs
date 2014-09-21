using System;
using System.Collections.Generic;

namespace MineProxy
{
    public partial class BadPlayer
    {
        public string Username { get; set; }

        public DateTime BannedUntil { get; set; }

        public string Reason { get; set; }
        
        public Dictionary<string, Client> UnbanVote = new Dictionary<string, Client>();
		
        public override string ToString()
        {
            string message = "Banned ";
            TimeSpan left = (BannedUntil - DateTime.Now);
            if (left.TotalSeconds < 0)
                message += "(expired)";
            else if (left.TotalSeconds < 60)
                message += left.TotalSeconds.ToString("0") + " seconds";
            else if (left.TotalMinutes < 60)
                message += left.TotalMinutes.ToString("0") + " minutes";
            else if (left.TotalHours < 24)
                message += left.TotalHours.ToString("0") + " hours";
            else if (left.TotalDays < 360)
                message += left.TotalDays.ToString("0") + " days";
            else
                message += "forever";
            message += ": " + Reason;
            return message;
        }
    }
}

