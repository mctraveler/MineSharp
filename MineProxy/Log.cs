using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.IO.Compression;
using MineProxy.NBT;
using System.Net;
using System.Diagnostics;
using MineProxy.Worlds;
using MineProxy.Packets;
using MineProxy.Packets.Plugins;

namespace MineProxy
{
    internal static class Log
    {
        /// <summary>
        /// Directory separator
        /// </summary>
        static DateTime LogDate = DateTime.MinValue;
        static readonly object logOpener = new object();
        static readonly string logPath = "log" + Path.DirectorySeparatorChar;

        static readonly string logPlayerPath = "log" + Path.DirectorySeparatorChar + "player" + Path.DirectorySeparatorChar;
        static readonly string logSignsPath = "log" + Path.DirectorySeparatorChar + "signs" + Path.DirectorySeparatorChar;
        static readonly string logBooksPath = "log" + Path.DirectorySeparatorChar + "books" + Path.DirectorySeparatorChar;
        static readonly string logExceptionPath = "log" + Path.DirectorySeparatorChar + "exception" + Path.DirectorySeparatorChar;
        static readonly string logChatPath = "log" + Path.DirectorySeparatorChar + "chat" + Path.DirectorySeparatorChar;
        static readonly string logActionPath = "log" + Path.DirectorySeparatorChar + "action" + Path.DirectorySeparatorChar;
        static readonly string logRegionPath = "log" + Path.DirectorySeparatorChar + "region" + Path.DirectorySeparatorChar;
        static readonly string logVotePath = "log" + Path.DirectorySeparatorChar + "vote" + Path.DirectorySeparatorChar;
        static readonly string logAuthFailPath = "log" + Path.DirectorySeparatorChar + "authfail" + Path.DirectorySeparatorChar;

        public static void Init()
        {
            Directory.CreateDirectory(logPath);
            
            Directory.CreateDirectory(logPlayerPath);
            Directory.CreateDirectory(logSignsPath);
            Directory.CreateDirectory(logBooksPath);
            Directory.CreateDirectory(logExceptionPath);
            Directory.CreateDirectory(logChatPath);
            Directory.CreateDirectory(logActionPath);
            Directory.CreateDirectory(logRegionPath);
            Directory.CreateDirectory(logVotePath);
            Directory.CreateDirectory(logAuthFailPath);
            
            OpenLog();

            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
        }

        static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
                Console.Error.WriteLine("Unhandled Exception, Terminating");
            else
                Console.Error.WriteLine("Unhandled Exception");
            Exception ex = e.ExceptionObject as Exception;

            if (ex == null)
                return;
            PrintException(ex);
            Log.Write(ex, null);
        }

        public static void PrintException(Exception e)
        {
            Console.Error.WriteLine(e.GetType().Name);
            Console.Error.WriteLine(e.Message);
            Console.Error.WriteLine(e.StackTrace);
            if (e.InnerException == null)
                return;
            Console.Error.WriteLine("Inner:");
            PrintException(e.InnerException);
        }

        static string today { get { return LogDate.ToString("yyyy-MM-dd"); } }

        static void OpenLog()
        {
            lock (logOpener)
            {
                if (LogDate == DateTime.Now.Date)
                    return;
                LogDate = DateTime.Now.Date;
            
                DisposeClose(ref playerLog, logPlayerPath);
                DisposeClose(ref signLog, logSignsPath);
                DisposeClose(ref bookLog, logBooksPath);
                DisposeClose(ref exceptions, logExceptionPath, " " + DateTime.Now.ToString("HH'.'mm"));
                DisposeClose(ref proxychat, logChatPath);
                DisposeClose(ref actionLog, logActionPath);
                DisposeClose(ref regionLog, logRegionPath);
                DisposeClose(ref voteWriter, logVotePath);
                DisposeClose(ref authFailWriter, logAuthFailPath);
            }
        }

        static void DisposeClose(ref TextWriter s, string path)
        {
            DisposeClose(ref s, path, "");
        }

        static void DisposeClose(ref TextWriter s, string path, string suffix)
        {
            var old = s as StreamWriter;
            s = new StreamWriter(path + today + suffix, true);

            if (old == null)
                return;

            bool empty = (old.BaseStream.Position == 0);
            string name = (old.BaseStream as FileStream).Name;
            old.Flush();
            old.Close();

            if (empty)
            {
                File.Delete(name);
            }
        }
        
