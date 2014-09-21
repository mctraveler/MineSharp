using System;
using MineProxy.Packets;
using MineProxy.Data;
using MineProxy.Plugins;

namespace MineProxy.Regions
{
    /// <summary>
    /// Manages the scoreboard updates for a single player
    /// </summary>
    public static class ScoreboardRegionManager
    {
        public static void UpdateAllPlayersRegion()
        {
            foreach (var p in PlayerList.List)
            {
                UpdateRegion(p);
            }
        }

        public static void UpdateRegion(Client player)
        {
            var w = player.Session.CurrentRegion;
            if (w == null)
            {
                player.Score = null;
                return;
            }

            if (player.Settings.ShowRegionScoreboard == false)
                return;

            var score = new Scoreboard("region", w.ColorName, ScreenPosition.Sidebar);

            score.Add(Chat.Italic + w.ColorType, 99);
            //score.Add(Chat.Gray + Chat.Underline + Chat.Bold + "Residents", w.ResidentCount);
            int n = 0;
            foreach (var p in w.Residents.ToArray())
            {
                if (p == player.MinecraftUsername)
                    score.Add(p, 1);
                else
                    score.Add(Chat.Gray + p, 1);
                n++;
                if (n >= 10)
                    break;
            }

            player.Score = score;

        }
        
    }
}

