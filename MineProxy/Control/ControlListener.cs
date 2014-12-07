using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace MineProxy
{
    public static class ControlListener
    {
        static Threads thread = null;
        public static int port = 25465;

        static List<ControllerThread> controllerThreads = new List<ControllerThread>();

        public static void Start(int controlPort)
        {
            if (thread != null)
                throw new InvalidOperationException("ContorlListener already started");
            port = controlPort;

            thread = Threads.Create("Controller Listener", Run, null);
            thread.Start();
        }
		
        public static void Stop()
        {
            if (Program.Active)
                Log.WriteServer("Expected MainClass.Active to be false");

            Console.Write("ControlListener stopping...");
            try
            {
                using (TcpClient tc = new TcpClient())
                    tc.Connect(IPAddress.Loopback, port);
            } catch
            {
            }
            //Close all controller threads
            foreach (var c in controllerThreads)
            {
                c.Dispose();
            }
            Console.WriteLine("done");
        }
		
        private static void Run()
        {
            while (Program.Active)
            {
                TcpListener listener = new TcpListener(IPAddress.Loopback, port);
                try
                {
                    listener.Start();
					
                    while (Program.Active)
                    {
                        Console.WriteLine("Listening for controller on " + port);
					
                        TcpClient client = listener.AcceptTcpClient();
                        if (Program.Active == false)
                            return;
                        Console.WriteLine("New controller connected");
					
                        ControllerThread ct = new ControllerThread(client);
                        controllerThreads.Add(ct);
                        Threads.Run("Controller Client", ct.Run);
                        Console.WriteLine("Controller thread started");
                    }

                } catch (ThreadInterruptedException)
                {
                    Console.WriteLine("ControlListener stoped");
#if !DEBUG
                } catch (Exception e)
                {
                    Log.WriteServer(e);
                    Thread.Sleep(5000);
#endif
                } finally
                {
                    try
                    {
                        listener.Stop();
                    } catch (Exception e)
                    {
                        Log.WriteServer(e);
                    }
                }
            }
        }
    }
}