        public static void Flush()
        {
            if (playerLog != null)
                playerLog.Flush();
            if (signLog != null)
                signLog.Flush();
            if (bookLog != null)
                bookLog.Flush();
            //if (usersLog != null)
            //  usersLog.Flush ();
            if (exceptions != null)
                exceptions.Flush();
            if (proxychat != null)
                proxychat.Flush();
            if (actionLog != null)
                actionLog.Flush();
            if (regionLog != null)
                regionLog.Flush();
            if (voteWriter != null)
                voteWriter.Flush();
        }
        
        static string Timestamp { get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); } }
        
        static TextWriter actionLog;
        
        public static void WriteAction(Client victim, Attacked attack, bool fatal)
        {
            if (LogDate != DateTime.Now.Date)
                OpenLog();
            
            double distance = victim.Session.Position.DistanceTo(attack.By.Session.Position);
            
            if (fatal)
                actionLog.WriteLine(Timestamp + "\t" +
                    victim.MinecraftUsername + "\tkilled by\t" + attack.By.MinecraftUsername + "\t" + attack.Item + "\t" + distance.ToString("0.00"));
            else
                actionLog.WriteLine(Timestamp + "\t" +
                    victim.MinecraftUsername + "\tattacked by\t" + attack.By.MinecraftUsername + "\t" + attack.Item + "\t" + distance.ToString("0.00"));
            actionLog.Flush();
        }
        
        static TextWriter regionLog;
        
        public static void WriteRegion(Client player, WorldRegion region, bool inside)
        {
            if (LogDate != DateTime.Now.Date)
                OpenLog();
            
            if (inside)
                regionLog.WriteLine(Timestamp + "\t" +
                    player.MinecraftUsername + "\tEntered\t" + region + "\tat" + player.Session.Position);
            else
                regionLog.WriteLine(Timestamp + "\t" +
                    player.MinecraftUsername + "\tLeft\t" + region + "\tat" + player.Session.Position);
                    
            //regionLog.Flush ();
        }
        
        static TextWriter playerLog;
        
        /// <summary>
        /// Player activities log
        /// </summary>
        public static void WritePlayer(Client player, string message)
        {
            if (LogDate != DateTime.Now.Date)
                OpenLog();

            playerLog.Write(Timestamp + "\t");
            if (player.MinecraftUsername == null)
                playerLog.Write(player.RemoteEndPoint);
            else
                playerLog.Write(player.MinecraftUsername);
            playerLog.WriteLine("\t" + message);
            //playerLog.Flush ();
        }
        
        /// <summary>
        /// Player activities log
        /// </summary>
        public static void WritePlayer(WorldSession session, string message)
        {
            WritePlayer(session.Player, message);
        }
        
        /// <summary>
        /// Player activities log
        /// </summary>
        public static void WritePlayer(string username, string message)
        {
            if (LogDate != DateTime.Now.Date)
                OpenLog();

            playerLog.WriteLine(Timestamp + "\t" +
                username + "\t" + message);
            //playerLog.Flush ();
        }
        
        static TextWriter signLog;
        
        public static void WriteSign(Client player, UpdateSignClient sign)
        {
            if (LogDate != DateTime.Now.Date)
                OpenLog();

            signLog.WriteLine(Timestamp + "\t" + player.MinecraftUsername + "\t" + sign.Position + "\t" + sign.Text1 + "\t" + sign.Text2 + "\t" + sign.Text3 + "\t" + sign.Text4);
            //signLog.Flush ();
        }

        static TextWriter bookLog;

        public static void WriteBook(Client player, MCBook pm)
        {
            if (LogDate != DateTime.Now.Date)
                OpenLog();

            try
            {
                if (pm.Content ["author"] != null && pm.Content ["title"] != null)
                    bookLog.WriteLine(Timestamp + "\t" + player.MinecraftUsername + "\t" + pm.Content ["title"].String + " by " + pm.Content ["author"].String);
                else
                    bookLog.WriteLine(Timestamp + "\t" + player.MinecraftUsername + "\tUnsigned");
                foreach (TagString p in pm.Content["pages"].ToListString())
                    bookLog.WriteLine(Timestamp + "\t" + player.MinecraftUsername + "\t" + p.String);

            } catch (Exception e)
            {
                Log.Write(e, player);
            }
        }
        
        //static TextWriter usersLog;
        
        public static void WriteUsersLog()
        {
            //Disabled
            /*
            if (LogDate != DateTime.Now.Date)
                OpenLog ();

            string users = "";
            Client[] list = PlayerList.List;
            foreach (Client p in list) {
                users += p.MinecraftUsername + ", ";                    
            }
            usersLog.WriteLine (Timestamp + "\t" + list.Length + "\t" + users);
            //usersLog.Flush ();
            */
        }
        
        static TextWriter exceptions;
        
        public static void WriteServer(Exception e)
        {
            Write(e, null);
        }

        static string lastWriteServer = null;

        public static void WriteServer(string error)
        {
            //Prevent repetitive logs
            if (error == lastWriteServer)
                return;
            lastWriteServer = error;

            Debug.WriteLine("WriteServer: " + error);

            if (LogDate != DateTime.Now.Date)
                OpenLog();

            try
            {
                exceptions.WriteLine(Timestamp + "\t" + error);
                //exceptions.Flush ();
            } catch (Exception x)
            {
                PrintException(x);
                Console.Error.WriteLine("Original error:" + error);
            }
        }

        public static void WriteServer(string error, Exception e)
        {
            Debug.WriteLine("WriteServer: " + error);
            
            if (LogDate != DateTime.Now.Date)
                OpenLog();
            
            try
            {
                exceptions.WriteLine(Timestamp + "\t" + error);
                exceptions.WriteLine(e.GetType().Name + ": " + e.Message);
                exceptions.WriteLine(e.StackTrace);
                if (e.InnerException != null)
                    Write(e.InnerException, null);
                exceptions.WriteLine();
            } catch (Exception x)
            {
                PrintException(x);
                Console.Error.WriteLine("Original error:" + error);
                if (e == null)
                    return;
                PrintException(e);
            }
        }

        public static void Write(Exception e, Client player)
        {
            if (e == null)
                return;
            
            //Don't log a few items
            if (player != null)
            {
                if (e is EndOfStreamException)
                    return;
                if (e is System.Net.Sockets.SocketException)
                    return;
            }

            if (LogDate != DateTime.Now.Date)
                OpenLog();

            try
            {
                exceptions.Write(Timestamp + "\t");
                if (player != null)
                {
                    exceptions.Write(player.MinecraftUsername + "\t");
                }
                PrevException prev = e as PrevException;
                if (prev == null)
                {
                    exceptions.WriteLine(e.GetType().Name + ": " + e.Message);
                    exceptions.WriteLine(e.StackTrace);
                    if (e.InnerException != null)
                        Write(e.InnerException, player);
                    exceptions.WriteLine();
                } else
                    exceptions.WriteLine(prev.Packet);
                //exceptions.Flush ();
            } catch (Exception x)
            {
                PrintException(x);
                PrintException(e);
            }
        }
        
        static TextWriter proxychat;
        
        public static void WriteChat(Client player, string channel, int receivers, string message)
        {
            if (LogDate != DateTime.Now.Date)
                OpenLog();

            proxychat.Write(Timestamp + "\t");
            proxychat.WriteLine(
                player.MinecraftUsername + "\t" +
                channel + "\t" +
                receivers + "\t" +
                message);
            //proxychat.Flush ();
        }

        static TextWriter voteWriter;
        
        public static void WriteVote(string service, string username, string address, string timestamp)
        {
            if (LogDate != DateTime.Now.Date)
                OpenLog();

            voteWriter.Write(Timestamp + "\t");
            voteWriter.WriteLine(
                service + "\t" +
                username + "\t" +
                address + "\t" +
                timestamp);
            //voteWriter.Flush ();
        }

        static TextWriter authFailWriter;
        
        public static void WriteAuthFail(string username, IPEndPoint address, string message)
        {
            if (LogDate != DateTime.Now.Date)
                OpenLog();
            
            authFailWriter.WriteLine(Timestamp + "\t" +
                username + "\t" +
                address + "\t" +
                message);
            //authFailWriter.Flush();
        }

    }
}

