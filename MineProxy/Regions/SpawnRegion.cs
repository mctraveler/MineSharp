using System;
using System.Threading;
using System.Collections.Generic;
using MineProxy.Chatting;
using MineProxy.Worlds;
using MineProxy.Packets;

namespace MineProxy
{
    public static class SpawnRegion
    {
        public const string Type = "night";
        //Name = "Lost City Spawn Point";
		
        public static void Start()
        {
            Threads.Run("Spawn Thunder and Banned", Run);
        }

        static DateTime nextThunder = DateTime.Now.AddSeconds(15);
        static DateTime nextSound = DateTime.Now.AddSeconds(5);
        static Random rand = new Random();

        static void Run()
        {
            const int MinX = 0;
            const int MaxX = 10;
            const int MinZ = 0;
            const int MaxZ = 10;
			
            while (true)
            {
                try
                {
                    Thread.Sleep(2000);
					
                    if (nextThunder < DateTime.Now)
                    {
                        nextThunder = DateTime.Now.AddSeconds(rand.Next(2, 30));
						
                        SpawnGlobalEntity tb = new SpawnGlobalEntity(new CoordDouble(rand.Next(MinX, MaxX), 0, rand.Next(MinZ, MaxZ)));
                        tb.EID = 0;
							
                        foreach (Client p in PlayerList.List)
                        {
                            if (p.Session.CurrentRegion == null)
                                continue;
                            if (p.Session.CurrentRegion.Type != Type)
                                continue;
                            if (p.Session.Mode == GameMode.Creative)
                                continue;
							
                            p.Queue.Queue(tb);								
                        }
                    }
					
                    if (nextSound < DateTime.Now)
                    {
                        nextSound = DateTime.Now.AddSeconds(rand.Next(30, 70));
						
                        Explosion ex = new Explosion(new CoordDouble(rand.Next(MinX, MaxX), 0, rand.Next(MinZ, MaxZ)));
                        ex.Radius = 3;
						
                        foreach (Client p in PlayerList.List)
                        {
                            WorldSession ws = p.Session;
                            if (ws == null)
                                continue;
                            if (ws.CurrentRegion == null)
                                continue;
                            if (ws.CurrentRegion.Type != Type)
                                continue;
                            if (ws.Mode == GameMode.Creative)
                                continue;
                            p.Queue.Queue(ex);
                        }
                    }
					
                }
                catch (Exception e)
                {
                    Log.WriteServer(e);
                }
            }
        }
		
        //Rain
        static readonly ChangeGameState rain = new ChangeGameState(GameState.BeginRaining);

        static public void Entering(Client player)
        {
            //Rain
            if (player.Session.Mode != GameMode.Creative)
                player.SendToClient(rain);
        }

        static internal void Leaving(Client player)
        {
            //Resume outside weather
            player.SendToClient(World.Main.Weather);
			
            player.TellAbove(Chat.Aqua, "Chat range: " + Parser.DistanceMax + " blocks");
        }

        static internal bool FilterClient(Client player, Packet packet)
        {
            if (player.Session.Mode == GameMode.Creative)
                return false;
			
            //Pick up lava is drawn into it
            PlayerBlockPlacement placement = packet as PlayerBlockPlacement;
            if (placement != null)
                return CursedLand(player, placement);

            PlayerDigging pd = packet as PlayerDigging;
            if (pd != null)
            if (DiggProtect(player, pd))
                return true;
						
            return false;
        }

        static bool DiggProtect(Client player, PlayerDigging pd)
        {
            if (pd.Status == PlayerDigging.StatusEnum.FinishedDigging)
            {
                BlockChange bc = new BlockChange(pd.Position.Offset(pd.Face), BlockID.Fire);
                player.SendToClient(bc);
				
                player.TellAbove(Chat.Purple, "You are still in spawn");
                return true;
            }
			
            return false;
        }

        static bool CursedLand(Client player, PlayerBlockPlacement placement)
        {
            SlotItem i = player.Session.ActiveItem;
            if (i != null && i.ItemID == BlockID.Bucket)
            {
                var pps = new PlayerPositionLookClient();
                pps.Position = placement.BlockPosition.CloneDouble();
                pps.OnGround = false;
                pps.Position.Y += 1.1;
                pps.Position.X += 0.5;
                pps.Position.Z += 0.5;
                pps.Pitch = Math.PI / 2;
                pps.Yaw = player.Session.Yaw;
                player.FromClient(pps);
				
                var ppc = new PlayerPositionLookServer(pps.Position);
                ppc.Pitch = Math.PI / 2;
                ppc.Yaw = player.Session.Yaw;
                player.SendToClient(ppc);
            }
			
            BlockChange bc = new BlockChange(placement.BlockPosition.Offset(placement.FaceDirection), BlockID.Air);
            player.SendToClient(bc);
			
            //Block all actions
            return true;
        }

        public static bool FilterServer(WorldSession player, Packet packet)
        {
            if (player.Mode == GameMode.Creative && player.Player.Admin(Permissions.AnyAdmin))
                return false;
			
            byte pid = packet.PacketID;

            //Never stop raining
            if (pid == ChangeGameState.ID)
            {
                ChangeGameState ns = packet as ChangeGameState;
                if (ns.Reason == GameState.EndRaining)
                    return true;
            }
			
            //Block Time
            if (pid == TimeUpdate.ID)
            {
                player.Player.SendToClient(new TimeUpdate(18000));
                return true;
            }
			
            return false;
        }
    }
}

