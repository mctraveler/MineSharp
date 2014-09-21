using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using Bot;
using MiscUtil.IO;
using MiscUtil.Conversion;
using System.Text;
using System.Security.Cryptography;
using MineProxy.NBT;
using System.Net.Sockets;
using MineProxy.Network;
using MineProxy.Data;
using MineProxy.Chatting;
using MineProxy.Worlds;
using Newtonsoft.Json;

namespace MineProxy
{
    public partial class Client
    {
        public string MinecraftUsername { get; set; }

        /// <summary>
        /// No longer Use the one from the vanilla server since it is in offline mode
        /// </summary>
        public Guid UUID { get { return Settings.UUID; } }

        /// <summary>
        /// Real name or color less nick
        /// </summary>
        [JsonIgnore]
        public string Name
        {
            get
            { 
                if (Settings.Nick == null)
                    return MinecraftUsername;
                return Format.StripFormat(Settings.Nick);
            }
        }

        /// <summary>
        /// Real name or colored nick(as assigned)
        /// </summary>
        public string NameOverHead
        {
            get
            { 
                if (Settings.Nick == null)
                    return MinecraftUsername;
                return Settings.Nick;
            }
        }

        /// <summary>
        /// The entity ID as the client knows it.
        /// Other entityID may me used with the backend server.
        /// </summary>
        public int EntityID { get; set; }

        public string Locale { get; set; }

        public string Language { get { return Locale.Split('_')[0]; } }

        public int ViewDistance { get; set; }

        public void Kick(string message)
        {
            try
            {
                Log.WritePlayer(this, "Kicked: " + message);
                string nick = MinecraftUsername;
                if (nick == null)
                    nick = "someone";
                Chatting.Parser.TellAdmin(Permissions.Ban, Chat.Gray + nick + " was kicked: " + message);
                Close(message);
            }
            catch (Exception e)
            {
                Log.Write(e, this);
            }
        }

        public bool Admin(Permissions test)
        {
            if (MinecraftServer.Admins.ContainsKey(MinecraftUsername) == false)
                return false;
            
            var p = MinecraftServer.Admins[MinecraftUsername];
            return (p & test) == test;
        }

        /// <summary>
        /// Return true if player has any of the permissions
        /// </summary>
        /// <returns><c>true</c>, if any was admined, <c>false</c> otherwise.</returns>
        /// <param name="test">Test.</param>
        public bool AdminAny(Permissions test)
        {
            if (MinecraftServer.Admins.ContainsKey(MinecraftUsername) == false)
                return false;

            if (test == Permissions.AnyAdmin)
                return true;

            var p = MinecraftServer.Admins[MinecraftUsername];
            return (p & test) != 0;
        }

        public bool Donor { get { return Donors.IsDonor(this); } }

        /// <summary>
        /// When false, no other player can attack you. At first attack to another player your pvp is set to true.
        /// </summary>
        public bool PvP = false;

        #region Chat

        public string LastOutTell { get; set; }

        public string LastInTell { get; set; }

        public string ChatChannel { get; set; }

        internal Entry ChatEntry { get; set; }

        #endregion

        public PlayerDat Dat = null;

        public void ReadDat()
        {
            Tag t = FileNBT.Read("world" + Path.DirectorySeparatorChar + "players" + Path.DirectorySeparatorChar + MinecraftUsername + ".dat");
            //Console.Error.WriteLine ("Loading DAT");
            //Console.Error.WriteLine (t);
            this.Dat = new PlayerDat(t);
        }

        public void WriteDat()
        {
            Tag t = Dat.ExportTag();
            //Console.Error.WriteLine ("Saving DAT");
            //Console.Error.WriteLine (t);
            FileNBT.Write(t, "world" + Path.DirectorySeparatorChar + "players" + Path.DirectorySeparatorChar + MinecraftUsername + ".dat");
        }

        void SessionClosed(WorldSession s, Exception e)
        {
            if (Phase == Phases.Gaming)
            {
                this.TellSystem(Chat.Red, "Session error: " + e.Message);
                if (s == Session)
                    SetWorld(World.Construct);
            }
            else
            {
                SetWorld(World.Void);
            }
        }

        #region Banned players

        public void BanByServer(DateTime bannedUntil, string reason)
        {
            Banned.Ban(null, MinecraftUsername, bannedUntil, reason);
        }

        #endregion
    }
}

