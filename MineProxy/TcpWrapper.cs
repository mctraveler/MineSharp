using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using MineProxy;

namespace MineSharp
{
    public abstract class TcpWrapper
    {
        public TcpWrapper()
        {
        }

        TextWriter writer;

        protected void SendCommand(string command)
        {
            if (writer == null)
            {
                return;
            }
            
            lock (writer)
            {
                Debug.WriteLine(">>> " + command);
                writer.WriteLine(command);
                writer.Flush();
            }
        }

        protected abstract void LineReceived(string line);

        TcpClient client;
        protected const int commandPort = 25765;
        bool active = true;
        
        /// <summary>
        /// Connect and run.
        /// </summary>
        public void Run()
        {
            Console.WriteLine("Connecting to MineWrapper on localhost:" + commandPort + "...");

            while (active)
            {
                try
                {
                    client = new TcpClient();
                    client.Connect("localhost", commandPort);

                    Console.WriteLine("Connected to MineWrapper");

                    NetworkStream ns = client.GetStream();
                    writer = new StreamWriter(ns, Encoding.UTF8);
                    TextReader reader = new StreamReader(ns, Encoding.UTF8);
                
                    Reconnecting();
                
                    while (active)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                            throw new EndOfStreamException();
                        LineReceived(line);
                    }
#pragma warning disable 168
                } catch (SocketException se)
                {
                    Console.WriteLine("Wrapper Connection: " + se.Message);
                    try
                    {
                        Thread.Sleep(1000);
                    } catch (ThreadInterruptedException)
                    {
                        return;
                    }
                } catch (EndOfStreamException eos)
                {
#pragma warning restore 168
                    Console.WriteLine("Wrapper Connection: " + eos.Message);
                    try
                    {
                        Thread.Sleep(1000);
                    } catch (ThreadInterruptedException)
                    {
                        return;
                    }
                } catch (Exception e)
                {
                    Log.WriteServer(e);
                    try
                    {
                        Thread.Sleep(1000);
                    } catch (ThreadInterruptedException)
                    {
                        return;
                    }
                } finally
                {
                    client.Close();
                }
            }
        }
        
        public void Stop()
        {
            try
            {
                active = false;
                client.Close();
            } catch (Exception)
            {
            }
        }
        
        protected abstract void Reconnecting();
    }
}

