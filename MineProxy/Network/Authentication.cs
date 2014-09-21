using System;
using System.Net;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Text;

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

        public static string VerifyUserLogin(string username, byte[] sharedKey, byte[] serverId)
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
                byte[] bHash = sha.ComputeHash(ms.ToArray());
                hash = McHex(bHash);
            }

            string url = "http://session.minecraft.net/game/checkserver.jsp?user=" + username + "&serverId=" + hash;
            try
            {
                using (WebClient client = new WebClient())
                {

                    Debug.WriteLine("Auth: Verifying against " + url);

                    // Download data.
                    byte[] resp = client.DownloadData(url);
                    string r = Encoding.ASCII.GetString(resp);
                    if (r != "YES")
                    {
                        Debug.WriteLine("Auth: Failed login.minecraft.net expected YES got: " + r);
                        Debug.WriteLine("ServerID: " + McHex(serverId));
                        Debug.WriteLine("Hash: " + hash);
                        return r;
                    }

                    Debug.WriteLine("Auth: Success login.minecraft.net said YES");
                    Debug.WriteLine("ServerID: " + McHex(serverId));
                    Debug.WriteLine("Hash: " + hash);
                    return null; //no error == success
                }
            } catch (WebException we)
            {
                return we.Message;
            }
        }

        public static string McHex(byte[] hex)
        {
            string s = BitConverter.ToString(hex).Replace("-", "");
            if ((hex [0] & 0x80) == 0)
                return s.TrimStart('0');
            byte[] inv = new byte[hex.Length];

            //Inverse all bytes and add a "-"
            for (int n = hex.Length - 1; n >= 0; n--)
            {
                inv [n] = (byte)~hex [n];
            }
            inv [inv.Length - 1] += 1;
            s = "-" + BitConverter.ToString(inv).Replace("-", "");
            return s;
        }

        

    }
}

