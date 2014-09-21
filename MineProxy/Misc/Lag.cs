using System;
using System.Diagnostics;

namespace MineProxy
{
    public class Lag
    {
        
        public class MemoryUsage
        {
            public double Total { get; set; }
            public double Used { get; set; }
            
            public double TotalGB { get { return Total / 1024 / 1024 / 1024; } }
            public double UsedGB { get { return Used / 1024 / 1024 / 1024; } }
            
            public double Quota
            {
                get
                {
                    return (double)Used / Total;
                }
            }
        }
        
        /// <summary>
        /// Result from the free command
        /// </summary>
        public static MemoryUsage ServerMemoryUsage()
        {
            try
            {
                using (Process proc = new Process() {
                    StartInfo = new ProcessStartInfo {
                        FileName = "free",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                })
                {
                    proc.Start();
                    proc.WaitForExit();
                    //Skip labels
                    proc.StandardOutput.ReadLine();
                    string l = proc.StandardOutput.ReadLine();
                    string lc = proc.StandardOutput.ReadLine();
                    string[] values = l.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                    string[] valuesc = lc.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                    MemoryUsage mu = new MemoryUsage();
                    if (values.Length >= 3)
                    {
                        //Megs
                        mu.Total = double.Parse(values [1]) * 1024; 
                        mu.Used = double.Parse(valuesc [2]) * 1024; 
#if DEBUG
                        //mu.Used = 0.99 * mu.Total;
#endif
                    }
                    return mu;
                }
            } catch (Exception)
            {
                return null;
            }
        }
        
        public static string ServerLoad(bool colorCode)
        {
            try
            {
                using (Process proc = new Process() {
                    StartInfo = new ProcessStartInfo {
                        FileName = "uptime",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                })
                {
                    proc.Start();
                    proc.WaitForExit();
                    string l = proc.StandardOutput.ReadLine();
                    int p = l.IndexOf("load average: ") + "load average: ".Length;
                    l = l.Substring(p);
                    string line = "";
                    int m = 1;
                    foreach (string s in l.Replace(",", "").Split(' '))
                    {
                        double d;
                        if (double.TryParse(s, out d) == false)
                            double.Parse(s.Replace(".", ","));
                        if (colorCode) 
                            line += Chat.White;
                        line += m + " min: ";
                        if (colorCode)
                        {
                            if (d > 1.5)
                                line += Chat.Red;
                            else if (d > 0.9)
                                line += Chat.Yellow;
                            else
                                line += Chat.Green;
                        } else if (d > 1.5)
                            line += "<strong>";
                        
                        line += d.ToString("0%") + " ";
                        
                        if (colorCode == false && d > 1.5)
                            line += "</strong>";
                        
                        if (m == 5)
                            m = 15;
                        if (m == 1)
                            m = 5;
                    }
                    
                    return line;
                }
            } catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}

