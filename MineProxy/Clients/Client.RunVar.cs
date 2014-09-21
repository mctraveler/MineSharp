using System;
using MineProxy.Plugins;
using MineProxy.Misc;

namespace MineProxy
{
    //What is stored on the client at runtime
    public partial class Client
    {
        #region Helpers to persistent data

        //local not persistent, but used to calculate the uptime
        private DateTime LastConnected = DateTime.Now;

        public TimeSpan Uptime
        {
            get { return Settings.Uptime + (DateTime.Now - LastConnected); }
        }

        #endregion

        #region Scoreboard
        
        /// <summary>
        /// Sidebar scoreboard
        /// </summary>
        /// <value>The score.</value>
        public Scoreboard Score
        {
            get{ return _scoreboard;}
            set
            {
                if (_scoreboard != null)
                    SendToClient(_scoreboard.Remove());
                _scoreboard = value;
                if (value == null)
                    return;
                
                
                SendToClient(value.CreateFill());
            }
        }
        private Scoreboard _scoreboard;

        #endregion
    }
}

