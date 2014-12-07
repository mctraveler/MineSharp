using System;
using System.Threading;
using System.IO;
using System.Diagnostics;
using MineProxy;

namespace MineSharp.Wrapper
{
    public static class BackupThread
    {

        static Thread thread;

        public static void Start()
        {
            thread = new Thread(BackupThreadRun);
            thread.Start();
        }

        static void BackupThreadRun()
        {
            DateTime nextBackup = DateTime.Now.AddMinutes(30);
            DateTime nextDisk = DateTime.Now.AddMinutes(5);
            //DateTime nextRestart = DateTime.Now.AddHours (6);
            try
            {
                while (true)
                {
                    //Wait one minute
                    if (Program.Exit.WaitOne(new TimeSpan(0, 1, 0)))
                        break;

                    //Watchdog
                    Watchdog.Test();

                    //Every minute we store to disk
                    //MainClass.SendCommand("save-all");

                    //5 minute disk copy
                    if (nextDisk < DateTime.Now)
                    { // || nextBackup < DateTime.Now
                        nextDisk = DateTime.Now.AddMinutes(5);
                        //MainClass.SendCommand ("save-off");
//						MainClass.SendCommand ("tell Player saving to disk");
//						MainClass.SendCommand ("tell nuxas saving to disk");
                        BackendManager.SendCommandAll("save-all");

                        if (Program.Exit.WaitOne(new TimeSpan(0, 0, 5)))
                            break;

                        try
                        {
                            string path = "/home/bin/backupDisk";
                            if (File.Exists(path))
                            {
                                Process p = Process.Start(path);
                                p.WaitForExit();
                                Console.WriteLine("BackupDisk done: " + p.ExitCode);
                            }
                        } catch (Exception e)
                        {
                            Console.Error.WriteLine(e.Message);
                        } finally
                        {
                            //MainClass.SendCommand ("save-on");
//							MainClass.SendCommand ("tell Player save done");
//							MainClass.SendCommand ("tell nuxas save done");
                        }
                    }

                    //6h restart
                    /*
					if (nextRestart < DateTime.Now) {
						nextRestart = DateTime.Now.AddHours (6);

						MainClass.SendCommand ("say Scheduled restart, this will take 20 seconds");
                        if (Program.Exit.WaitOne(new TimeSpan(0, 0, 3)))
                            break;

						MainClass.SendCommand ("save-off");
						MainClass.SendCommand ("save-all");

                        if (Program.Exit.WaitOne(new TimeSpan(0, 0, 10)))
                            break;

						MainClass.Kill();

                        if (Program.Exit.WaitOne(new TimeSpan(0, 0, 3)))
                            break;

						MainClass.SendCommand ("say Now we should be back up and running");
					}*/

                    //Hourly backups
                    if (nextBackup < DateTime.Now)
                    {
                        try
                        {
                            string path = "/home/bin/backup";
                            if (File.Exists(path))
                            {
                                Process p = Process.Start(path);
                                p.WaitForExit();
                                Console.WriteLine("Backup done " + p.ExitCode);
                            }
                        } catch (Exception e)
                        {
                            Console.Error.WriteLine(e.Message);
                        } finally
                        {
                            nextBackup = DateTime.Now.AddMinutes(60);
                            BackendManager.SendCommandAll("save-on");
                        }
                    }
                }
            } catch (ThreadInterruptedException)
            {
                return;
            }
        }
    }
}

