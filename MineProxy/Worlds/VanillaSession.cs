
using System;
using MiscUtil.IO;
using System.IO;
using System.Threading;
using System.Net;
using MineProxy;
using System.Net.Sockets;
using MiscUtil.Conversion;
using System.Collections.Generic;
using MineProxy.Data;
using MineProxy.Plugins;
using MineProxy.Network;
using MineProxy.Misc;
using MineProxy.Chatting;
using MineProxy.Packets;
using System.Text;

namespace MineProxy.Worlds
{
    public partial class VanillaSession : WorldSession
    {
        public override string ShortWorldName { get { return "Vanilla: " + Vanilla.ServerName; } }
        //public DateTime AfkTime = DateTime.Now;
        //public CoordDouble AfkPos = new CoordDouble();

        public readonly OreTracker OreTracker;

        public class OpenWindowRegion
        {
            public WindowOpen Window { get; set; }

            public WorldRegion Region { get; set; }

            public OpenWindowRegion(WindowOpen window, WorldRegion region)
            {
                this.Window = window;
                this.Region = region;
            }
        }

        /// <summary>
        /// The open window or null if no window is open
        /// </summary>
        public readonly Dictionary<int,OpenWindowRegion> OpenWindows = new Dictionary<int, OpenWindowRegion>();

        /// <summary>
        /// Last region player clicked into, identify the region of the open window
        /// </summary>
        public WorldRegion LastClickRegion { get; set; }

        public readonly List<CoordInt> Spawners = new List<CoordInt>();
        readonly Threads thread;
        public readonly VanillaWorld Vanilla;

        /// <summary>
        /// -1 until set
        /// </summary>
        public int maxUncompressed = -1;

        public VanillaSession(VanillaWorld world, Client player)
            : base(player)
        {
            this.World = world;
            this.Vanilla = world;

            OreTracker = new OreTracker(player);

            if (player.MinecraftUsername == null)
                throw new ArgumentException("Player must be logged in, missing minecraftusername");

            thread = Threads.Create(this, RunServerReader, WatchdogKilled);
            thread.User = Player.MinecraftUsername;
            thread.Start();
        }

        void WatchdogKilled()
        {
            Close("Watchdog killed");
        }

        /// <summary>
        /// Entity ID assigned by the backend server
        /// </summary>
        public int EID { get; private set; }

        /// <summary>
        /// Backend UUID, offline type
        /// </summary>
        public Guid UUID { get; set; }

        //public short Health { get; set; }

        public Attacked Attacker { get; set; }

        #region ChunkControl

        public bool ChunkFirst = true;
        public Dimensions ChunkLastDimension = Dimensions.Overworld;
        public int ChunkLastX = 0;
        public int ChunkLastZ = 0;
        public List<string> ChunkLoadedByServer = new List<string>();

        #endregion

        static object serverConnectLock = new object();
        Phases phase = Phases.Handshake;
        object serverWriterLock = new object();
        Socket socket;
        Stream serverStream;

        void RunServerReader()
        {
            try
            {
                thread.State = "Connecting";
                ConnectToServer();
        
                //Handshake
                thread.State = "Handshake";
                RunServerHandshake();
            
                if (phase == Phases.FinalClose)
                    return;

                thread.State = "Handshake done";
            
                //Logged in!
                thread.State = "Fully logged in";
                ReceiverRunServer();
            }
            catch (SocketException se)
            {
                thread.State = "SE: " + se.Message;
                Log.Write(se, Player);
            }
            catch (IOException ioe)
            {
                thread.State = "IOE: " + ioe.Message;
                //Log.Write(ioe, Player);
            }
            catch (ObjectDisposedException ode)
            {
                thread.State = "ODE: " + ode.Message;
                //Log.Write(ode, Player);
#if !DEBUG
            } catch (Exception e)
            {
                thread.State = "Exception: " + e.Message;
                Log.Write(e, Player);
#endif
            }
            finally
            {
                thread.State = "Finally";
                phase = Phases.FinalClose;
                if (socket != null)
                    socket.Close();
            }
        }

