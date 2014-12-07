using System;
using System.Threading;
using System.Net.Sockets;
using MineProxy.Control;
using System.IO;
using System.Collections.Generic;
using MineProxy.Commands;
using Newtonsoft.Json;
using System.Text;

namespace MineProxy
{
    public class ControllerThread : IDisposable
    {
        readonly NetworkStream stream;
        readonly TcpClient client;

        public ControllerThread(TcpClient client)
        {
            stream = client.GetStream();
            this.client = client;
        }

        public void Dispose()
        {
            try
            {
                client.Close();
            } catch (ObjectDisposedException)
            {
            }
        }

        int messageID = 2;

        public void Run()
        {
            try
            {
                Console.WriteLine("Listening for controller commands");
                RunController();
            } catch (ObjectDisposedException)
            {
                //Connection closed
            } catch (IOException)
            {
                //Connection closed
#if !DEBUG
            } catch (Exception e)
            {
                Log.WriteServer(e);
#endif
            } finally
            {
                Dispose();
            }
        }

        void RunController()
        {
            BinaryReader r = new BinaryReader(stream);
            BinaryWriter w = new BinaryWriter(stream);
        
            while (Program.Active)
            {
                int length = r.ReadInt32();
                byte[] packet = r.ReadBytes(length);
                string json = Encoding.UTF8.GetString(packet);
                ControlMessage c = JsonConvert.DeserializeObject<ControlMessage>(json);
                if (Program.Active == false)
                    return;
                
                try
                {
                    //Console.WriteLine ("Got from controller " + c);
                    if (c.Kick != null)
                    {
                        //Console.WriteLine ("Kick");
                        Client p = PlayerList.GetPlayerByUsername(c.Kick.Username);
                        if (p != null)
                        {
                            p.Kick(c.Kick.Reason);
                        }
                    }
                    
                    if (c.Ban != null)
                    {
                        Banned.Ban(null, c.Ban.Username, c.Ban.BannedUntil, c.Ban.Reason);
                    }
                    
                    if (c.Pardon != null)
                    {
                        Banned.Pardon(null, c.Pardon.Username);
                    }
                    
                    if (c.PlayerUpdate)
                    {
                        //Console.WriteLine ("Update");
                        //Send complete player status update
                        PlayersUpdate pu = new PlayersUpdate();
                        pu.List = new List<Control.Player>();
                        pu.MessageID = messageID;
                        messageID += 2;
                        foreach (Client pp in PlayerList.List)
                        {
                            pu.List.Add(new Control.Player(pp));
                        }
                        
                        byte[] buffer = Json.Serialize(pu);
                        
                        w.Write((int)buffer.Length);
                        w.Write(buffer);
                        w.Flush();
                        //Console.WriteLine ("Update Sent");
                    }
                    
                    if (c.TP != null)
                    {
                        var p = PlayerList.GetPlayerByUsernameOrName(c.TP.Username);
                        if (p == null)
                            return;
                        if (c.TP.ToUsername != null)
                        {
                            var pTo = PlayerList.GetPlayerByUsernameOrName(c.TP.ToUsername);
                            if (pTo == null)
                                return;
                            p.Session.World.Send("tp " + p.MinecraftUsername + " " + pTo.MinecraftUsername);
                            continue;
                        }
                        if (c.TP.Position != null)
                        {
                            p.Warp(c.TP.Position, (Dimensions)c.TP.Dimension, Worlds.World.Main);
                            continue;
                        }
                    }
                } catch (Exception ie)
                {
                    Log.WriteServer(ie);
                }
            }
        }
    }
}

