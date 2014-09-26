using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using MineProxy.NBT;
using MineProxy.Data;
using System.Text;
using System.Globalization;
using MineProxy.Commands;

namespace MineProxy
{
    public static class Settings
    {
        static FileSystemWatcher watcher;

        public const string SettingsPath = "Settings/";

        const string ConfigFile = "config.txt";
        const string AdminsFile = "admins.txt";
        const string DonorsFile = "donors.txt";
        const string RulesFile = "rules.txt";
        const string SpellFile = "spell.txt";

        const string ConfigPath = SettingsPath + ConfigFile;
        const string AdminsPath = SettingsPath + AdminsFile;
        const string DonorsPath = SettingsPath + DonorsFile;
        const string RulesPath = SettingsPath + RulesFile;
        const string SpellPath = SettingsPath + SpellFile;

        public static void Start()
        {
            if (watcher != null)
            {
                Log.WriteServer("Settingswatcher Already started once");
                return;
            }

            Directory.CreateDirectory(SettingsPath);

            //Load once at startup
            var files = Directory.GetFiles(SettingsPath);
            foreach (var f in files)
            {
                try
                {
                    LoadFile(Path.GetFileName(f));
                } catch (Exception e)
                {
                    Log.WriteServer(e);
                }
            }

            watcher = new FileSystemWatcher();
            watcher.Path = SettingsPath;
            watcher.Changed += Changed;
            watcher.Created += Changed;
            watcher.Renamed += Changed;
            watcher.EnableRaisingEvents = true;
        }

        static void LoadFile(string name)
        {
            switch (Path.GetFileName(name))
            {
                case ConfigFile:
                    LoadConfig();
                    return;
                case AdminsFile:
                    LoadAdmins();
                    return;
                case DonorsFile:
                    LoadDonors();
                    return;
                case RulesFile:
                    RuleBook.Rules.Load(RulesPath);
                    return;
                case SpellFile:
                    LoadSpelling();
                    return;
                default:
                    Log.WriteServer("Settings, Unknown file changed: " + name);
                    return;
            }
        }


        static void Changed(object sender, FileSystemEventArgs e)
        {
            if (MainClass.Active == false)
                return;
            try
            {
                //Log.WriteServer("Settings changed: " + e.Name);

                //Allow canges to settle
                Thread.Sleep(500);
                LoadFile(e.Name);
            } catch (ThreadInterruptedException)
            {
                return;
            } catch (Exception ex)
            {
                Log.WriteServer(ex);
            }
        }

        public static void LoadAdmins()
        {
            if (File.Exists(AdminsPath) == false)
                return;

            try
            {
                using (TextReader r = new StreamReader(AdminsPath, System.Text.Encoding.UTF8))
                {
                    var admins = new List<string>();
                    while (true)
                    {
                        string line = r.ReadLine();
                        if (line == null)
                            break;
                        line = line.Trim();
                        if (line.StartsWith("#"))
                            continue; //Comment
                        if (line.StartsWith("//"))
                            continue; //Comment
                        if (line == "")
                            continue;
                        var p = line.Trim().Split('\t');
                        admins.Add(p [0]);
                    }
                    MinecraftServer.Admins = admins;
                }
            } catch (Exception e)
            {
                Log.WriteServer(e);
            }
        }

        public static void LoadConfig()
        {
            if (File.Exists(ConfigPath) == false)
                return;

            try
            {
                using (TextReader r = new StreamReader(ConfigPath, System.Text.Encoding.UTF8))
                {
                    MinecraftServer.AdminEmail = r.ReadLine().Trim();
                    MinecraftServer.PingReplyMessage = r.ReadLine().Trim();
                    MinecraftServer.MaxSlots = int.Parse(r.ReadLine().Trim());
                    MinecraftServer.TexturePack = r.ReadLine().Trim();
                }
            } catch (Exception e)
            {
                Log.WriteServer(e);
            }

        }

        public static void LoadDonors()
        {
            if (File.Exists(DonorsPath) == false)
                return;

            Dictionary<string,Donor> l = new Dictionary<string, Donor>();

            //Read all donor statuses
            foreach (string u in System.IO.File.ReadAllLines(DonorsPath, System.Text.Encoding.UTF8))
            {
                if (u.Contains(" "))
                    continue;
                if (u.StartsWith("#"))
                    continue;
                string[] parts = u.Split(new char[]{'\t'}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                    continue;
                
                Donor d = new Donor();
                d.Username = parts [0].ToLowerInvariant();
                
                if (int.TryParse(parts [1], out d.Slots) == false)
                    continue;

                if (DateTime.TryParse(parts [2], out d.Expire) == false)
                    continue;
                 
                if (d.Expire < DateTime.Now)
                    d.Slots = 0;

                if (parts.Length < 4)
                    d.ExpireLegacy = d.Expire;
                else
                    if (DateTime.TryParse(parts [3], out d.ExpireLegacy) == false)
                    continue;

                //Duplicate, keep the longest one
                if (l.ContainsKey(d.Username))
                {
                    //Merge
                    var d2 = l [d.Username];

                    if (d.ExpireLegacy > d2.ExpireLegacy)
                        d2.ExpireLegacy = d.ExpireLegacy;

                    if (d.Expire > d2.Expire)
                    {
                        d2.Expire = d.Expire;
                    }

                    d2.Slots += d.Slots;

                    //Merge done
                    continue;
                }

                l.Add(d.Username, d);
            }
            Donors.Update(l);
        }


        public static void LoadSpelling()
        {
            if (File.Exists(SpellPath) == false)
            {
                Chatting.Spelling.Corrections = new Dictionary<string, string>();
                Log.WriteServer("Missing: " + SpellPath);
                return;
            }
            
            Dictionary<string,string> spelling = new Dictionary<string, string>();          
            string[] lines = File.ReadAllLines(SpellPath, Encoding.UTF8);
            foreach (string l in lines)
            {
                string[] words = l.Split('\t');
                if (words.Length != 2)
                {
                    Log.WriteServer("Spelling: Not 2 parts: " + l);
                    continue;
                }
                
                if (spelling.ContainsKey(words [0]))
                {
                    Log.WriteServer("Spelling: Duplicate: " + words [0]);
                    continue;
                }
                
                spelling.Add(words [0], words [1]);
            }
            Chatting.Spelling.Corrections = spelling;
            Console.WriteLine("Spelling loaded: " + spelling.Count);
        }
    }
}

