using System;
using MineProxy.Chatting;
using System.Collections.Generic;
using MineProxy.Commands;
using MineProxy.Worlds;
using MineProxy.Packets;

namespace MineProxy
{
    /// <summary>
    /// Allow admins to remain invisible to other players
    /// </summary>
    public static class Cloak
    {
        public static void Init(CommandManager c)
        {
            c.AddAdminCommand(Cloak.CloakCommand, "cloak");
        }

        static readonly Dictionary<string, CoordDouble> cloakBack = new Dictionary<string, CoordDouble>();

        static void CloakCommand(Client player, string[] cmd, int iarg)
        {
            if (player.Admin(Permissions.Cloak) == false)
                return;

            if (cmd.Length < 2)
            {
                TellMode(player);
                return;
            }
	
            switch (cmd [iarg])
            {
                case "clear":
                case "reset":
                case "normal":
                case "off":
                    SetCloak(player, null);
                    break;
                case "none":
                case "invisible":
                case "on":
                    SetCloak(player, MobType.None.ToString());
                    break;
            /*
			case "Villager":
			case "Snowman":
			case "EnderDragon":
				player.Tell (Chat.Red, cmd [1] + " is disabled/not working");
				return;
				*/
                case "back":
                    if (cloakBack.ContainsKey(player.MinecraftUsername) == false)
                        throw new ErrorException("No saved position, use /cloak first");
                    player.Warp(cloakBack [player.MinecraftUsername], player.Session.Dimension, World.Main);
                    break;

                default:
                    SetCloak(player, cmd [iarg]);
                    break;
            }
        }
		
        static void Decloak(Client player, string[] cmd, int iarg)
        {
            SetCloak(player, null);
        }

        public static void SetCloak(Client player, string type)
        {
            if (type == null)
                player.Settings.Cloaked = null;
            else
            {
                try
                {
                    MobType mt = (MobType)Enum.Parse(typeof(MobType), type);
                    player.Settings.Cloaked = mt.ToString();

                    cloakBack.Remove(player.MinecraftUsername);
                    cloakBack.Add(player.MinecraftUsername, player.Session.Position);
                } catch (Exception)
                {
                    player.TellSystem(Chat.DarkRed, "Unknown mob: " + type);
                }
            }
            player.SaveProxyPlayer();
		
            VanillaSession rs = player.Session as VanillaSession;
            if (rs != null)
            {
                if (player.Settings.Cloaked == null)
                {
                    SpawnPlayer spawnNamedEntity = new SpawnPlayer(rs.EID, player);
                    spawnNamedEntity.Position = rs.Position;
                    spawnNamedEntity.Pitch = rs.Pitch;
                    spawnNamedEntity.Yaw = rs.Yaw;
                    PlayerList.QueueToAll(spawnNamedEntity);
                    player.Queue.Queue(new DestroyEntities(rs.EID));

                    player.Session.World.Send("gamemode 0 " + player.MinecraftUsername);

                } else
                {
                    PlayerList.QueueToAll(new DestroyEntities(rs.EID));

                    player.Session.World.Send("gamemode 1 " + player.MinecraftUsername);
                }
            }

            TellMode(player);

            PlayerList.UpdateTabPlayers();
        }
		
        static void TellMode(Client player)
        {
            if (player.Settings.Cloaked != null)
                player.TellSystem(Chat.Purple, "You are a " + Chat.DarkAqua + player.Settings.Cloaked);
            else
                player.TellSystem(Chat.Purple, "You are yourself");
        }
		
        public static bool Filter(VanillaSession real, Packet p)
        {
            Client player = real.Player;
            IEntity e = p as IEntity;
            if (e == null)
                return false;
			
            //Pass all own actions normally
            if (e.EID == player.EntityID)
                return false;
			
            Client namedPlayer = null;
            VanillaSession r = World.Main.GetPlayer(e.EID); 
            if (r != null)
                namedPlayer = r.Player;
		
            byte pid = p.PacketID;

            //New named entity
            if (pid == SpawnPlayer.ID)
            {
                SpawnPlayer spawnNamedEntity = (SpawnPlayer)p;
			
                //Search by vanilla uuid since we ahv enot yet modified the packet
                if (namedPlayer == null)
                    namedPlayer = PlayerList.GetPlayerByVanillaUUID(spawnNamedEntity.PlayerUUID);
                if (namedPlayer == null)
                {
                    Debug.WriteLine("NamedEntity not found in player list: " + spawnNamedEntity.PlayerUUID);
                    return false; //Still let through
                }
                				
                //pass any none cloaked player
                if (namedPlayer.Settings.Cloaked == null)
                    return false;
				
                //Invisible mode, hide everything about entity
                if (namedPlayer.Settings.Cloaked == MobType.None.ToString())
                    return true;
				
                if (namedPlayer.Settings.Cloaked == null)
                {
                    //Debug.WriteLine ("Spawning player " + ne.PlayerName + " as nick " + np.Name);
                    return false;
                }
                //Debug.WriteLine ("Spawning player " + ne.PlayerName + " cloaked as \"" + np.Cloaked + "\" nick: " + np.Name);
				
                MobType mt;
                try
                {
                    mt = (MobType)Enum.Parse(typeof(MobType), namedPlayer.Settings.Cloaked);
                } catch (Exception)
                {
                    Debug.WriteLine("Unknown cloak for " + namedPlayer.MinecraftUsername + ": " + namedPlayer.Settings.Cloaked);
                    return false;
                }
				
                SpawnMob ms = new SpawnMob(mt);
                ms.EID = spawnNamedEntity.EID;
                ms.Pos = spawnNamedEntity.Position;
                ms.Pitch = spawnNamedEntity.Pitch;
                ms.Yaw = spawnNamedEntity.Yaw;
				
                //Add metadata to specific mobs - some already set in constructor

                if (ms.Type == MobType.MagmaCube || ms.Type == MobType.Slime)
                    ms.Metadata.SetByte(16, 1);
                if (ms.Type == MobType.Ghast)
                    ms.Metadata.SetByte(16, 0);
                if (ms.Type == MobType.Enderman)
                {
                    ms.Metadata.SetByte(16, (sbyte)BlockID.Dirt);
                    ms.Metadata.SetByte(17, 0);
                }
                if (ms.Type == MobType.Blaze)
                    ms.Metadata.SetByte(16, 0);
                if (ms.Type == MobType.Sheep)
                    ms.Metadata.SetByte(16, 0);
                if (ms.Type == MobType.Spider)
                    ms.Metadata.SetByte(16, 0);
				
                player.Queue.Queue(ms);
                return true;
            }
			
            if (pid == DestroyEntities.ID)
                return false;
			
            //Not in list
            if (namedPlayer == null)
                return false;
			
            //Not cloaked
            if (namedPlayer.Settings.Cloaked == null)
                return false;
			
            //Invisible mode, hide everything about entity
            if (namedPlayer.Settings.Cloaked == MobType.None.ToString())
                return true;
			
            //Else any mob
            if (pid == Animation.ID)
                return true;
			
            if (pid == EntityEquipment.ID)
                return true;
			
            return false;
        }

    }
}