        /// <summary>
        /// Throw exception on error
        /// </summary>
        void RunServerHandshake()
        {
            thread.WatchdogTick = DateTime.Now;

            //Handshaker
            SendToBackend(new Handshake(MinecraftServer.IP.ToString(), Vanilla.Endpoint.Port, HandshakeState.Login));
            SendToBackend(new LoginStart(Player.MinecraftUsername));

            thread.WatchdogTick = DateTime.Now;

            //No encryption with localhost?!!! :D
            /*
            //Read Encryption Request
            var encReq = new EncryptionRequest(PacketReader.Read(serverReader, EncryptionRequest.ID));
            Debug.FromServer(encReq, Player);

            //Send response
            if (encReq.ServerID != "")
                throw new InvalidOperationException("Backend server online mode must be false, ID=-, got " + encReq.ServerID);
            crypto = new CryptoMC(serverStream, encReq);
            SendToBackend(new EncryptionResponse(crypto));

            //Start encryption
            serverStream = crypto;
            serverReader = new EndianBinaryReader(EndianBitConverter.Big, serverStream);
            serverWriter = new EndianBinaryWriter(EndianBitConverter.Big, serverStream);
            */

            byte[] buffer = PacketReader.ReadHandshake(serverStream);
            if (buffer[0] == 0)
            {
                string disconnectMessage = Encoding.ASCII.GetString(buffer, 2, buffer.Length - 2);
                Player.TellSystem("[Error]", disconnectMessage);
                throw new InvalidOperationException(disconnectMessage);
            }

            var compression = new SetCompression(buffer);
            maxUncompressed = compression.MaxSize;
            Debug.FromServer(compression, Player);

            var loginSuccess = new LoginSuccess(PacketReader.Read(serverStream));
            UUID = loginSuccess.UUID;
            Debug.FromServer(loginSuccess, Player);
        
            //Read LoginRequest
            JoinGame res = new JoinGame(PacketReader.Read(serverStream));
            Debug.FromServer(res, Player);

            thread.State = "Got Login";
            EID = res.EntityID;
            Mode = (GameMode)res.GameMode;
            Dimension = res.Dimension;

            //Store UUID permanently - No need to store from backend since that is offline uuid

            phase = Phases.Gaming;
        }
        #if DEBUG
        Packet prev1 = null;
        Packet prev2 = null;
        Packet prev3 = null;
        Packet prev4 = null;
        Packet prev5 = null;
        Packet prev6 = null;
        Packet prev7 = null;
        Packet prev8 = null;
        #endif

        void ReceiverRunServer()
        {
            PacketFromServer p = null;
            try
            {
                thread.State = "Active loop";
                while (phase == Phases.Gaming)
                {
                    try
                    {
#if DEBUG
                        prev8 = prev7;
                        prev7 = prev6;
                        prev6 = prev5;
                        prev5 = prev4;
                        prev4 = prev3;
                        prev3 = prev2;
                        prev2 = prev1;
                        prev1 = p;
#endif
                        //thread.State = "Active loop, prev: " + p;
                        p = PacketFromServer.ReadServer(serverStream);
                        thread.WatchdogTick = DateTime.Now;

                        Debug.FromServer(p, Player);
                        //thread.State = "Got Packet " + p;
                    }
                    catch (EndOfStreamException)
                    {
                        phase = Phases.FinalClose;
                        return;
                    }
                    catch (IOException)
                    {
                        phase = Phases.FinalClose;
                        return;
                    }
                    catch (ObjectDisposedException)
                    {
                        phase = Phases.FinalClose;
                        return;
                    }
                    catch (Exception e)
                    {
                        if (phase != Phases.FinalClose)
                        {
#if DEBUG
                            Log.Write(new PrevException(prev8), Player);
                            Log.Write(new PrevException(prev7), Player);
                            Log.Write(new PrevException(prev6), Player);
                            Log.Write(new PrevException(prev5), Player);
                            Log.Write(new PrevException(prev4), Player);
                            Log.Write(new PrevException(prev3), Player);
                            Log.Write(new PrevException(prev2), Player);
                            Log.Write(new PrevException(prev1), Player);
#endif
                            Log.Write(e, Player);
                            phase = Phases.FinalClose;
                        }
                        return;
                    }
                
                    if (phase == Phases.FinalClose)
                        break;
            
                    try
                    {
                        FromServerGaming(p);
                    }
                    catch (Exception e)
                    {
                        Log.Write(e, Player);
                        return;
                    }
                }
            }
            finally
            {
                thread.State = "Loop ended, closing";
                    
#if DEBUG
                Console.WriteLine("ServerReceiver Ended: " + Player);
#endif
                serverStream.Close();
                thread.State = "Stream closed";                 
            }
        }

