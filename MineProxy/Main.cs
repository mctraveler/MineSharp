using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Bot;
using MineProxy.Query;
using MineProxy.Chatting;
using MineProxy.Worlds;
using MineProxy.Clients;

namespace MineProxy
{
    class MainClass
    {
        static TcpListener listener = null;

        public static void Main(string[] args)
        {
            //Bug workaround
            System.Web.Util.HttpEncoder.Current = System.Web.Util.HttpEncoder.Default;

            Directory.SetCurrentDirectory(MineSharp.Settings.BaseWorldsPath);
            Console.WriteLine();
            Console.WriteLine("Starting Mineproxy: " + DateTime.Now);
            Log.Init();

            //Init commands
            Commands.MainCommands.Init();

            System.Threading.Thread.CurrentThread.Name = "Player Listener";			
			
            Console.CancelKeyPress += HandleConsoleCancelKeyPress;
			
            int controllerPort = 25465;

#if DEBUG
            //Only for debugging the live server
            //MinecraftServer.Port = 25665;

            //Debug.WriteLine(MinecraftProtocol.Minecraft.Protocol);
#endif
			
			
            //Load settings
            Banned.LoadBanned();
            MineProxy.Regions.WarpPortalVisuals.Init();
			
            //VoteListener.Start();
            ServerCommander.Startup();
            ControlListener.Start(controllerPort);
            BackupProxy.Start();

            try
            {
                PlayerList.StartUpdater();
                //SpawnRegion.Start ();
                SettingsLoader.Start();
                QueryListener.Start();
            } catch (Exception e)
            {
                Log.WriteServer(e);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            while (true)
            {
                Console.WriteLine(DateTime.Now + " Starting Main true loop");
                listener = new TcpListener(IPAddress.Any, MinecraftServer.MainPort);
                try
                {
                    listener.Start();
					
                    Console.WriteLine("Listening for players on " + MinecraftServer.MainPort);
                    
                    while (Active)
                    {
						
                        TcpClient client = listener.AcceptTcpClient();
                        if (Active == false)
                            break;
					
                        //Console.WriteLine ("Got incoming");
                        try
                        {
                            //check banned ip
                            if (Banned.IsBanned(((IPEndPoint)client.Client.RemoteEndPoint).Address))
                            {
                                try
                                {
                                    client.Close();
                                } catch (Exception e)
                                {
                                    Log.WriteServer("Error closing banned ip", e);
                                }
                                continue;
                            }
                            
                            Client proxy = new VanillaClient(client.Client);
                            proxy.Start();
                        } catch (SocketException)
                        {
                            try
                            {
                                client.Close();
                            } catch
                            {
                            }
                        }
                    }
					
                    //Program is exiting here
                    Console.WriteLine("Kicking all players: " + ShutdownMessage);
                    foreach (Client p in PlayerList.List)
                    {
                        p.Kick(ShutdownMessage);
                    }

                    Log.Flush();
                    ServerCommander.Shutdown();
                    ControlListener.Stop();
                    return;
			
#if !DEBUG	
                } catch (Exception e)
                {
                    Console.WriteLine(DateTime.Now + " Main loop error: " + e.GetType().Name + " " + e.Message);
                    Log.WriteServer("MainClass.Main general", e);
                    System.Threading.Thread.Sleep(500);
#endif
                } finally
                {
                    Console.WriteLine(DateTime.Now + " Main loop finally Start");
                    try
                    {
                        //Save region stats
                        RegionLoader.Save(World.Main.Regions);
                        Regions.WarpPortalVisuals.Stop();
                    } catch (Exception e)
                    {
                        Log.WriteServer("Main closing region stats saving", e);
                    }
                    Console.WriteLine(DateTime.Now + " Main loop finally End");
                    Environment.Exit(0);
                }
            }
        }

        static void HandleConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (Active)
            {
                Console.Error.WriteLine("Aborted from console.");
                Shutdown("Please reconnect");
                e.Cancel = true;
            }
        }

        public static bool Active = true;
        static string ShutdownMessage = "Bye(generic restart)";

        public static void Shutdown(string message)
        {
            ShutdownMessage = Chat.Purple + "Restarting... " + Chat.Gold + message;
            Active = false;
            using (TcpClient t = new TcpClient())
                t.Connect(IPAddress.Loopback, MinecraftServer.MainPort);
			
        }
    }
}
