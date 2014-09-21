using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using MineProxy.NBT;
using MineProxy.Network;
using MineProxy.Data;

namespace MineProxy
{
    public static class MinecraftServer
    {
        public static string PublicIP = "91.123.200.164";

        public static readonly IPAddress IP = IPAddress.Loopback;

        public const int MainPort = 25565;

        public static int BackendPort { get; set; }
        
        /// <summary>
        /// Max in the real world
        /// </summary>
        public static int MaxSlots = 20;

        public static string AdminEmail { get; set; }
        
        public static string PingReplyMessage { get; set; }

        public static string TexturePack { get; set; }

        public static Dictionary<string,Permissions> Admins = new Dictionary<string, Permissions>();

        static MinecraftServer()
        {
            RsaBytes = CryptoMC.Init();
            PingReplyMessage = "Loading...";
        }

        /// <summary>
        /// DER encoded RSA public key
        /// </summary>
        public static readonly byte[] RsaBytes;

        /// <summary>
        /// Number of players to present to the playerlist
        /// </summary>
        public const int PlayerListSlots = 60; //40=2 columns, 60=3 columns, 80= 4 columns

        public const ProtocolVersion BackendVersion = ProtocolVersion.v1_8;
        public const ProtocolVersion FrontendVersion = ProtocolVersion.v1_8;
    }
}