        /// <summary>
        /// What the Server sends to the client
        /// </summary>
        protected virtual void FromServerGaming(PacketFromServer packet)
        {
            try
            {
                byte pid = packet.PacketID;

                if (pid == PassThrough.ID)
                {
                    Player.SendToClient(packet);
                    return;
                }

                //Before we rewrite the eid
                //Record all presented mobs, so far we have no method of cleaning this up so hold your thumbs
                if (pid == SpawnMob.ID)
                    World.Main.UpdateEntity((SpawnMob)packet);              
                if (pid == EntityMetadata.ID)
                    World.Main.UpdateEntity((EntityMetadata)packet);
                //if (pid == SpawnPlayer.ID)
                //World.Main.UpdateEntity((SpawnPlayer)packet);
                if (pid == SpawnObject.ID)
                    World.Main.UpdateEntity((SpawnObject)packet);
                if (pid == SpawnPainting.ID)
                    World.Main.UpdateEntity((SpawnPainting)packet);

                //Rewrite own EID, Fix eid to client eid player
                if (packet is IEntity)
                {
                    IEntity ie = (IEntity)packet;
                    if (ie.EID == EID)
                    {
                        ie.EID = Player.EntityID;
                        packet.SetPacketBuffer(null);
                    }
                }
                if (pid == SpawnObject.ID)
                {
                    var so = (SpawnObject)packet;
                    if (so.SourceEntity == EID)
                    {
                        so.SourceEntity = Player.EntityID;
                        packet.SetPacketBuffer(null);
                    }
                }

                //Region filters
                if (CurrentRegion != null && CurrentRegion.FilterServer(this, packet))
                    return;
                
                //Cloak and Nick
                if (Cloak.Filter(this, packet))
                    return;

                switch (pid)
                {
                    case SpawnPlayer.ID:
                        //Change reported uuid from offline to the known with names
                        var sp = (SpawnPlayer)packet;
                        var p = PlayerList.GetPlayerByVanillaUUID(sp.PlayerUUID);
                        if (p != null)
                        {
                            Debug.WriteLine("SpawnPlayer changed from " + sp.PlayerUUID);
                            Debug.WriteLine("SpawnPlayer changed to   " + p.UUID);
                            sp.PlayerUUID = p.UUID;
                            sp.SetPacketBuffer(null);
                        }
                        else
                            Debug.WriteLine("SpawnPlayer not changed: " + sp.PlayerUUID);
                        break;

                    case ChangeGameState.ID:
                        ChangeGameState ns = packet as ChangeGameState;
                        if (ns.Reason == GameState.BeginRaining || ns.Reason == GameState.EndRaining)
                            World.Main.Weather = ns;
                        if (ns.Reason == GameState.ChangeGameMode)
                            Mode = (GameMode)ns.Value;
                        break;

                    case SpawnObject.ID:
                        var so = (SpawnObject)packet;
                        if (so.Type != Vehicles.Item)
                            break;
                        OreTracker.Spawn(so, Position);
                        break;
                    case EntityMetadata.ID:
                        OreTracker.Track((EntityMetadata)packet);
                        break;

                    case EntityEffect.ID:
                        EntityEffect ee = (EntityEffect)packet;
                        if (ee.Effect == PlayerEffects.MoveSpeed)
                        {
                            EffectSpeed.Active = true;
                            EffectSpeed.Amplifier = ee.Amplifier;
                        }
                        if (ee.Effect == PlayerEffects.MoveSlowdown)
                        {
                            EffectSlow.Active = true;
                            EffectSlow.Amplifier = ee.Amplifier;
                        }
                        break;

                    case RemoveEntityEffect.ID:
                        RemoveEntityEffect re = (RemoveEntityEffect)packet;
                        if (re.Effect == PlayerEffects.MoveSpeed)
                            EffectSpeed.Active = false;
                        if (re.Effect == PlayerEffects.MoveSlowdown)
                            EffectSlow.Active = false;
                        break;
                
                    case TimeUpdate.ID:
                        World.Main.Time = packet as TimeUpdate;
                        break;

                    case UseBed.ID:
                        UseBed ub = (UseBed)packet;
                        if (ub.EID == Player.EntityID)
                            Sleeping = true;
                        break;

                //Waking up, among other things
                    case Animation.ID:
                        Animation a = (Animation)packet;
                        if (a.EID == Player.EntityID)
                        {
                            if (a.Animate == Animations.LeaveBed)
                                Sleeping = false;
                        }
                        break;
                                
#region Position and speed
                
                //Regions and player position
                    case PlayerPositionLookServer.ID:
                        var pp = (PlayerPositionLookServer)packet;
                        if (pp.Position.Y % 1 > 0.95)
                        {
                            //Console.WriteLine(pp.Position.ToString("0.00") + " " + pp.HeadY);
                            pp.Position.Y = Math.Ceiling(Position.Y);
                            //pp.HeadPosition = Position.Y + 1.62;
                            //Console.WriteLine(pp.Position.ToString("0.00") + " " + pp.Stance);
                            packet.SetPacketBuffer(null);
                        }
                        SetPosition(pp.Position, false);
                        break;

                    case Respawn.ID:
                        Dimension = ((Respawn)packet).Dimension;
                        SetPosition(Position, false);
                        break;                

                //Boats and carts
                    case AttachEntity.ID:
                        AttachEntity ae = (AttachEntity)packet;
                        if (ae.EID == Player.EntityID)
                        {
                            AttachedEntity = ae.VehicleID;
                        }
                        break;
                
                    case UpdateBlockEntity.ID:
                        if (Player.Admin())
                        {
                            var ute = (UpdateBlockEntity)packet;
                            if (ute.Action == UpdateBlockEntity.Actions.MobSpawner)
                            {
                                if (Spawners.Contains(ute.Pos) == false)
                                    Spawners.Add(ute.Pos);
                            }
                        }
                        break;

                    case EntityTeleport.ID:
                        if (AttachedEntity != 0)
                        {
                            EntityTeleport et = (EntityTeleport)packet;
                            if (et.EID == AttachedEntity)
                            {
                                SetPosition(et.Position, false);
                            }
                        }
                        break;

                    case EntityRelativeMove.ID:
                        if (AttachedEntity != 0)
                        {
                            EntityRelativeMove er = (EntityRelativeMove)packet;
                            if (er.EID == AttachedEntity)
                            {
                                SetPosition(Position + er.Delta, false);
                            }
                        }
                        break;                
#endregion

                    case ChatMessageServer.ID:
                        if (ServerParser.ParseServer(this.Player, (ChatMessageServer)packet))
                            return;
                        break;
                
                //Inventory
                    case WindowItems.ID:
                        var wi = (WindowItems)packet;
                        for (int n = 0; n < wi.Items.Length; n++)
                        {
                            if (wi.Items[n] == null)
                                continue;
                            if (wi.Items[n].Count > 64)
                            {
                                wi.Items[n] = null;
                                packet.SetPacketBuffer(null);
                            }
                        }
                        break;

                    case SetSlot.ID:
                        //Debug.WriteLine(packet);
                        SetSlot ss = (SetSlot)packet;

                        if (0 <= ss.Slot && ss.Slot < Inventory.Length)
                            Inventory[ss.Slot] = ss.Item;
                        break;

                //Tab player list items: Block Player, managed in PlayerList thread
                    case PlayerListItem.ID:
                        return;

                    case HeldItemServer.ID:
                //Active item
                        var hc = (HeldItemServer)packet;
                        if (hc.SlotID >= 0 && hc.SlotID <= 8)
                            ActiveInventoryIndex = hc.SlotID;
                        else
                            Log.Write(
                                new InvalidOperationException("Invalid holding slot id: " + hc.SlotID),
                                this.Player
                            );
                        break;

                //Keep a record of what window is open
                    case WindowOpen.ID:
                        WorldRegion r = LastClickRegion ?? CurrentRegion;
                        var wo = (WindowOpen)packet;
                        if (OpenWindows.ContainsKey(wo.WindowID))
                            OpenWindows.Remove(wo.WindowID);
                        OpenWindows.Add(wo.WindowID, new OpenWindowRegion(wo, r));

                        if (r == null || (Player.Admin() == false) && (Mode == GameMode.Creative))
                        {
                            //Leave unmodified
                        }
                        else
                        {
                            if (r.Type == Protected.Type &&
                                r.IsResident(Player) == false &&
                                wo.InventoryType != "CraftingTable" &&
                                wo.InventoryType != "Enchant" &&
                                wo.WindowTitle != "container.enderchest")
                            {
                                var cj = new ChatJson();
                                cj.Text = "Locked: " + r.Name;
                                wo.WindowTitle = cj.Serialize();
                                wo.SetPacketBuffer(null);
                            }
                        }
                        break;
                    
                //Turn off pvp when you die
                    case UpdateHealth.ID:
                        UpdateHealth uh = (UpdateHealth)packet;
                        if (Player.PvP && uh.Health <= 0)
                        {
                            Player.PvP = false;
                            Player.TellSystem(Chat.Purple, "PvP off");
                        }
                        break;
                
                
                    case WindowCloseServer.ID:
                        WindowClose((WindowCloseServer)packet);
                        break;

                //Disconnect while running the game
                //For disconnect at login see the handshake part
                    case Disconnect.ID:
                        Disconnect d = (Disconnect)packet;
                        if (d.Reason.Text == "Flying is not enabled on this server")
                        {
                            #if !DEBUG
                            Player.BanByServer(DateTime.Now.AddMinutes(2), d.Reason.Text);
                            #else
                            Chatting.Parser.Say(Chat.Yellow + Player.Name + Chat.Blue, " was kicked for flying");
                            #endif
                            return;
                        }
                        if (d.Reason.Text == "Internal server error")
                        {
                            Log.WritePlayer(
                                this.Player,
                                "Backend: Internal error - Restarting connection"
                            );
                            Player.SetWorld(World.Main);
                            return;
                        }
                        Player.SetWorld(World.Construct);
                        return;
                }
                //////////////Sending packet
                Player.SendToClient(packet);
                //////////////
                
#if !DEBUG
            } catch (Exception e)
            {
                thread.State = "ServerGamingException: " + e.Message;
                    
                Log.Write(e, this.Player);
                Player.Queue.Queue(packet);
#endif
            }
            finally
            {
            }
        }

