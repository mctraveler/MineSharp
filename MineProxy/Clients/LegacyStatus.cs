using System;
using System.IO;
using MiscUtil.IO;
using System.Text;
using MiscUtil.Conversion;
using System.Threading;
using System.Collections.Generic;
using MineProxy.Worlds;

namespace MineProxy.Clients
{
    public static class LegacyStatus
    {
        static byte[] cache;
        static Timer t;

        static LegacyStatus()
        {
            Update(null);
            t = new Timer(Update);
            t.Change(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public static void SendStatus(Stream stream)
        {
            stream.Write(cache, 0, cache.Length);
        }

        static void Update(object o)
        {
            int count = 0;
            int max = MinecraftServer.MaxSlots;
            List<string> players = new List<string>();
            foreach (Client p in PlayerList.List)
            {
                if (p.MinecraftUsername == "Player")
                    continue;
                if (p.Settings.Cloaked != null)
                    continue;
                count ++;
                players.Add(p.Name);
                if (p.Session is VanillaSession == false)
                    max ++;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                ms.WriteByte(0xff);
                EndianBinaryWriter w = new EndianBinaryWriter(EndianBitConverter.Big, ms);
                WriteString16(w, 
                    "§1\0" +
                    ((int)MinecraftServer.FrontendVersion).ToString() + "\0" +
                    MinecraftServer.FrontendVersion.ToText() + "\0" +
                    (MinecraftServer.PingReplyMessage ?? "Hi") + "\0" +
                    count + "\0" + max);

                cache = ms.ToArray();
                //Debug.WriteLine(BitConverter.ToString(cache));
            }
        }

        static void WriteString16(EndianBinaryWriter writer, string message)
        {   
            byte[] buffer = Encoding.BigEndianUnicode.GetBytes(message);
            short length = (short)(buffer.Length / 2);
            if (message.Length != length)
                throw new InvalidProgramException();
            writer.Write(length);
            writer.Write(buffer);
        }

    }
}

