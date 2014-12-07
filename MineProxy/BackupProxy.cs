using System;
using System.Threading;
using System.IO;
using System.Diagnostics;
using MineProxy.Chatting;
using MineProxy.Worlds;

namespace MineProxy
{
    public static class BackupProxy
    {
        static Threads thread;

        public static void Start()
        {
            thread = Threads.Create("Backup", BackupThreadRun, null);
            thread.Start();
        }

        /// <summary>
        /// True to prevent automatic restarts
        /// </summary>
        static int vanillaCountdown = -1;
        static int proxyCountdown = -1;

        public static DateTime LastVanillaRestart = DateTime.MinValue;
        public static DateTime LastProxyRestart = DateTime.Now;

        static void BackupThreadRun()
        {
#if DEBUG
            //DateTime nextBackendRestart = DateTime.Now.AddSeconds(10);
            DateTime nextBackendRestart = DateTime.Now.AddHours(6);
#else
            DateTime nextBackendRestart = DateTime.Now.AddHours(12);
#endif
            string reason = "";

            try
            {
                while (true)
                {
#if DEBUG
                    //Thread.Sleep (new TimeSpan (0, 0, 5));
                    Thread.Sleep(new TimeSpan(0, 1, 0));
#else
                    Thread.Sleep(new TimeSpan(0, 1, 0));
#endif
                    //Automatic restart
                    var mu = Lag.ServerMemoryUsage();
                    if (mu == null)
                    {
                        //Error in proxy, restart it
                        DateTime next = LastProxyRestart.AddMinutes(20);
                        if (next < DateTime.Now)
                        {
                            //nextBackendRestart = DateTime.Now.AddHours(6);
                            LastProxyRestart = DateTime.Now;
                            proxyCountdown = 1;
                            reason = "Restarting Proxy";
                        } else if (proxyCountdown < 0)
                        {
                            Chatting.Parser.TellAdmin("Failed to read Mem status, not until, " + next.ToShortTimeString());
                            Chatting.Parser.TellAdmin("use /proxystop to restart now");
                        }
                    } else if (mu.Quota > 0.88)
                    {
                        DateTime next = LastVanillaRestart.AddMinutes(15);
                        if (next < DateTime.Now)
                        {
                            //nextBackendRestart = DateTime.Now.AddHours(6);
                            LastVanillaRestart = DateTime.Now;
                            vanillaCountdown = 1;
                            reason = "High Memory Usage, Restart";
                        } else if (vanillaCountdown < 0)
                        {
                            Chatting.Parser.TellAdmin("High Memory Usage, not until, " + next.ToShortTimeString());
                            Chatting.Parser.TellAdmin("use /vanillastop to restart now");
                        }
                    }
                   
                    //Backend restart
                    if (nextBackendRestart < DateTime.Now)
                    {
                        nextBackendRestart = DateTime.Now.AddHours(6);
                        LastVanillaRestart = DateTime.Now;
                        vanillaCountdown = 3;
                        reason = "Scheduled restart";
                    }

                    if (vanillaCountdown >= 0)
                    {
                        if (vanillaCountdown == 1)
                            World.Main.Say(Chat.Purple + "[SERVER] ", reason + " in " + vanillaCountdown + " minute...");
                        if (vanillaCountdown > 1)
                            World.Main.Say(Chat.Purple + "[SERVER] ", reason + " in " + vanillaCountdown + " minutes...");
                        if (vanillaCountdown <= 0)
                        {
                            World.Main.Say(Chat.Purple + "[SERVER] ", reason + " in 3");
                            Thread.Sleep(1000);
                            World.Main.Say(Chat.Purple + "[SERVER] ", reason + " in 2");
                            Thread.Sleep(1000);
                            World.Main.Say(Chat.Purple + "[SERVER] ", reason + " in 1");
                            Thread.Sleep(1000);
                            Log.WriteServer("Automatic Restart of Java, High memory usage");
                            World.Main.Say(Chat.Purple + "[SERVER] ", reason + " NOW!");
                            World.Main.Send("stop");
                        }
                        vanillaCountdown -= 1;
                    }
                    
                    if (proxyCountdown >= 0)
                    {
                        if (proxyCountdown == 1)
                            World.Main.Say(Chat.Purple + "[SERVER] ", reason + " in " + proxyCountdown + " minute...");
                        if (proxyCountdown > 1)
                            World.Main.Say(Chat.Purple + "[SERVER] ", reason + " in " + proxyCountdown + " minutes...");
                        if (proxyCountdown <= 0)
                        {
                            World.Main.Say(Chat.Purple + "[SERVER] ", reason + " in 3");
                            Thread.Sleep(1000);
                            World.Main.Say(Chat.Purple + "[SERVER] ", reason + " in 2");
                            Thread.Sleep(1000);
                            World.Main.Say(Chat.Purple + "[SERVER] ", reason + " in 1");
                            Thread.Sleep(1000);
                            Log.WriteServer("Automatic Restart of Proxy, Unknown memory usage");
                            World.Main.Say(Chat.Purple + "[SERVER] ", reason + " NOW!");
                            World.Main.Send("stop");
                            Program.Shutdown(reason);
                            return;
                        }
                        proxyCountdown -= 1;
                    }
                    

                    //Proxy flush/save
                    Log.Flush();
                    foreach (Client p in PlayerList.List)
                    {
                        try
                        {
                            p.SaveProxyPlayer();
                        } catch (Exception e)
                        {
                            Log.Write(e, p);
                        }
                    }
                }
            } catch (Exception e)
            {
                Log.WriteServer(e);
            }
        }
    }
}