        void WindowClose(WindowCloseClient wc)
        {
            if (OpenWindows.ContainsKey(wc.WindowID))
                OpenWindows.Remove(wc.WindowID);
        }

        void WindowClose(WindowCloseServer wc)
        {
            if (OpenWindows.ContainsKey(wc.WindowID))
                OpenWindows.Remove(wc.WindowID);
        }

        protected override void SetPosition(CoordDouble pos, bool speedguard)
        {
            base.SetPosition(pos, speedguard);

            //AFK detection
            /*if (AfkPos.DistanceTo(pos) > 1)
            {
                AfkTime = DateTime.Now;
                AfkPos = pos;
            }
            if (DateTime.Now - AfkTime > AfkWorld.Timeout)
            {
                //Dont go afk world automatically, since it could break book writing
                //Player.SetWorld(World.AFK);
            }*/

            if (Dimension == Dimensions.Nether && pos.Y > 127 && this.Mode != GameMode.Creative)
            {
                Player.Warp(new CoordDouble(16, 200, -16), Dimensions.Overworld, World.Main);
                return;
            }
        }

        /// <summary>
        /// What the Client sends to the server.
        /// </summary>
        public override void FromClient(PacketFromClient packet)
        {
            while (phase == Phases.Handshake)
                Thread.Sleep(100);
            //Wait for handshake to complete
            if (phase == Phases.FinalClose)
                throw new SessionClosedException();
    
            if (packet.PacketID == PassThrough.ID)
            {
                SendToBackend(packet);
                return;
            }

            //Fix players eid to what is known for the server
            if (packet is IEntity)
            {
                IEntity ie = (IEntity)packet;
                if (ie.EID == Player.EntityID)
                {
                    ie.EID = EID;
                    packet.SetPacketBuffer(null);
                }
            }

            WorldRegion region = CurrentRegion;

            switch (packet.PacketID)
            {
                case PlayerPosition.ID:
                    if (AttachedEntity > 0)
                    {
                        //Ignore relative movements
                    }
                    else
                    {
                        var pp = ((PlayerPosition)packet);
                        if (pp.Position.Y > -900)
                        {
                            SetPosition(pp.Position, true);
                            OnGround = pp.OnGround != 0;
                        }
                    }
                    break;

                case PlayerPositionLookClient.ID:
                    if (AttachedEntity > 0)
                    {
                        //Ignore relative movements
                    }
                    else
                    {
                        PlayerPositionLookClient pp = ((PlayerPositionLookClient)packet);
                        if (pp.Position.Y > -900)
                        {
                            SetPosition(pp.Position, true);
                            OnGround = pp.OnGround;
                        }
                    }
                    Pitch = ((PlayerPositionLookClient)packet).Pitch;
                    Yaw = ((PlayerPositionLookClient)packet).Yaw;
                    break;

                case PlayerGround.ID:
                    OnGround = ((PlayerGround)packet).OnGround;
                    //Good but a few false positives
                    /*
                    if (Sprinting)//Hacked client: Invalid sprint
                    {
                        Chatting.Parser.TellAdmin(Player.Name + Chat.Gray + " sprinting standing still");
                    }*/
                    break;

                case  EntityAction.ID:
                    EntityAction ea = packet as EntityAction;
                    switch (ea.Action)
                    {
                        case EntityAction.Actions.LeaveBed:
                            Sleeping = false;
                            break;
                        case EntityAction.Actions.Crounch:
                            Crouched = true;
                            break;
                        case EntityAction.Actions.Uncrounch:
                            Crouched = false;
                            break;
                        case EntityAction.Actions.StartSprinting:
                            Sprinting = true;
                            break;
                        case EntityAction.Actions.StopSprinting:
                            Sprinting = false;
                            break;
                    }
                    break;

                case UseEntity.ID:
                    if (Mode == GameMode.Creative && (Player.Admin() == false))
                        return; //Donors can't hurt while in creative mode

                    if (UseEntityFromClient((UseEntity)packet))
                        return;
                    break;
                
                case HeldItemClient.ID:
                    //Active item
                    var hc = (HeldItemClient)packet;
                    if (hc.SlotID >= 0 && hc.SlotID <= 8)
                        ActiveInventoryIndex = hc.SlotID;
                    else
                        Log.Write(
                            new InvalidOperationException("Invalid holding slot id: " + hc.SlotID),
                            this.Player
                        );
                    break;
                        
            //Prevent non admins from getting items in creative
                case CreativeInventory.ID:
                    if (Player.Admin() == false)
                    {
                        Player.SendToClient(new EntityStatus(Player.EntityID, EntityStatuses.EntityHurt));
                        Player.TellSystem(Chat.Yellow, "Creative Inventory Disabled");
                        return;
                    }

                    CreativeInventory ci = (CreativeInventory)packet;
                    if (0 <= ci.Slot && ci.Slot <= Inventory.Length)
                        Inventory[ci.Slot] = ci.Item;
                    break;

            //If block action is done from another region
                case PlayerBlockPlacement.ID:
                    //AfkTime = DateTime.Now;

                    //Non admins can't place block
                    if (Mode == GameMode.Creative && (Player.Admin() == false))
                    {
                        Player.SendToClient(new EntityStatus(Player.EntityID, EntityStatuses.EntityHurt));
                        Player.TellSystem(Chat.Yellow, "Creative Build Disabled");
                        return;
                    }

                    PlayerBlockPlacement pb = (PlayerBlockPlacement)packet;
                    if (pb.BlockPosition.IsNull() == false)
                    {
                        CoordDouble pos = pb.BlockPosition.CloneDouble();
                        region = RegionCrossing.GetRegion(pos, Dimension, World.Regions);
                        //Remember the last position clicked so we can do better chest protection
                        LastClickRegion = region; //this is obsolete since we moved to blocking open across regions
                    }
                    else
                    {
                        if (pb.Item != null)
                            Charge = new ChargeState(pb.Item.ItemID);
                    }

                    if (FilterDirection(pb.BlockPosition))
                        return;

                    if (region == null)
                    {
                        if (Dimension == 0 && FilterLava(pb))
                            return;
                    }
                    if (Protected.ProtectBlockPlace(this, region, pb))
                        return;

                    break;
            
                case PlayerDigging.ID:
                    //AfkTime = DateTime.Now;
                    PlayerDigging pd = (PlayerDigging)packet;
                    if (pd.Status == PlayerDigging.StatusEnum.FinishedDigging || pd.Status == PlayerDigging.StatusEnum.StartedDigging)
                    {
                        CoordDouble pos = pd.Position.CloneDouble();
                        region = RegionCrossing.GetRegion(pos, Dimension, World.Regions);
                    }
                    //Log breaking blocks to determine if they found the diamond
                    if (pd.Status == PlayerDigging.StatusEnum.FinishedDigging)
                        OreTracker.BrokeBlock = DateTime.Now;

                    if (pd.Status == PlayerDigging.StatusEnum.ShootArrow)
                        Charge = null;

                    //Prevent non admin creative from digging
                    if (Mode == GameMode.Creative && Player.Admin() == false)
                        return;
                    if (FilterDirection(pd.Position))
                        return;

                    if (Protected.ProtectBlockBreak(this, region, pd))
                        return;
                    break;
            
                case WindowClick.ID:
                    var wc = packet as WindowClick;
                    if (wc.WindowID == 0)
                    {
                        //TODO: handle
                    }
                    //AfkTime = DateTime.Now;
                    if (Protected.ProtectChestsClick(this, wc))
                        return;

                    ///Workaround bug clicky items duplication
                    if (wc.Item != null)
                    {
                        if (wc.Item.Count > 64)
                            return;
                    }
                    break;

                case WindowCloseClient.ID:
                    WindowClose((WindowCloseClient)packet);
                    break;
            }

            if (region != null)
            {
                if (region.Type == SpawnRegion.Type)
                if (SpawnRegion.FilterClient(Player, packet))
                    return;
                if (region.Type == SpawnTimeRegion.Type)
                if (SpawnTimeRegion.FilterClient(region, Player, packet))
                    return;
            }

            SendToBackend(packet);
        }

