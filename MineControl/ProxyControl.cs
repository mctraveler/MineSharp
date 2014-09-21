using System;
using System.Collections.Generic;
using MineProxy.Control;
using System.Net.Sockets;
using System.Net;
using MineProxy;
using System.Threading;
using System.IO;
using MineProxy.Data;

namespace MineControl
{
    public static class ProxyControl
    {
        private static int port;

        static ProxyControl()
        {
            Players = new PlayersUpdate();
            Players.List = new List<MineProxy.Control.Player>();
        }

        public static void Connect(int port)
        {
            ProxyControl.port = port;
            Thread t = new Thread(Run);
            t.Start();
        }

        static bool active = true;

        public static void Stop()
        {
            active = false;
            if (writer != null)
                writer.Dispose();
        }

        static BinaryWriter writer;

        public static PlayersUpdate Players { get; private set; }

        public static event Action Update;

        public static bool Connected  { get; private set; }

        static void Run()
        {
            DateTime nextUpdate = DateTime.Now;
			
            while (active)
            {
                try
                {
                    TcpClient client = new TcpClient();
                    client.Connect(IPAddress.Loopback, port);
                    var stream = client.GetStream();
                    var reader = new BinaryReader(stream);
                    writer = new BinaryWriter(stream);
                    
                    Console.WriteLine("Listening on proxy");
					
                    Connected = true;
					
                    while (true)
                    {
                        TimeSpan wait = nextUpdate - DateTime.Now;
                        if (wait.Ticks > 0)
                            Thread.Sleep(wait);
                        nextUpdate = DateTime.Now.Add(PlayerHistory.StepSize);
						
                        //Console.WriteLine ("Sending");
                        ControlMessage cm = new ControlMessage();
                        cm.PlayerUpdate = true;
                        Send(cm);
						
                        int length = reader.ReadInt32();
                        byte[] data = reader.ReadBytes(length);
                        PlayersUpdate pu = Json.Deserialize<PlayersUpdate>(data);
                        if (pu.List == null)
                            continue;
                        Players = pu;
                        //Console.WriteLine ("Got reply");
						
                        foreach (var p in Players.List)
                            PlayerHistory.Update(p);
						
                        if (Update != null)
                            Update();
                    }
                } catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                    Thread.Sleep(1000);
                } catch (IOException ioe)
                {
                    Console.WriteLine(ioe.Message);
                    Thread.Sleep(1000);
                } catch (ObjectDisposedException ode)
                {
                    Console.WriteLine(ode.Message);
                    Thread.Sleep(1000);
                } finally
                {
                    Connected = false;
                    if (writer != null)
                        writer.Dispose();
                    Players = new PlayersUpdate();
                    Players.List = new List<MineProxy.Control.Player>();
                    if (Update != null)
                        Update();
                }
            }
        }

        public static void Ban(MineProxy.Control.Player player, DateTime until, string reason)
        {
            Ban(player.Username, until, reason);
        }

        public static void Ban(string player, DateTime until, string reason)
        {
            ControlMessage cm = new ControlMessage();
            cm.Ban = new Ban();
            cm.Ban.BannedUntil = until;
            cm.Ban.Username = player;
            cm.Ban.Reason = reason;
            Send(cm);
        }

        public static void Pardon(string username)
        {
            ControlMessage cm = new ControlMessage();
            cm.Pardon = new Pardon();
            cm.Pardon.Username = username;
            Send(cm);
        }

        public static void Kick(MineProxy.Control.Player player, string reason)
        {
            ControlMessage cm = new ControlMessage();
            cm.Kick = new Kick();
            cm.Kick.Username = player.Username;
            cm.Kick.Reason = reason;
            Send(cm);
        }

        public static void Tp(string player, CoordDouble position, Dimensions dimension)
        {
            ControlMessage cm = new ControlMessage();
            cm.TP = new TpPlayer();
            cm.TP.Username = player;
            cm.TP.Position = position;
            cm.TP.Dimension = (int)dimension;
            Send(cm);
        }

        static void Send(ControlMessage cm)
        {
            byte[] data = Json.Serialize(cm);
            try
            {
                lock (writer)
                {
                    writer.Write((int)data.Length);
                    writer.Write(data);
                    writer.Flush();
                }
            } catch (ObjectDisposedException)
            {
            }
        }
    }
}

