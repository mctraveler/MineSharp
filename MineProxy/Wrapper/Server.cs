using System;
using System.Threading;
using System.Diagnostics;
using System.IO;
using MineProxy;

namespace MineSharp.Wrapper
{
    public class Server
    {
        public readonly ManualResetEvent Running = new ManualResetEvent(false);

        /// <summary>
        /// for good
        /// </summary>
        readonly ManualResetEvent shutdown = new ManualResetEvent(false);

        /// <summary>
        /// Directory name
        /// </summary>
        public readonly string Name;
        readonly Thread thread;
        public DateTime LastReceived;
        readonly object processLock = new object();
        Process process;
        readonly ProcessStartInfo psi = new ProcessStartInfo();
        bool exit = false;

        public Server(string dirName)
        {
            this.Name = dirName;
            this.thread = new Thread(Run);

            psi.FileName = "java";
            psi.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), Name);

            string argsPath = Path.Combine(psi.WorkingDirectory, "args");
            if (File.Exists(argsPath))
                psi.Arguments = File.ReadAllText(argsPath) + " -jar " + MineSharp.Settings.MinecraftServerJar + " nogui";
            else
                psi.Arguments = "-Xincgc -jar " + MineSharp.Settings.MinecraftServerJar + " nogui";

            psi.CreateNoWindow = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
        }

        public override string ToString()
        {
            try
            {
                return string.Format("[{0} th={1} pid={2} Running={3}]", Name, thread.ManagedThreadId, process.Id, Running);
            }
            catch
            {
                return string.Format("[{0} th={1} pid=- Running={2}]", Name, thread.ManagedThreadId, Running);
            }
        }

        public void Start()
        {
            thread.Start();
        }

        public void Stop()
        {
            exit = true;
            SendCommand("stop");
            if (shutdown.WaitOne(TimeSpan.FromSeconds(30)) == false)
            {
                //Always kill it
            }
            Kill();
        }

        void Log(string message)
        {
            Console.WriteLine(this + " " + message);
        }

        void Log(Exception e)
        {
            Console.Error.WriteLine(this + " " + e.GetType().Name + ": " + e.Message);
            Console.Error.WriteLine(e.StackTrace);
        }

        public void Run()
        {
            Log("run");

            while (Program.Exit.WaitOne(0) == false)
            {
                if (exit)
                {
                    Log("exit loop");
                    break;
                }

                Log("Starting: " + psi.FileName + " " + psi.Arguments);
                using (Process p = Process.Start(psi))
                {
                    try
                    {
                        Running.Set();
                        BackendManager.SendToClients(Name + "\tstarted");

                        Console.Error.WriteLine("Minecraft server running");
                        lock (processLock)
                        {
                            process = p;
                        }
                        Log("started");

                        Thread olt = new Thread(OutListener);
                        Thread elt = new Thread(ErrorListener);
                        olt.Start();
                        elt.Start();
                        p.WaitForExit();
                        Log("Exited with code: " + p.ExitCode);
                        olt.Abort();
                        elt.Abort();
                    }
                    catch (Exception e)
                    {
                        Log(e);
                    }
                    finally
                    {
                        lock (processLock)
                        {
                            Kill();
                            process = null;
                        }

                        Running.Reset();
                        try
                        {
                            BackendManager.SendToClients(Name + "\tstopped");
                        }
                        catch (Exception ex)
                        {
                            Log(ex);
                        }
                    }
                    Console.Error.WriteLine("Restarting minecraft");
                }
            }
            shutdown.Set();
        }

        void OutListener()
        {
            while (true)
            {
                try
                {
                    string line = process.StandardOutput.ReadLine();
                    if (line == null)
                        return;

                    LastReceived = DateTime.Now;

                    BackendManager.SendToClients(Name + "\tout\t" + line);

                }
                catch (Exception e)
                {
                    Log(e);
                }
            }
        }

        void ErrorListener()
        {
            while (true)
            {

                try
                {
                    string line = process.StandardError.ReadLine();
                    if (line == null)
                        return;

                    BackendManager.SendToClients(Name + "\terror\t" + line);

                    LastReceived = DateTime.Now;

                    if (line.Contains(" [SEVERE] Unexpected exception"))
                    {
                        //restart
                        Log("Sending stop because: " + line);
                        SendCommand("stop");
                    }

                }
                catch (Exception e)
                {
                    Log(e);
                }
            }
        }

        public void Kill()
        {
            Log("Kill()");

            lock (processLock)
            {
                if (process == null)
                    return;
                try
                {
                    Log("process.Kill()");
                    process.Kill();
                }
                catch
                {
                }
                try
                {
                    Log("kill -kill " + process.Id);
                    Process.Start("/bin/kill", "-kill " + process.Id).WaitForExit();
                }
                catch (Exception e2)
                {
                    Log(e2);
                }
            }
        }

        public void SendCommand(string command)
        {
            lock (processLock)
            {
                #if DEBUG
                Console.WriteLine(">>> " + command);
                #endif

                if (process == null)
                {
                    Log("No process for command: " + command);
                    return;
                }
                try
                {
                    process.StandardInput.WriteLine(command);
                }
                catch (Exception e)
                {
                    Log("Error writing command: " + command);
                    Log(e);
                }
            }
        }
    }
}