        bool FilterLava(PlayerBlockPlacement placement)
        {
            if (Player.Admin())
                return false;

            //Log lava
            SlotItem i = ActiveItem;
            //Prevent fire, lava and TNT
            if (i == null)
                return false;

                
            if ((i.ItemID == BlockID.Lava || i.ItemID == BlockID.Lavabucket)
                && CurrentRegion == null
                && (placement.BlockPosition.IsNull() == false)
                && (placement.BlockPosition.Y > 80))
            {
                string msg = "High lava at " + placement.BlockPosition;
                Log.WritePlayer(this, msg);
                Chatting.Parser.TellAdmin(Chat.Red + Player.MinecraftUsername + " " + msg);
                return true;
            }
            
            if (i.ItemID == BlockID.FlintandSteel
                || i.ItemID == BlockID.Fireball
                || i.ItemID == BlockID.Lava
                || i.ItemID == BlockID.TNT
                || i.ItemID == BlockID.Lavabucket)
            {
                if (placement.BlockPosition.IsNull() == false)
                {
                    if (Player.Uptime < TimeSpan.FromHours(48))
                    {
                        //Prevent youngsters to place lava in wilderness
                        return true;
                    }

                    string msg = "put " + i.ItemID + " at " + placement.BlockPosition;
                    Log.WritePlayer(this, msg);
                    Chatting.Parser.TellAdmin(Chat.Red + Player.MinecraftUsername + " " + msg);
                }
            }
            return false;
        }

