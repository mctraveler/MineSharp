using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;
using System.Net;
using MineProxy;

namespace MineSharp.Wrapper
{
    class Client : IDisposable
    {
        readonly TcpClient tcp;
        readonly Thread thread;
        readonly TextWriter writer;
        readonly TextReader reader;

        /// <summary>
        /// Prevents clients from issue the save command when we are running a backup session
        /// </summary>
        public bool BlockSaveCommand { get; set; }

        /// <summary>
        /// Send console output to client
        /// </summary>
        bool Echo = true;

        bool active = true;

        public Client(TcpClient tcp)
        {
            this.tcp = tcp;
            NetworkStream ns = tcp.GetStream();
            writer = new StreamWriter(ns, Encoding.UTF8);
            reader = new StreamReader(ns, Encoding.UTF8);

            this.thread = new Thread(Run);
            this.thread.Start();
        }

        void Run()
        {
            try
            {
                SendLine(BackendManager.RunningServers());

                while (active)
                {
                    try
                    {
                        if (Program.Exit.WaitOne(0))
                            break;

                        string line = reader.ReadLine();
                        if (line == null)
                            return;
                        line = line.Trim();
                        if (line == "")
                            continue;
                        string[] args = line.Split(' ', '\t');

                        ParseCommand(args);
                    } catch (InvalidArgumentException ia)
                    {
                        string error = "ERROR\t" + ia.Message;
                        Console.Error.WriteLine(error);
                        SendLine(error);
                        continue;
                    } catch (Exception e)
                    {
                        BackendManager.Log(e);
                        return;
                    }
                }
            } finally
            {
                BackendManager.ClientClosed(this);
                tcp.Close();
            }
        }

        void ParseCommand(string[] args)
        {
            switch (args [0])
            {
                case "list":
                    SendLine(BackendManager.RunningServers());
                    return;

                case "shutdown":
                    Console.WriteLine("Got Shutdown command");
                    SendLine("shutting down");
                    Program.Shutdown((args.Length > 1) ? string.Join(" ", args, 1, args.Length - 1) : "Admin initated restart");
                    active = false;
                    return;

                case "exit": //Close client connection
                    SendLine("bye");
                    active = false;
                    return;

                case "stop":
                    if (args.Length != 2)
                        throw new InvalidArgumentException("Usage: stop <name>");

                    Console.WriteLine("Got Suspend command: " + args [1]);
                    if (BackendManager.StopServer(args [1]))
                        SendLine("stopped " + args [1]);
                    else
                        SendLine("not running " + args [1]);
                    return;

                case "start":
                    if (args.Length < 2)
                        throw new InvalidArgumentException("Usage: start <name>");

                    Console.WriteLine("Got Resume command");
                    Server s = BackendManager.StartServer(args [1]);
                    SendLine("running\t" + s);
                    return;
            }

            if (BlockSaveCommand && args [0].StartsWith("save"))
                return;

            //Vanilla commands
            if (args.Length < 2)
                throw new InvalidArgumentException("Usage: <serverName> command");

            Server server = BackendManager.GetServer(args [0]);
            if (server == null)
                throw new InvalidArgumentException("No such running server: " + args [0]);

            server.SendCommand(string.Join(" ", args, 1, args.Length - 1));
        }

        public void Stop()
        {
            tcp.Close();
        }

        public void SendLine(string line)
        {
            if (Echo == false)
                return;
            try
            {
                writer.WriteLine(line);
                writer.Flush();
            } catch (IOException)
            {
                return;
            } catch (Exception e)
            {
                BackendManager.Log(e);
            }
        }

        public void Dispose()
        {
            tcp.Close();
        }

        public override string ToString()
        {
            return "[" + ((IPEndPoint)this.tcp.Client.RemoteEndPoint).Port + "]";
        }
    }

}

