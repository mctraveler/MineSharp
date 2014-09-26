using System;
using System.Net;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using MineProxy.Network;

namespace MineProxy
{
    public static class Authentication
    {
        static readonly object loginLock = new object();

        /// <summary>
        /// Earliest time for the next login, prevent too frequent login requests
        /// </summary>
        static DateTime nextLogin = DateTime.Now;

        static readonly TimeSpan loginSeparation = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Help from
        /// http://wiki.vg/Protocol_Encryption#Server
        /// </summary>
        public static AuthResponse VerifyUserLogin(string username, byte[] sharedKey, byte[] serverId)
        {
            lock (loginLock)
            {
                TimeSpan delay = nextLogin - DateTime.Now;
                if (delay > loginSeparation)
                    delay = loginSeparation;
                if (delay.Ticks > 0)
                    System.Threading.Thread.Sleep(delay);
                nextLogin = DateTime.Now + loginSeparation;
            }

            SHA1Managed sha = new SHA1Managed();
            string hash;

            //Auth hash
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] sid = Encoding.ASCII.GetBytes(McHex(serverId));
                ms.Write(sid, 0, sid.Length);
                ms.Write(sharedKey, 0, sharedKey.Length);
                ms.Write(MinecraftServer.RsaBytes, 0, MinecraftServer.RsaBytes.Length);
                ms.Write(Encoding.ASCII.GetBytes("Hell"), 0, 4);
                byte[] bHash = sha.ComputeHash(ms.ToArray());
                hash = McHex(bHash);
            }

            //Or should hash be the serverID only?
            string url = "https://sessionserver.mojang.com/session/minecraft/hasJoined?username=" + username + "&serverId=" + hash;
            try
            {
                using (WebClient client = new WebClient())
                {

                    Debug.WriteLine("Auth: Verifying against " + url);

                    // Download data.
                    byte[] resp = client.DownloadData(url);
                    var authResponse = Json.Deserialize<AuthResponse>(resp);
                    #if DEBUG
                    string r = Encoding.ASCII.GetString(resp);
                    Console.WriteLine(r);
                    #endif
                    return authResponse;
                }
            }
            catch (WebException)
            {
                return new AuthResponse();
            }
        }

        public static string McHex(byte[] hex)
        {
            string s = BitConverter.ToString(hex).Replace("-", "");
            if ((hex[0] & 0x80) == 0)
                return s.TrimStart('0');
            byte[] inv = new byte[hex.Length];

            //Inverse all bytes and add a "-"
            for (int n = hex.Length - 1; n >= 0; n--)
            {
                inv[n] = (byte)~hex[n];
            }
            inv[inv.Length - 1] += 1;
            s = "-" + BitConverter.ToString(inv).Replace("-", "");
            return s;
        }

        

    }
}

