using System;
using System.Threading;
using System.Collections.Generic;

namespace MineProxy
{
    public class Threads
    {
        #region Instance

        public readonly System.Threading.Thread thread;
		
        public string User = "";
        DateTime stateTime = DateTime.Now;
        string state = "";
		
        public string State
        {
            get
            {
                return state;
            }
            set
            {
                WatchdogTick = DateTime.Now;
                stateTime = DateTime.Now;
                state = value;
            }
        }

        public double StateTime
        {
            get
            {
                return (DateTime.Now - stateTime).TotalSeconds;
            }
        }

        public string ThreadState { get { return thread.ThreadState.ToString(); } }

        public bool Alive { get { return thread.IsAlive; } }
		
        readonly Action run;
        readonly Action watchdogKill;

        public readonly object Object;

        Threads(object o, Action run, Action killed)
        {
            if (run == null)
                throw new ArgumentNullException("run");

            this.Object = o;
            this.run = run;
            this.watchdogKill = killed;
            thread = new Thread(Run);
            thread.Name = o.ToString();
            thread.IsBackground = true;
        }
		
        void Run()
        {
            try
            {
                run();
            } finally
            {
                lock (list)
                    list.Remove(this);
            }
        }
		
        public void Start()
        {
            thread.Start();
        }
        
        public void Interrupt()
        {
            thread.Interrupt();
        }
        
        public void Join()
        {
            thread.Join();
        }
        
        #endregion

        #region Static

        static List<Threads> list = new List<Threads>();

        public static void Run(object o, Action run)
        {
            Threads t = Create(o, run, null);		
            t.Start();
        }
		
        public static Threads Create(object o, Action run, Action watchdogKill)
        {
            Threads t = new Threads(o, run, watchdogKill);
            lock (list)
                list.Add(t);
            return t;			
        }

        public static void Stop()
        {
            lock (list)
            {
                foreach (var t in list)
                {
                    try
                    {
                        t.thread.Interrupt();
                        if (t.thread.Join(500) == false)
                            t.thread.Abort();
                    } catch (Exception e)
                    {
                        Log.WriteServer("Stopping thread: " + t, e);
                    }
                }
            }
        }
	
        public static Threads[] List
        {
            get
            { 
                lock (list)
                    return list.ToArray();
            }
        }

        #endregion

        #region Watchdog

#if DEBUG
        readonly TimeSpan watchdogTimeout = TimeSpan.FromSeconds(30);
#else
        readonly TimeSpan watchdogTimeout = TimeSpan.FromSeconds(120);
#endif

        public DateTime WatchdogTick
        {
            get { return watchdogTick;}
            set { watchdogTick = value; }
        }
        DateTime watchdogTick = DateTime.Now;
         
        /// <summary>
        /// Kill all threads that fail the watchdog test
        /// </summary>
        public static void WatchdogCheck()
        {   
            foreach (Threads t in List)
            {
                if (t.watchdogKill == null)
                    continue;

                t.WatchdogTest();
            }
        }

        /// <summary>
        /// Makes sure it is running or kill it off
        /// </summary>
        void WatchdogTest()
        {
            try
            {
                if (DateTime.Now - watchdogTimeout > watchdogTick)
                {
                    string errorMessage = "WatchDog Triggered, Thread: " + this.thread.Name + ", State: " + this.State + ", User: " + this.User + ", Last tick: " + WatchdogTick;
                    Console.WriteLine(errorMessage);
                    Log.WriteServer(errorMessage);
#if !DEBUG
                    if (watchdogKill != null)
                    {
                        try
                        {
                            watchdogKill();
                        } catch (Exception ex)
                        {
                            Log.WriteServer("While using watchdogkill code for " + this, ex);
                        }
                    }
                    thread.Interrupt();
                    thread.Join(5000);
                    thread.Abort();
#endif
                }
            } catch (Exception e)
            {
                Log.WriteServer(e);
            }
        }

        #endregion

        #region Debugging
        public static void DebugThreads()
        {
            Console.WriteLine("\nThread: Starting debug\n");
            foreach (Threads t in Threads.List)
            {
                Console.WriteLine("Thread: " + t.Object + " - " + t.User);
                if (Thread.CurrentThread == t.thread)
                {
                    Console.WriteLine("Thread: this one, not testing");
                    continue;
                }
                Console.WriteLine("Thread: suspend");
#pragma warning disable 612
                t.thread.Suspend();
                Thread.Sleep(5000);
                t.thread.Resume();
#pragma warning restore 612
                Console.WriteLine("Thread: resumed\n");
            }
            Console.WriteLine("Thread: all done\n");
            
        }

        #endregion
    }
}

