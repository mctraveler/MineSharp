using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;
using System.Net;

namespace MineSharp.Wrapper
{
    class InvalidArgumentException : Exception
    {
        public InvalidArgumentException(string message) : base(message)
        {
            
        }
    }


}

