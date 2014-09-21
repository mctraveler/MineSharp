using System;

namespace MineProxy.Chatting
{
    public class Entry
    {
        public string Message { get; private set; }
        
        public string Channel { get; private set; }
        
        public DateTime TimeStamp { get; private set; }
        
        public Entry(string channel, string message)
        {
            this.Channel = channel;
            this.Message = message;
            this.TimeStamp = DateTime.Now;
        }
        
        public override string ToString()
        {
            if (Channel == null)
                return Message;
            else
                return "[" + Channel + "] " + Message;
        }
    }
}

