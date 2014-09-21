using System;

namespace MineProxy.Worlds
{
    /// <summary>
    /// When the session has ended and will no longer work
    /// </summary>
    public class SessionClosedException : Exception
    {
        public SessionClosedException() : base("Connection lost, reconnect with /back")
        {
        }
    }
}

