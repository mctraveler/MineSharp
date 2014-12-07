using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using MineProxy;

namespace MineSharp.Wrapper
{
    /// <summary>
    /// Manages start and stop of the backend, the vanilla java servers.
    /// </summary>
    class BackendManager
    {
        const int ControlPort = 25765;

        /// <summary>
        /// Vanilla minecraft servers
        /// </summary>
        static readonly Dictionary<string,Server> servers = new Dictionary<string, Server>();
        /// <summary>
        /// Incoming MineProxy/MineControl/Telnet controllers
        /// </summary>
        static List<Client> clients = new List<Client>();

        public static void Start()
        {
            Thread tcpListener = new Thread(TcpListener);
            tcpListener.Start();

            Console.WriteLine("Control Listening on port: " + ControlPort);

            KillAllJava();

            //Start main
            try
            {
                StartServer("main");
            }
            catch (Exception ex)
            {
                Console.WriteLine("While starting main: " + ex.Message);
            }
        }

        public static void Shutdown()
        {
            lock (servers)
            {
                foreach (var s in GetServers())
                    s.Stop();
            }
            Thread.Sleep(2000);
            lock (servers)
            {
                foreach (var s in GetServers())
                    s.Kill();
            }
        }


        static void KillAllJava()
        {
            #if !DEBUG
            try
            {
                Process.Start("/usr/bin/pkill", "-kill java").WaitForExit();
            }
            catch (Exception ex)
            {
                Log(ex);
            }
            #endif
        }

        public static void SendCommandAll(string command)
        {
            foreach (var s in GetServers())
                s.SendCommand(command);
        }

        public static Server StartServer(string name)
        {
            lock (servers)
            {
                Server s = GetServer(name);
                if (s != null)
                    return s;

                if (Directory.Exists(name) == false)
                    throw new InvalidArgumentException("Missing directory: " + name);

                s = new Server(name);
                servers.Add(name, s);
                s.Start();
                SendToClients(RunningServers());
                return s;
            }
        }

        public static bool StopServer(string name)
        {
            lock (servers)
            {
                Server s = GetServer(name);
                if (s == null)
                    return false;

                s.Stop();
                servers.Remove(name);
                SendToClients(RunningServers());
                return true;
            }
        }

        public static string RunningServers()
        {
            string line = "servers";
            var servers = GetServers();
            foreach (var s in servers)
                line += "\t" + s.Name;
            return line;
        }


        public static Server GetServer(string name)
        {
            lock (servers)
            {
                if (servers.ContainsKey(name))
                    return servers[name];
                return null;
            }
        }

        /// <summary>
        /// "Threadsafe" list, not related to storage list
        /// </summary>
        public static List<Server> GetServers()
        {
            lock (servers)
            {
                List<Server> list = new List<Server>();
                foreach (var kvp in servers)
                    list.Add(kvp.Value);
                return list;
            }
        }

        static void TcpListener()
        {
            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, ControlPort));
            listener.Start();
            while (true)
            {
                TcpClient tc = listener.AcceptTcpClient();
                if (tc == null)
                    continue;
                if (Program.Exit.WaitOne(0) == true)
                    return;
                Client c = new Client(tc);
                Console.WriteLine("New client: " + c);
                lock (clients)
                    clients.Add(c);
            }
        }

        public static void ClientClosed(Client c)
        {
            lock (clients)
            {
                clients.Remove(c);
            }
        }

        public static void SendToClients(string line)
        {
            lock (clients)
            {
                #if DEBUG
                //Console.WriteLine("::: " + line);
                #endif

                foreach (Client c in clients)
                {
                    try
                    {
                        c.SendLine(line);
                    }
                    catch (Exception e)
                    {
                        Log(e);
                    }
                }
            }
        }

        public static void Log(Exception e)
        {
            Console.Error.WriteLine(e.GetType().Name);
            Console.Error.WriteLine(e.Message);
            Console.Error.WriteLine(e.StackTrace);
        }
    }
}

