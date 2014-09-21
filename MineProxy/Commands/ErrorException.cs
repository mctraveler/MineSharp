using System;

namespace MineProxy.Commands
{
    /// <summary>
    /// Command failed message
    /// </summary>
    public class ErrorException : Exception
    {
        public ErrorException(string message) : base(message)
        {
        }
    }
}

