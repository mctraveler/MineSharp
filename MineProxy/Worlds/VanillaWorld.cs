using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using MineProxy.Chatting;
using System.Net;
using System.Threading;
using MineProxy.Packets;
using MineSharp.Wrapper;

namespace MineProxy.Worlds
{
    /// <summary>
    /// Interface to the backend vanilla server
    /// </summary>
    public class VanillaWorld : World
    {
        /// <summary>
        /// That we connect to in the back
        /// </summary>
        public readonly IPEndPoint Endpoint;
        /// <summary>
        /// Directory where the server is stored
        /// </summary>
        public readonly string ServerName;

        /// <summary>
        /// When true it won't automatically start itself
        /// </summary>
        public bool Suspended { get; set; }

        protected List<VanillaSession> players = new List<VanillaSession>();
        /// <summary>
        /// Keep a record of what mob has what id
        /// </summary>
        readonly Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        public VanillaSession[] ConPlayers = new VanillaSession[0];
        public ChangeGameState Weather = new ChangeGameState(GameState.EndRaining);
        public TimeUpdate Time = new TimeUpdate(0);

        public override string ToString()
        {
            var s = BackendManager.GetServer(ServerName);
            return string.Format("[" + ServerName + " " + (s == null ? "unloaded" : s.ToString()) + " " + Players.Length + " players]");
        }

        public VanillaWorld(string name)
        {
            this.ServerName = name;
            this.Endpoint = GetPortFromProperties(name);
            Commands = new VanillaWorldCommands();

            string regionPath = Path.Combine(name, "regions.json");
            this.Regions = RegionLoader.Load(regionPath);

            stopBackend = new Timer(StopBackend, null, -1, -1);
        }

        IPEndPoint GetPortFromProperties(string name)
        {
            //Get port from server.properties
            string key = "server-port=";
            string[] lines = File.ReadAllLines(Path.Combine(name, "server.properties"));
            foreach (string l in lines)
            {
                string t = l.Trim();
                if (t.StartsWith(key) == false)
                    continue;

                int port = int.Parse(t.Substring(key.Length));
                return new IPEndPoint(IPAddress.Loopback, port);
            }
            throw new InvalidDataException("Missing: server-port");
        }

        public override void Send(string commands)
        {
            var s = BackendManager.GetServer(ServerName);
            if (s == null)
                throw new MineProxy.Commands.ErrorException("Server not running");
            s.SendCommand(commands);
        }

        void KickSlot(Client player)
        {
            player.TellSystem(Chat.Aqua, "Sorry your slot was taken over");
            player.SetWorld(World.Wait);
            player.TellSystem(Chat.Gold, "See /donate for guaranteed slot");
        }

        public sealed override WorldSession Join(Client player)
        {	
            //reset scoreboard
            player.Score = null;

            if (Suspended)
            {
                player.TellSystem(Chat.Purple, "Vanilla is suspended by admin, it will be right back");
                return null;
            }

            StartBackend();

            lock (players)
            {
                int pc = 0;
                foreach (VanillaSession r in players)
                    if (r.Player.Settings.Cloaked == null)
                        pc++;
				
                if (pc >= MinecraftServer.MaxSlots && player.Settings.Cloaked == null)
                {
					
                    //Kick old player
                    //New visitors are given an advantage
                    if (player.Uptime.TotalMinutes < 15)
                    {
                        //Kick normal player
                        foreach (VanillaSession r in players)
                        {
                            if (r.Player.Settings.Cloaked != null)
                                continue;
                            if (Donors.IsDonor(r.Player))
                                continue;
                            if (r.Player.Uptime.TotalMinutes < 15)
                                continue;
                            KickSlot(r.Player);
                            World.Main.Say(Chat.Gray, r.Player.Name + " left its slot to " + player.Name + "(new)");
                            goto slotFree;
                        }
                    }
					
                    if (Donors.IsDonor(player.MinecraftUsername))
                    {
                        //Kick any non donor
                        foreach (VanillaSession r in players)
                        {
                            if (r.Player.Settings.Cloaked != null)
                                continue;
                            if (Donors.IsDonor(r.Player))
                                continue;
                            if (r.Player.Uptime.TotalMinutes < 15)
                                continue;
                            KickSlot(r.Player);
                            this.Say(Chat.Gray, r.Player.Name + " left its slot to donor " + player.Name);
                            goto slotFree;
                        }
                        foreach (VanillaSession r in players)
                        {
                            if (r.Player.Settings.Cloaked != null)
                                continue;
                            if (Donors.IsDonor(r.Player))
                                continue;
                            KickSlot(r.Player);
                            this.Say(Chat.Gray, r.Player.Name + " left its slot to donor " + player.Name);
                            goto slotFree;
                        }
                        goto slotFree;
                    }
					
                    //Block
                    Chat.ReadFile("full.txt", Chat.Aqua, player);
                    this.Say(Chat.DarkGreen, "No free slot for " + player.Name);
                    return null;
                }
				
                slotFree:
				
                var server = BackendManager.StartServer(ServerName);
                if(server.Running.WaitOne(10) == false)
                    Log.WriteServer("Timeout waiting for server to start");

                VanillaSession s = CreateSession(player);
                players.Add(s);
                base.Join(s);
#if DEBUG
                Console.WriteLine(s.Player.MinecraftUsername + " Joining Real: " + Players.Length);
#endif
                return s;
            }
			
        }

