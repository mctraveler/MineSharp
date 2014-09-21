using System;

namespace MineProxy.Control
{
	public partial class ChatEntry
	{
        public string Message { get; set; }
        
        public string Channel { get; set; }
        
        public DateTime Timestamp { get; set; }
        
		public override string ToString ()
		{
			if (Channel == null)
				return Message;
			else
				return "[" + Channel + "] " + Message;
		}
	}
}

