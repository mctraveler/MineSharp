using System;
using System.Threading;
using Bot;
using MineProxy.Chatting;
using MineProxy.Worlds;

namespace MineProxy
{
    public class ServerCommander : TcpWrapper
    {
        static ServerCommander instance;

        readonly Threads thread;

        ServerCommander()
        {
            thread = Threads.Create(this, Run, null);
        }

        public override string ToString()
        {
            return "ServerCommander: " + commandPort;
        }

        protected override void Reconnecting()
        {
            firstServers = true;
        }

        public static void Startup()
        {
            if (instance != null)
                throw new InvalidOperationException("Already started Servercommander");

            instance = new ServerCommander();
            instance.thread.Start();
        }

        public static void Shutdown()
        {
            try
            {
                if (instance == null)
                    return;

                instance.Stop();
            } catch (Exception)
            {
            }
        }

        bool firstServers = true;

        protected override void LineReceived(string line)
        {
            string[] parts = line.Split('\t');

            if (parts [0] == "servers")
            {
                foreach (var kvp in World.VanillaWorlds)
                {
                    if (line.Contains("\t" + kvp.Value.ServerName) == false)
                    {
                        kvp.Value.Stopped.Set();
                        kvp.Value.Running.Reset();
                    } else
                    {
                        if (firstServers)
                        {
                            kvp.Value.Stopped.Reset();
                            kvp.Value.Running.Set();
                        }
                    }
                }
                firstServers = false;
                return;
            }

            if (line.Contains("INFO]: Done"))
            {
                VanillaWorld w;
                if (World.VanillaWorlds.ContainsKey(parts [0]))
                    w = World.VanillaWorlds [parts [0]];
                else
                {
                    var p = PlayerList.GetPlayerByUsername("nuxas");
                    if (p != null)
                        p.TellSystem(Chat.Red, "Unkown world: " + line);
                    return;
                }

                w.Running.Set();
                w.Stopped.Reset();

                if (parts [0] == World.Main.ServerName)
                {
                    PlayerList.SetRealWorldForAllInConstruct();
                }
            }
        }

        public static void Send(VanillaWorld world, string command)
        {
            instance.SendCommand(world.ServerName + "\t" + command);
        }

        public static void SendWrapperCommand(string cmd)
        {
            instance.SendCommand(cmd);
        }
    }
}

