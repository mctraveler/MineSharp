using System;

namespace MineProxy
{
    /// <summary>
    /// When an unexpected protocol format/behaviour was encountered
    /// </summary>
    public class ProtocolException : Exception
    {
        public ProtocolException(string message) : base(message)
        {
        }
    }
}

