using System;
using System.IO;
using MineProxy.Chatting;
using MineProxy.Clients;
using Newtonsoft.Json;

namespace MineProxy
{
    public partial class Client
    {
        /// <summary>
        /// local lock for accessing player data files
        /// </summary>
        static object playerSaveLock = new object();
        static readonly DateTime FirstOnlineEver = new DateTime(2011, 6, 17);

        public static ClientSettings LoadProxyPlayer(string username)
        {
            if (username != Path.GetFileName(username))
                return null;

            string path = "proxy/players/" + username + ".json";
            
            lock (playerSaveLock)
            {
                try
                {
                    var s = Json.Load<ClientSettings>(path);
                    if (s.FirstOnline < new DateTime(2000, 1, 1))
                    {
                        Console.WriteLine("First " + username + " " + s.FirstOnline);
                        s.FirstOnline = DateTime.Now;
                    }
                    if(s.Stats == null)
                        s.Stats = new ClientStats();
            
                    //Fix
                    if (s.FirstOnline < FirstOnlineEver)
                    {
                        Console.WriteLine("Firstonlinefix: " + s.FirstOnline + " -> " + FirstOnlineEver);
                        s.FirstOnline = FirstOnlineEver;
                    }
                    if (s.Uptime > TimeSpan.FromHours(24))
                        s.Help = false;
                    return s;

                } catch (InvalidDataException ide)
                {
                    Log.Write(ide, null);
                    return null;
                }
            }
        }

        internal void SaveProxyPlayer()
        {
            if (MinecraftUsername == null)
                return;
            if (MinecraftUsername != Path.GetFileName(MinecraftUsername))
                throw new InvalidDataException("Invalid username " + MinecraftUsername);

            if (Settings.Cloaked == null)
                Settings.LastOnline = DateTime.Now;

            string dirPath = "proxy/players/";
            string path = dirPath + MinecraftUsername + ".json";
            if (Directory.Exists(dirPath) == false)
                Directory.CreateDirectory(dirPath);

            lock (playerSaveLock)
            {
                //Update uptime
                Settings.Uptime += (DateTime.Now - LastConnected);
                LastConnected = DateTime.Now;

                Json.Save<ClientSettings>(path, Settings);
            }
        }
    }
}

