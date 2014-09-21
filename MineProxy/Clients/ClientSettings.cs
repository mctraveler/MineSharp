using System;

namespace MineProxy
{
    public partial class ClientSettings
    {
        /// <summary>
        /// We store the real id given their username
        /// </summary>
        public Guid UUID { get; set; }

        /// <summary>optional bool Hellbanned = 5;</summary>
        public string Cloaked { get; set; }

        /// <summary>How the player appears in the game.</summary>
        public string Nick { get; set; }

        /// <summary>
        /// <para>User chosen password to login, if set mojang authentication is not used</para>
        /// </summary>
        public byte[] PasswordHash { get; set; }
        //public readonly MineProxy.ClientStats Stats = new MineProxy.ClientStats(); // Implemented by user elsewhere
        /// <summary>Chat feature</summary>
        public bool Firehose { get; set; }

        /// <summary>If chat guide should show, [near]</summary>
        public bool Help { get; set; }

        public TimeSpan Uptime { get; set; }

        /// <summary>Last time seen</summary>
        public DateTime LastOnline { get; set; }

        public DateTime SignedRules { get; set; }

        /// <summary>Display the timestamp in the chat</summary>
        public bool ShowTimestamp { get; set; }

        /// <summary>Show the region scoreboard</summary>
        public bool ShowRegionScoreboard { get; set; }

        public ClientStats Stats { get; set; }

        public DateTime FirstOnline { get; set; }

        public double WalkDistance { get; set; }

        public ClientSettings()
        {
            //Some defaults for new players
            Firehose = true;
            Help = true;
            ShowRegionScoreboard = true;
        }
    }
}