        /// <summary>
        /// Prevent too frequent messages
        /// </summary>
        DateTime lastPvpMessage;

        /// <summary>
        /// return true to block packet
        /// </summary>
        bool UseEntityFromClient(UseEntity ue)
        {
            //Attack others
            if (ue.Type == UseEntity.Types.Attack)
            {
                //Admin instant kill using bedrock
                if (Player.Admin() &&
                    this.ActiveItem != null &&
                    this.ActiveItem.ItemID == BlockID.Bedrock)
                {
                    
                    VanillaSession dust = World.Main.GetPlayer(ue.Target);
                    if (dust == null)
                    {
                        Player.TellSystem(Chat.Purple, "Failed to find player");
                        return true;
                    }
                    dust.SendToBackend(new ChatMessageClient("/kill"));
                    Log.WritePlayer(this, "Admin kill: " + dust.Player.MinecraftUsername);
                    return true;
                }
                
                if (Player.Settings.Cloaked != null)
                    return false;

                Debug.WriteLine("Attacked: " + ue.Target);
                VanillaSession target = World.Main.GetPlayer(ue.Target);
                
                if (target == null && Player.Settings.Cloaked == null)
                {
                    Entity e = World.Main.GetEntity(ue.Target);
                    var m = e as Mob;
                    var v = e as Vehicle;
                    WorldRegion cr = CurrentRegion;
                    //prevent killing mobs and villagers inside region
                    if (cr != null && cr.Type == "protected" && (cr.IsResident(Player) == false))
                    {
                        if (m != null && m.Type >= MobType.Pig)
                        {
                            Player.TellSystem(Chat.Pink, "No killing inside this region");
                            return true;
                        }
                        if (v != null && v.Type == Vehicles.Frame)
                        {
                            //Protect frames
                            Debug.WriteLine("Frame protected");
                            return true;
                        }
                    }
                    if (m != null && m.Owner != "")
                    {
                        //this could prevent killing of tamed animals   
                    }
                }
                if (target != null && target.Player.Settings.Cloaked == null)
                {
                    
                    if (this.ActiveItem != null)
                    {
                        switch (this.ActiveItem.ItemID)
                        {
                            case BlockID.Rose:
                            case BlockID.Dandelion:
                                PlayerInteraction.Prod(this.Player, target.Player);
                                return true;
                        }
                    }
                    
                    //Anywhere but war
                    WorldRegion r = CurrentRegion;
                    if (r != null && r.Type == "war")
                    {
                        //War Zone
                        //r.Say (Chat.Yellow + Name + Chat.Gold + " attacked " + Chat.Yellow + target.Name + Chat.Gold + " using " + newAttack.Item);
                        SendToBackend(ue);
                        return true;
                    }
                    
                    //Anywhere but war
                    if (Player.PvP == false)
                    {
                        Player.TellSystem(Chat.Purple, "PvP active");
                        Player.PvP = true;
                    }
                    
                    Attacked newAttack = new Attacked(this.Player);
                    
                    if (target.Attacker == null || target.Attacker.Timestamp.AddSeconds(10) < DateTime.Now)
                    {
                        //Regular zone
                        if (target.Player.PvP == false && ((r == null) || (r.IsResident(Player) == false)))
                        {
                            if (target.lastPvpMessage < DateTime.Now)
                            {
                                target.lastPvpMessage = DateTime.Now.AddSeconds(5);
                                PlayerInteraction.Prod(target.Player);
                                target.Player.TellSystem(Chat.Pink, Player.Name + " can't hurt you");
                            }
                            Player.TellSystem(Chat.Pink, "You challenge " + target.Player.Name + " to a fight to the " + Chat.Red + "death");
                            return true;
                        }
                        
                        string msg = Player.Name + " attacked " + target.Player.Name + " using " + newAttack.Item;
                        Chatting.Parser.SayFirehose(Chat.Gray, msg);
                        target.TellSystem(Chat.Gray, msg);
                        this.TellSystem(Chat.Gray, msg);
                        Log.WriteAction(target.Player, newAttack, false);
                    }
                    target.Attacker = newAttack;
                }
                return false;
            }
            else
            {
                //Right click
                Mob m = World.Main.GetEntity(ue.Target) as Mob;
                if (m == null)
                    return false;
                if (m.Owner == "")
                    return false;

                if (m.Type == MobType.Ocelot)
                    this.TellSystem(Chat.Pink, "Meow " + m.Owner);
                else if (m.Type == MobType.Wolf)
                    this.TellSystem(Chat.Pink, "Woof " + m.Owner);
                else
                    this.TellSystem(Chat.Pink, "Owner: " + m.Owner);

                return false;
            }
        }

