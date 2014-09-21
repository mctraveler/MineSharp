using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using MineProxy.Packets;
using MineProxy.Misc;

namespace MineProxy.Clients
{
    public class ChatClient : Client
    {
        TextWriter writer;

        public ChatClient(Socket client, NetworkStream stream) : base(client, stream)
        {
            writer = new StreamWriter(stream, Encoding.UTF8);
            writer.NewLine = "\r\n";
        }

        string LoginPrompt(TextReader reader)
        {   
            writer.Write(ColorCode("§6MCTraveler.eu\n§kLogin: "));
            writer.Flush();

            string user = reader.ReadLine();
            if (user == null)
                return null;
            writer.Write(ColorCode("Password: "));
            writer.Flush();
            string pass = reader.ReadLine();
            if (pass == null)
                return null;

            switch(user + " " + pass)
            {
                //case "nuxas ":
                //case "Player ":
                    //return user;
                default:
                    return null;
            }
        }

        protected override void ReceiverRunClient()
        {
            using (TextReader reader = new StreamReader(base.clientStream))
            {
                clientThread.WatchdogTick = DateTime.Now.AddMinutes(3);

                MinecraftUsername = LoginPrompt(reader);
                if (MinecraftUsername == null)
                    return;
                EntityID = 1;//Not used

                clientThread.User = MinecraftUsername;
                clientThread.State = "Logged in";

                Phase = Phases.Gaming;
                clientThread.State = "Gaming";

                Queue = new SendQueue(this);

                PlayerList.LoginPlayer(this);

                clientThread.WatchdogTick = DateTime.Now.AddHours(3);

                //Logged in

                string pl = "Players: ";
                foreach (var p in PlayerList.List)
                {
                    if (p.Settings.Cloaked != null)
                        continue;
                    if (p.MinecraftUsername == "Player")
                        continue;
                    pl += p.Name + ", ";
                }
                writer.WriteLine(pl);

                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        return;
                    line = line.Trim(' ', '\n', '\r', '\t');
                    if (line == "exit")
                        return;
                    if(line == "")
                        continue;
                    ChatLoop(line);

                    clientThread.WatchdogTick = DateTime.Now.AddHours(3);
                }
            }
        }

        void ChatLoop(string line)
        {
            var chat = new ChatMessageClient(line);
            FromClient(chat);
        }

        protected override void SendToClientInternal(PacketFromServer packet)
        {
            var chat = packet as ChatMessageServer;
            if (chat != null)
            {
                if(chat.Json.Text != null)
                    writer.WriteLine(ColorCode(chat.Json.Text));
                else
                    writer.WriteLine(chat.ToString()); //TODO: to the transaltion
                writer.Flush();
                return;
            }
            var disc = packet as Disconnect;
            if (disc != null)
            {
                writer.WriteLine(ColorCode(disc.Reason.Text));
                writer.Flush();
                return;
            }
        }

        const char esc = (char)27;
        static Dictionary<string, string> codes = new Dictionary<string, string>();

        static ChatClient()
        {
            codes.Add("§0", esc + "[30;21;47m"); //gray background
            codes.Add("§1", esc + "[34m");
            codes.Add("§2", esc + "[32m");
            codes.Add("§3", esc + "[36m");
            codes.Add("§4", esc + "[31m");
            codes.Add("§5", esc + "[35m");
            codes.Add("§6", esc + "[33m");
            codes.Add("§7", esc + "[37m");
            codes.Add("§8", esc + "[30;1m");
            codes.Add("§9", esc + "[34;1m");
            codes.Add("§a", esc + "[32;1m");
            codes.Add("§b", esc + "[36;1m");
            codes.Add("§c", esc + "[31;1m");
            codes.Add("§d", esc + "[35;1m");
            codes.Add("§e", esc + "[33;1m");
            codes.Add("§f", esc + "[37;1m");

            codes.Add("§k", esc + "[6m"); //Random
            //codes.Add("§l", esc + "[m"); //Bold
            codes.Add("§m", esc + "[9m"); //strike
            codes.Add("§n", esc + "[4m"); //underline
            codes.Add("§o", esc + "[3m"); //italic
            codes.Add("§r", esc + "[0m"); //reset
        }

        static string ColorCode(string input)
        {
            foreach (var c in codes)
            {
                input = input.Replace(c.Key, c.Value);
            }
            return input;
        }
    }
}

