using System;
using System.Net.Sockets;
using System.IO;
using System.Net;
using MiscUtil.IO;
using MiscUtil.Conversion;
using MineProxy.Plugins;
using MineProxy.Worlds;
using MineProxy.Misc;

namespace MineProxy
{
    public partial class Client
    {
        /// <summary>
        /// Per player saved settings
        /// </summary>
        public ClientSettings Settings { get; set; }
        
        /// <summary>
        /// Only for offline players
        /// </summary>
        protected Client()
        {
            this.Settings = new ClientSettings();
        }

        /// <summary>
        /// Custom client base
        /// </summary>
        protected Client(Socket socket, Stream client) : this()
        {
            this.socket = socket;
            RemoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
            //LocalPort = (socket.LocalEndPoint as IPEndPoint).Port;
            Country = GeoIP.Lookup(RemoteEndPoint.Address);

            clientThread = Threads.Create(this, ReceiverRunClientWrapper, WatchdogKilled);

            Phase = Phases.Handshake;
            SetWorld(World.Void);

            this.clientStream = client;
        }
    }
}