        private bool FilterDirection(CoordInt block)
        {
            if (block.X == -1 && block.Y == 255 && block.Z == -1)
                return false;
                    
            //Player view is 1.5 above its position
            CoordDouble offset = block - this.Position + new CoordDouble(0.5, -1, 0.5);
            CoordDouble unit = new CoordDouble(
                                   -Math.Cos(Pitch * Math.PI / 180) * Math.Sin(Yaw * Math.PI / 180),
                                   -Math.Sin(Pitch * Math.PI / 180),
                                   Math.Cos(Pitch * Math.PI / 180) * Math.Cos(Yaw * Math.PI / 180));
            double viewDist = offset.Scalar(unit);
            double perDist = (offset - unit * viewDist).Abs;
#if DEBUG
            /*this.Tell ("Block: " + block +
                //"Offset: " + offset.ToString ("0.0") +
                //" Unit: " + unit.ToString ("0.0") +
                " Dist: " + viewDist.ToString ("0.0") +
                " Per: " + perDist.ToString ("0.0"));
                */
#endif
            //Ignore extreme values
            if (perDist > 20)
                return false;
            if (viewDist > 20)
                return false;
            
            //Logical max i 0.87
            //Dont block anymore since client does not restore uncomfirmed blocks
            
            if (perDist > 1.5)
            {
                //Log.WritePlayer (this, "Aimed sideways: " + perDist.ToString ("0.00") + " > 0.87");
                //return true;
            }
            if (viewDist > 6)
            {
                //Log.WritePlayer (this, "Aimed too far: " + viewDist.ToString ("0.0") + " > 6.0, " + block);
                //return true;
            }
            if (viewDist < -0.1)
            {
                //Log.WritePlayer (this, "Aimed behind: " + viewDist.ToString ("0.0") + " < 0");
                //return true;
            }
            return false;
        }

