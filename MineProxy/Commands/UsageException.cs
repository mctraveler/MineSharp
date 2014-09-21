using System;

namespace MineProxy.Commands
{
    public class UsageException : Exception
    {
        public UsageException(string message) : base(message)
        {
        }
    }
}

