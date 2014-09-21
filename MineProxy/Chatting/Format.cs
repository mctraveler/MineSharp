using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MineProxy.Packets;

namespace MineProxy.Chatting
{
    public class Format
    {
        /// <summary>
        /// Return a string without any format codes
        /// </summary>
        public static string StripFormat(string nick)
        {
            return Regex.Replace(nick, "§.", "").Replace("§", "");
        }


        const int wrapLength = 57;
        public static List<ChatMessageServer> Split(string prefix, string message, ChatPosition pos)
        {
            List<ChatMessageServer> msg = new List<ChatMessageServer>();

            if (prefix.Length + message.Length <= wrapLength)
            {
                msg.Add(ChatMessageServer.CreateText(prefix + message, pos));
                return msg;
            }

            //Split long messages
            if (prefix.Length > 57)
                return Split("", prefix + message, pos); //Split long prefix too
            int max = 57 - prefix.Length;
            if (max <= 30)
            {
                msg.Add(ChatMessageServer.CreateText(prefix));
                prefix = "";
                max = 57;
            }
            
            string[] parts = message.Split(' ');
            
            string m = "";
            foreach (string  p in parts)
            {
                //Fit
                if (m.Length + p.Length + (m == "" ? 0 : 1) <= max)
                {
                    if (m == "")
                        m = p;
                    else
                        m += " " + p;
                    continue;
                }
                //Dont fit, push it
                if (m == "")
                {
                    int step = max - prefix.Length;
                    for (int n = 0; n < p.Length; n+= step)
                    {
                        if (n + step < p.Length)
                            msg.Add(ChatMessageServer.CreateText(prefix + p.Substring(n, step)));
                        else
                            msg.Add(ChatMessageServer.CreateText(prefix + p.Substring(n)));
                        
                    }
                    continue;
                }
                //Send old, start a new
                msg.Add(ChatMessageServer.CreateText(prefix + m));
                m = p;
            }
            if (m != "")
                msg.Add(ChatMessageServer.CreateText(prefix + m));
            
            return msg;
        }
        
        /// <summary>
        /// These are applied relative to the receiving player
        /// </summary>
        public static string ReceiverMacros(Client player, string message)
        {
            message = message.Replace("\t", "    ");
            message = message.Replace("<serverversion>", MinecraftServer.BackendVersion.ToText());
            message = message.Replace("<name>", player.Name);
            message = message.Replace("<hours>", player.Uptime.TotalHours.ToString("0.0"));
            message = message.Replace("<donors>", Donors.DonorList);
            message = message.Replace("<legacydonors>", Donors.DonorListLegacy);
            message = message.Replace("<since>", (DateTime.Now - new DateTime(2011, 06, 17, 0, 0, 0)).TotalDays.ToString("0"));
            message = message.Replace("<expire8>", Donors.ExpireCount(Donors.DonorExpire8));
            message = message.Replace("<expire16>", Donors.ExpireCount(Donors.DonorExpire16));
            message = message.Replace("<renewmonth>", Donors.RenewMonth());
            message = message.Replace("<renewweek>", Donors.RenewWeek());
            message = message.Replace("<pos>", player.Session.Position.ToString("0"));
            return message;
        }
    }
}

