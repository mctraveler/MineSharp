using System;
using System.IO;
using System.Collections.Generic;
using MineProxy.Chatting;

namespace MineProxy
{
    public class Donor
    {
        public string Username;
        /// <summary>
        /// Number of donor slots being consumed
        /// </summary>
        public int Slots = 1;
        public DateTime Expire;
        /// <summary>
        /// Gratitude for multiple slots donated
        /// </summary>
        public DateTime ExpireLegacy;

        public override string ToString()
        {
            return Username + " x " + Slots + " " + Expire.ToShortDateString() + " " + ExpireLegacy.ToShortDateString();
        }

        public TimeSpan Left
        {
            get
            {
                var l = Expire - DateTime.Now;
                if (l < TimeSpan.Zero)
                    return TimeSpan.Zero;
                else 
                    return l;
            }
        }
        
        public static int CompareLongestFirst(Donor a, Donor b)
        {
            return DateTime.Compare(b.Expire, a.Expire);
        }
    }

    public static class Donors
    {
        public static Donor GetDonor(string username)
        {
            string lower = username.ToLowerInvariant();
            if (dict.ContainsKey(lower) == false)
                return null;
            return dict [lower];
        }

        public static bool IsDonor(string username)
        {
            if (dict.ContainsKey(username.ToLowerInvariant()) == false)
                return false;
            var e = dict [username.ToLowerInvariant()];
            return e.ExpireLegacy > DateTime.Now;
        }
        
        public static bool IsDonor(Client player)
        {
            return IsDonor(player.MinecraftUsername);
        }
        
        public static string Status(string username)
        {
            Donor d = GetDonor(username);
            if (d == null) 
                return "You are not a donor, use /donate to become one";
            if (d.Expire > DateTime.Now)
            {
                return "You are a donor level " + d.Slots + " for " + (d.Expire - DateTime.Now).TotalDays.ToString("0") + " more days";
            }
            if (d.ExpireLegacy > DateTime.Now)  
                return "You are a major donor, it expired " + (DateTime.Now - d.Expire).TotalDays.ToString("0") + " days ago";
            return Chat.Purple + "You are no longer a donor, it expired " + (DateTime.Now - d.Expire).TotalDays.ToString("0") + " days ago";
        }

        /// <summary>
        /// Reload the list of donors from donors.txt
        /// </summary>
        public static void Update(Dictionary<string,Donor> l)
        {
            //Prepare donor list and do the expire calculations
            DonorList = null;
            DonorListLegacy = null;
            DonorExpire8 = DateTime.MinValue;
            DonorExpire16 = DateTime.MinValue;

            List<Donor> dl = new List<Donor>();
            foreach (var d in l.Values)
                dl.Add(d);

            dl.Sort(Donor.CompareLongestFirst);
            int expCount = 0;

            foreach (Donor d in dl)
            {
                if (d.Expire < DateTime.Now)
                {
                    //Note legacy donors
                    if (d.ExpireLegacy > DateTime.Now)
                    {
                        if (DonorListLegacy == null)
                            DonorListLegacy = d.Username;
                        else
                            DonorListLegacy += ", " + d.Username;
                    }
                    //Skip expired donors and legacy donors
                    continue;
                }

                Debug.WriteLine("Donor: " + d.Username + " x " + d.Slots + "\t" + (d.Expire - DateTime.Now).TotalDays.ToString("0"));

                if (DonorList == null)
                    DonorList = d.Username;
                else
                    DonorList += ", " + d.Username;
                if (d.Slots > 1)
                    DonorList += " x " + d.Slots;

                expCount += d.Slots;
                
                if (expCount < 8 + 1)
                    DonorExpire8 = d.Expire;
                if (expCount < 16 + 1)
                    DonorExpire16 = d.Expire;
            }
            if (expCount < 8)
                DonorExpire8 = DateTime.MinValue;
            if (expCount < 16)
                DonorExpire16 = DateTime.MinValue;

            dict = l;
            //PlayerList.UpdateRefreshTabPlayers();
        }

        public static string DonorList { get; set; }

        public static string DonorListLegacy { get; set; }

        /// <summary>
        /// When the 7th donor expires
        /// </summary>
        public static DateTime DonorExpire8 { get; set; }
        /// <summary>
        /// When the 16th donor expires
        /// </summary>
        public static DateTime DonorExpire16 { get; set; }

        public static string ExpireCount(DateTime expire)
        {
            if (expire < DateTime.Now)
                return "expired";
            int days = (int)(expire - DateTime.Now).TotalDays;
            if (days <= 1)
                return "1 day";
            return days + " days";
        }

        static Dictionary<string,Donor> dict = new Dictionary<string, Donor>();

        public static string RenewMonth()
        {
            return (16 - CountDonors(DateTime.Now.AddDays(30))).ToString();
        }
        public static string RenewWeek()
        {
            return (16 - CountDonors(DateTime.Now.AddDays(7))).ToString(); 
        } 

        public static int CountDonors(DateTime date)
        {
            int total = 0;
            foreach (var d in dict.Values)
            {
                if (d.Expire < date)
                    continue;
        
                total += d.Slots;
            }
            return total;
        }

    }
}