        static int rotatingSourceIP = 1;

        void ConnectToServer()
        {
            IPEndPoint localEP;
            lock (serverConnectLock)
            {
                //Get a unique addres for every connection attempt
                rotatingSourceIP = (rotatingSourceIP + 1) % 0xFFFF;
                byte[] address = new byte[4];
                address[0] = 127;
                address[1] = (byte)(rotatingSourceIP >> 8);
                address[2] = (byte)(rotatingSourceIP & 0xFF);
                address[3] = (byte)(1);
                localEP = new IPEndPoint(new IPAddress(address), 0);
            }
            //Console.WriteLine ("Connecting to " + MinecraftServer.IP + ":" + MinecraftServer.Port + " from " + localEP);

            Socket s = new Socket(
                           AddressFamily.InterNetwork,
                           SocketType.Stream,
                           ProtocolType.Tcp
                       );
            s.Bind(localEP);
            VanillaWorld vw = (VanillaWorld)World;
            s.Connect(vw.Endpoint);
            
            thread.User = Player.MinecraftUsername + " " + s.LocalEndPoint;
            thread.State = "Connected";

            socket = s;
            serverStream = new NetworkStream(s);
        }

        void SendToBackend(PacketFromClient packet)
        {
            if (phase == Phases.FinalClose)
                return;

            Debug.ToServer(packet);

            //Buffer to memory stream
            if (packet.PacketBuffer == null)
                packet.Prepare();

            lock (serverWriterLock)
            {
                if (maxUncompressed >= 0)
                {
                    Packet.WriteVarInt(serverStream, packet.PacketBuffer.Length + Packet.VarIntSize(packet.PacketBufferUncompressedSize));
                    Packet.WriteVarInt(serverStream, packet.PacketBufferUncompressedSize); //Uncompressed size if compressed
                }
                else
                    Packet.WriteVarInt(serverStream, packet.PacketBuffer.Length);

                //Console.WriteLine("TEST: " + (int)ms.Length + " @ " + packet.PacketID);
                serverStream.Write(packet.PacketBuffer, 0, packet.PacketBuffer.Length);
            }
        }

        public override void Kill()
        {
            World.Send("kill " + Player.MinecraftUsername); //Doesn't work either, bug in vanilla
            SendToBackend(new ChatMessageClient("/kill")); //Only works for admins
            Player.Settings.Stats.Suicide += 1;
        }
        #if DEBUG
        bool closed = false;
        #endif
        public override void Close(string message)
        {
            //If you leave a war-zone you die
//          if (this.CurrentRegion != null && this.CurrentRegion.Type == "war") {
//              SendToBackend (new ChatMessage ("/kill"));
//          }

            //Dont report errors, only system errors
            //Error = new Exception (message);

            Log.WritePlayer(this, "Real Closed: " + message);
#if DEBUG
            if (closed)
                Console.WriteLine("Already closed");
            closed = true;          
            Console.WriteLine("RealSession Close: " + message);
#endif
            Vanilla.Leave(this); 
            
            try
            {
                if (serverStream != null)
                    serverStream.Dispose();
            }
            catch (IOException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }
            
            //Must be after we send the disconenct packet
            phase = Phases.FinalClose;

        }

        public override void SetMode(GameMode mode)
        {
            if (this.Mode == mode)
                return;
            World.Send("gamemode " + ((int)mode) + " " + Player.MinecraftUsername);

            base.SetMode(mode);
        }
    }
}

