using System;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace MineProxy.Query
{
    public static class QueryListener
    {
        static UdpClient listener = new UdpClient(new IPEndPoint(IPAddress.Any, 25565));
        static readonly byte[] handshakeReply = new byte[7];
        static byte[] basicStatReply;
        static byte[] fullStatReply;

        public static void Start()
        {
            handshakeReply [0] = 9; //Handshake
            //1-4 session id set for every request
            handshakeReply [5] = (byte)'1'; //Challenge token
            handshakeReply [6] = 0; //null termination

            UpdateStat("Loading...", new List<string>());

            listener.BeginReceive(Received, null);
        }

        public static void UpdateStat(string motd, List<string> players)
        {
            SetBasicStat(motd, players.Count);
            SetFullStat(motd, players);
        }

        static void Received(IAsyncResult ar)
        {
            try
            {
                IPEndPoint ipe = null;
                byte[] data = listener.EndReceive(ar, ref ipe);

                if (data.Length < 3)
                    return; //Too short
                if (data [0] != 0xFE || data [1] != 0xFD)
                    return; //invalid magic

                Debug.WriteLine("Query: " + ipe + " " + BitConverter.ToString(data));
                switch (data [2])
                {
                    case 9:
                        Handshake(data, ipe);
                        break;
                    case 0:
                        if (data.Length == 11)
                            BasicStat(data, ipe);
                        else
                            FullStat(data, ipe);
                        break;
                    default:
                        Debug.WriteLine("Query Unhandled");
                        Log.WriteServer("Query Unhandled: " + ipe + ", " + BitConverter.ToString(data));
                        break;
                }

            } catch (Exception e)
            {
                Log.WriteServer(e);
                Thread.Sleep(500);
            } finally
            {
                listener.BeginReceive(Received, null);
            }
        }


        //FE-FD-09-00-00-00-00-00-00-00-00
        //
        static void Handshake(byte[] d, IPEndPoint ipe)
        {
            handshakeReply [1] = d [3]; //Session ID
            handshakeReply [2] = d [4]; //Session ID
            handshakeReply [3] = d [5]; //Session ID
            handshakeReply [4] = d [6]; //Session ID

            listener.Send(handshakeReply, handshakeReply.Length, ipe);

            Debug.WriteLine("Handshake reply sent to " + ipe);
        }

        static void WriteString(MemoryStream ms, string value)
        {
            byte[] s = Encoding.ASCII.GetBytes(value + "\0");
            ms.Write(s, 0, s.Length);
        }

        //FE-FD-00-00-00-00-00-00-00-00-01
        static void BasicStat(byte[] d, IPEndPoint ipe)
        {
            var r = basicStatReply;
            r [1] = d [3];
            r [2] = d [4];
            r [3] = d [5];
            r [4] = d [6];
            listener.Send(r, r.Length, ipe);
            Debug.WriteLine("BasicStat reply sent to " + ipe);
        }
        
        public static void SetBasicStat(string motd, int players)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(0); //Type: 0 = BasicStat
            ms.WriteByte(0); //Session ID, not set here
            ms.WriteByte(0);
            ms.WriteByte(0);
            ms.WriteByte(0);
            WriteString(ms, motd);
            WriteString(ms, "SMP");
            WriteString(ms, "world");
            WriteString(ms, players.ToString());
            WriteString(ms, MinecraftServer.MaxSlots.ToString());
            //Host
            var ip = (IPEndPoint)listener.Client.LocalEndPoint;
            ms.WriteByte((byte)(ip.Port & 0xFF));
            ms.WriteByte((byte)(ip.Port >> 8)); //Port
            WriteString(ms, MinecraftServer.PublicIP);

            basicStatReply = ms.ToArray();
        }

        //FE-FD-00-00-00-00-00-00-00-00-01
        static void FullStat(byte[] d, IPEndPoint ipe)
        {
            var r = fullStatReply;
            r [1] = d [3];
            r [2] = d [4];
            r [3] = d [5];
            r [4] = d [6];
            listener.Send(r, r.Length, ipe);
            Debug.WriteLine("FullStat reply sent to " + ipe);
        }
        
        public static void SetFullStat(string motd, List<string> players)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(0); //Type: 0 = BasicStat
            ms.WriteByte(0); //Session ID, not set here
            ms.WriteByte(0);
            ms.WriteByte(0);
            ms.WriteByte(0);

            WriteString(ms, "splitnum");
            //Key-Value Start
            ms.WriteByte(0x80);
            ms.WriteByte(0);

            WriteString(ms, "hostname");
            WriteString(ms, motd);

            WriteString(ms, "gametype");
            WriteString(ms, "SMP");

            WriteString(ms, "game_id");
            WriteString(ms, "MINECRAFT");

            WriteString(ms, "version");
            WriteString(ms, MinecraftServer.FrontendVersion.ToText());

            WriteString(ms, "plugins");
            WriteString(ms, "");

            WriteString(ms, "map");
            WriteString(ms, "world");
            
            WriteString(ms, "numplayers");
            WriteString(ms, players.Count.ToString());
            
            WriteString(ms, "maxplayers");
            WriteString(ms, MinecraftServer.MaxSlots.ToString());
            
            WriteString(ms, "hostport");
            WriteString(ms, "25565");
            
            WriteString(ms, "hostip");
            WriteString(ms, MinecraftServer.PublicIP);

            //Key-Value End
            ms.WriteByte(0);
            ms.WriteByte(1);

            //Player list
            WriteString(ms, "player_");
            ms.WriteByte(0); //Padding???
            foreach (string p in players)
                WriteString(ms, p);
            ms.WriteByte(0); //Padding???

            fullStatReply = ms.ToArray();
        }
    }
}

