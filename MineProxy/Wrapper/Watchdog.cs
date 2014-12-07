using System;

namespace MineSharp.Wrapper
{
    public static class Watchdog
    {
        static readonly TimeSpan Timeout = TimeSpan.FromSeconds(20);

        public static void Test()
        {
            foreach (Server s in BackendManager.GetServers())
                Test(s);
        }

        public static void Test(Server s)
        {
            DateTime start = DateTime.Now;
            DateTime timeout = DateTime.Now.Add(Timeout);
            s.SendCommand("whitelist");
            while (true)
            {
                if (s.LastReceived > start)
                    break;
#if DEBUG
                Console.WriteLine("Watchdog waiting");
#endif
                if (DateTime.Now < timeout)
                {
                    System.Threading.Thread.Sleep(100);
                    continue;
                }
                //Triggered
                BackendManager.Log(new Exception("Watchdog: " + s.Name));
                s.Kill();
                break;
            }
        }
    }
}

