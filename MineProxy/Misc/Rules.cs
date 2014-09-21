using System;
using MineProxy.NBT;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using MineProxy.Worlds;
using MineProxy.Packets.Plugins;
using MineProxy.Packets;

namespace MineProxy.RuleBook
{
    public static class Rules
    {
        static List<TagString> Pages;
        static TagCompound Content;

        public static void Load(string path)
        {
            var l = new TagList<TagString>();

            if (File.Exists(path))
            {
                var t = File.ReadAllLines(path);
                var p = "";
                foreach (string s in t)
                {
                    //New page
                    if (s == "\t----")
                    {
                        l.Add(new TagString(p));
                        p = "";
                        continue;
                    }
                    if (p == "")
                        p = s;
                    else
                        p = p + "\n" + s;
                }
                l.Add(new TagString(p));
            } else
            {
                l.Add(new TagString("Rules not found"));
            }

            var c = new TagCompound();
            c ["pages"] = l;
            c ["title"] = new TagString("Rules");
            Debug.WriteLine("Rules: " + c);
            Content = c;
        }

        public static void GetRules(Client c)
        {
            var si = new SlotItem(BlockID.BookAndQuill, 1, 0);
            si.Data = Content;

            var s = new SetSlot(0, 36, si);
            c.Queue.Queue(s);
        }

        //Records if the rules was signed
        public static void Verify(Client c, MCBook b)
        {
            if (b.Channel != MCBook.ChannelSign)
                return;

            try
            {
                var list = b.Content ["pages"].ToListString();

                if (list.Count != Pages.Count)
                    return;

                for (int p = 0; p < list.Count; p++)
                {
                    if (list [p].String != Pages [p].String)
                        return;
                }

            } catch (Exception e)
            {
                Log.Write(e, c);
            }

            //Signed
            bool first = c.Settings.SignedRules < new DateTime(2000, 01, 01);
            c.Settings.SignedRules = DateTime.Now;
            c.SaveProxyPlayer();
            c.TellSystem(Chat.Green, "You have signed the rules");
            c.TellSystem(Chat.Green, "Welcome to our server");
            if (first)
            {
				if (b.Content ["title"].String.ToLowerInvariant().Contains("mctraveler"))
                    World.Main.Send("give " + c.MinecraftUsername + " " + (int)BlockID.Diamond);
            }
        }
    }
}

