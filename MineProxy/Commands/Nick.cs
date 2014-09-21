using System;
using MineProxy;

namespace MineProxy.Commands
{
    /// <summary>
    /// Change the apparent nick for admin "Player"
    /// </summary>
    public static class Nick
    {
		
        public static string Get(Client player)
        {
            if (player.Settings.Nick == null)
                return "no nick";
            else
                return player.MinecraftUsername + " has nick " + player.Settings.Nick;
        }
					
        public static void Set(Client target, string nick)
        {
            if (nick.Length > 16)
            {
                return;
            }
			
            if (nick == "-" || nick == "reset" || nick == "clear" || nick == target.MinecraftUsername)
                target.Settings.Nick = null;
            else
                target.Settings.Nick = nick;
            target.SaveProxyPlayer();
        }
					
    }
}

