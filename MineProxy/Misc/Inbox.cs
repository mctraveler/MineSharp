using System;
using System.IO;

namespace MineProxy
{
    //Private message storage
    public static class Inbox
    {
        /// <summary>
        /// Path to inbox directory
        /// </summary>
        const string dir = "inbox";
        static readonly object locking = new object();

        /// <summary>
        /// Return null if invalid name
        /// </summary>
        static string PrepareName(string name)
        {
            name = name.ToLowerInvariant();
            string filename = Path.GetFileName(name);
            if (name != filename)
                return null;
            if (filename.Length > 16)
                return null;
            if (filename.Length == 0)
                return null;
            return filename;
        }

        /// <summary>
        /// Report inbox status to player
        /// </summary>
        public static void Status(Client player)
        {
            string name = PrepareName(player.MinecraftUsername);
            if (name == null)
            {
                player.TellSystem(Chat.Red, "Invalid name: " + player.MinecraftUsername);
                return;
            }
            string path = Path.Combine(dir, name);

            lock (locking)
            {
                if (File.Exists(path))
                    player.Inbox = File.ReadAllLines(path).Length;
                else
                    player.Inbox = 0;
            }
        }

        /// <summary>
        /// Read inbox messages to player
        /// </summary>
        public static void Read(Client player, string[] cmd, int iarg)
        {
            string name = PrepareName(player.MinecraftUsername);
            if (name == null)
            {
                player.TellSystem(Chat.Red, "Invalid name: " + player.MinecraftUsername);
                return;
            }
            string path = Path.Combine(dir, name);

            string[] messages;
            lock (locking)
            {
                if (File.Exists(path) == false)
                    messages = new string[0];
                else
                {
                    messages = File.ReadAllLines(path);
                    File.Delete(path);
                }
            }
            if (messages.Length == 0)
                player.TellSystem(Chat.Purple, "Inbox: no new messages");
            foreach (string line in messages)
            {
                string[] parts = line.Split(new char[]{'\t'}, 3);
                if (parts.Length != 3)
                    player.TellSystem(Chat.Purple, line);
                else
                {
                    DateTime timestamp;
                    DateTime.TryParse(parts [0], out timestamp);
                    TimeSpan ago = DateTime.Now - timestamp;
                    string agoString;
                    if (ago.TotalHours < 1)
                        agoString = ago.TotalMinutes.ToString("0") + " minutes";
                    else if (ago.TotalDays < 1)
                        agoString = ago.TotalHours.ToString("0") + " hours";
                    else
                        agoString = ago.TotalDays.ToString("0") + " days";

                    player.TellSystem(Chat.Yellow, "From " + parts [1] + " " + agoString + " ago");
                    player.TellSystem(Chat.White + "> ", parts [2]);
                }
            }

            Status(player);
        }
        
        /// <summary>
        /// Write message to player
        /// </summary>
        public static void Write(Client fromPlayer, string toPlayer, string message)
        {
            string name = PrepareName(toPlayer);
            if (name == null)
            {
                fromPlayer.TellSystem(Chat.Red, "Invalid name: " + toPlayer);
                return;
            }
            string path = Path.Combine(dir, name);

            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + fromPlayer.Name + "\t" + message + "\n";

            lock (locking)
            {
                File.AppendAllText(path, line);
            }

        }
        
        static Inbox()
        {
            try
            {
                //Prepare inbox
                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);
            } catch (Exception e)
            {
                Log.WriteServer(e);
            }
        }
    }
}