        protected virtual VanillaSession CreateSession(Client player)
        {
            return new VanillaSession(this, player);
        }

        public void Leave(VanillaSession session)
        {
            lock (players)
            {
                players.Remove(session);
                base.Leave(session);
#if DEBUG
                Console.WriteLine(session.Player.MinecraftUsername + " Leaving Real: " + Players.Length);
#endif

            }
            if (Players.Length < MinecraftServer.MaxSlots) // && session.Error == null)
                World.Wait.BringBack();


            if (Players.Length == 0)
            {
                //Shutdown after 5 minutes
                stopBackend.Change(5 * 60 * 1000, -1);
            }
        }

        readonly Timer stopBackend;

        void StopBackend(object o)
        {
            if (Players.Length > 0)
                return;

            StopBackend();
        }

        public void StartBackend()
        {
            stopBackend.Change(-1, -1);

            if (Suspended)
                return;
            BackendManager.StartServer(ServerName);
        }

        public void StopBackend()
        {
            BackendManager.StopServer(ServerName);
        }

        #region Entity Mirror

        public VanillaSession GetPlayer(int eID)
        {
            foreach (VanillaSession s in Players)
            {
                if (s.EID == eID)
                    return s;
            }
            return null;
        }

        public void UpdateEntity(SpawnMob spawn)
        {
            Mob m = null;
            lock (entities)
            {
                if (entities.ContainsKey(spawn.EID))
                    m = entities[spawn.EID] as Mob;
                if (m == null)
                {
                    m = new Mob(spawn.EID, spawn.Type);
                    entities[spawn.EID] = m;
                }
            }
            m.Update(spawn);
        }

        public void UpdateEntity(SpawnPlayer spawn)
        {
            Player m = null;
            lock (entities)
            {
                if (entities.ContainsKey(spawn.EID))
                    m = entities[spawn.EID] as Player;
                if (m == null)
                {
                    m = new Player(spawn.EID, spawn.PlayerUUID);
                    entities[spawn.EID] = m;
                }
            }
            m.Update(spawn);
        }

        public void UpdateEntity(SpawnObject spawn)
        {
            Vehicle m = null;
            lock (entities)
            {
                if (entities.ContainsKey(spawn.EID))
                    m = entities[spawn.EID] as Vehicle;
                if (m == null)
                {
                    m = new Vehicle(spawn.EID, spawn.Type);
                    entities[spawn.EID] = m;
                }
            }
            m.Update(spawn);
        }

        public void UpdateEntity(SpawnPainting spawn)
        {
            Vehicle m = null;
            lock (entities)
            {
                if (entities.ContainsKey(spawn.EID))
                    m = entities[spawn.EID] as Vehicle;
                if (m == null)
                {
                    m = new Vehicle(spawn.EID, Vehicles.Frame);
                    entities[spawn.EID] = m;
                }
            }
        }

        public void UpdateEntity(EntityMetadata meta)
        {   
            lock (entities)
            {
                if (entities.ContainsKey(meta.EID) == false)
                    return;

                entities[meta.EID].Update(meta);
            }
        }

        public Entity GetEntity(int eID)
        {
            lock (entities)
            {
                if (entities.ContainsKey(eID))
                    return entities[eID];
                else
                    return null;
            }
        }

        #endregion

    }
}

