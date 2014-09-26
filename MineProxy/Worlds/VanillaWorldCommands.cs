using System;
using MineProxy.Commands;
using MineProxy.Packets;

namespace MineProxy.Worlds
{
    public class VanillaWorldCommands : CommandManager
    {
        public VanillaWorldCommands()
        {
            AddCommand(AFK, "afk");
            AddCommand(NoRain, "norain", "rain", "rainoff", "nosnow");
            AddAdminCommand(Spawner, "spawner", "spawn");
        }

        static void AFK(Client player, string[] cmd, int iarg)
        {
            if (Banned.CheckBanned(player) != null)
                return;
            player.SetWorld(World.AFK);
        }
        
        static void NoRain(Client player, string[] cmd, int iarg)
        {
            ChangeGameState ns = new ChangeGameState(GameState.EndRaining);
            player.Queue.Queue(ns);
        }
        
        static void Spawner(Client player, string[] cmd, int iarg)
        {
            if (!player.Admin())
                throw new ErrorException("Spawn does no longer work");

            VanillaSession rs = player.Session as VanillaSession;
            if (rs.Spawners.Count == 0)
            {
                player.TellSystem(Chat.Purple, "No spawners yet detected");
                return;
            }
            CoordInt nearby = rs.Spawners [0];
            double dist = nearby.DistanceTo(rs.Position);
            foreach (CoordInt c in rs.Spawners)
            {
                double cdist = c.DistanceTo(rs.Position);
                if (cdist < 10)
                    continue;
                if (cdist < dist)
                {
                    dist = cdist;
                    nearby = c;
                }
            }   
            player.Warp(nearby.CloneDouble(), rs.Dimension, player.Session.World);
        }
        
    }
}

