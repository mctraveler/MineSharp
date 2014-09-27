using System;
using System.Net.Sockets;
using MiscUtil.IO;
using MiscUtil.Conversion;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using MineProxy.Packets;
using MineProxy.Chatting;
using MineProxy.Worlds;
using MineProxy.Misc;
using MineProxy.Packets.Plugins;

namespace MineProxy
{
    public enum Phases
    {
        Handshake,
        Gaming,
        FinalClose,
    }

    public partial class Client
    {
        protected Socket socket;
        protected Stream clientStream;
        protected Threads clientThread;
        public SendQueue Queue;

        public string ThreadStatus
        {
            get{ return clientThread.State; }
            set{ clientThread.State = value; }
        }

        public IPEndPoint RemoteEndPoint { get; private set; }

        public string Country { get; private set; }

        /// <summary>
        /// Current inbox state
        /// </summary>
        public int Inbox { get; set; }

        /// <summary>
        /// The incoming client version
        /// </summary>
        public ProtocolVersion ClientVersion { get; set; }

        /// <summary>
        /// Which world the player entered
        /// </summary>
        public WorldSession Session { get; private set; }

        readonly object sessionLock = new object();

        #region Chat Flood detection

        public int ChatFloodCount { get; set; }

        public DateTime ChatFloodNextReset { get; set; }

        #endregion

        #region Ping

        public double Ping = 10000;
        Dictionary<int, DateTime> pings = new Dictionary<int, DateTime>();

        #endregion

        #region Possession

        PossessSession[] PossessedBy = new PossessSession[0];
        readonly object possessLock = new object();

        /// <summary>
        /// Add a possesser to this player
        /// </summary>
        public void Possess(PossessSession session)
        {
            var ghostList = new List<PossessSession>(session.Player.PossessedBy);
            if (this.Session is PossessSession && ghostList.Contains((PossessSession)this.Session))
            {
                session.Player.TellSystem(Chat.Red, "Already possessed by " + Name);
                return;
            }

            lock (possessLock)
            {
                var list = new List<PossessSession>(PossessedBy);
                list.Add(session);
                PossessedBy = list.ToArray();
            }
        }

        /// <summary>
        /// Remove a possesser from this player
        /// </summary>
        public void Exorcise(PossessSession session)
        {
            lock (possessLock)
            {
                var list = new List<PossessSession>(PossessedBy);
                list.Remove(session);
                PossessedBy = list.ToArray();
            }
        }

        private void SendToPossessors(PacketFromClient p)
        {
            var pby = PossessedBy;
            if (pby.Length == 0)
                return;
                
            VanillaSession real = Session as VanillaSession;

            PacketFromServer ps = null;

            switch (p.PacketID)
            {
                case PlayerPositionLookClient.ID:
                    {
                        if (real == null)
                            return;

                        var pl = (PlayerPositionLookClient)p;
                        var et = new EntityTeleport(real.EID, pl.Position);
                        et.Yaw = pl.Yaw;
                        et.Pitch = pl.Pitch;

                        EntityHeadYaw hy = new EntityHeadYaw(real.EID, pl.Yaw);
                        foreach (WorldSession s in pby)
                        {
                            s.Player.Queue.Queue(et);
                            s.Player.Queue.Queue(hy);
                        }
                    }
                    return;
            }
            
            if (ps == null)
                return;
                
            foreach (WorldSession s in pby)
                s.Player.Queue.Queue(ps);
        }

        private void SendToPossessors(PacketFromServer p)
        {
            var pby = PossessedBy;
            if (pby.Length == 0)
                return;

            VanillaSession real = Session as VanillaSession;

            switch (p.PacketID)
            {
                case PlayerAbilities.ID:
                case ChatMessageServer.ID:
                    return;

                case ChangeGameState.ID:
                    var ns = (ChangeGameState)p;
                    if (ns.Reason == GameState.ChangeGameMode)
                        return;
                    break;

                case CollectItem.ID:
                    {
                        var ci = (CollectItem)p;
                        if (ci.EID == EntityID && real != null)
                            ci.EID = real.EID;
                        break;
                    }

                
                case Animation.ID:
                    {
                        Animation pl = (Animation)p;
                        var et = new Animation(this.EntityID, pl.Animate);
                        p = et;
                        break;
                    }
                case Disconnect.ID:
                    {
                        //Kick all possessers
                        foreach (WorldSession s in pby)
                            s.Player.SetWorld(World.Construct);
                        return;
                    }
                case PlayerListItem.ID:
                    return;
            }

            foreach (WorldSession s in pby)
                s.Player.Queue.Queue((PacketFromServer)p);
        }

        #endregion

        public void Start()
        {
            clientThread.Start();
        }

        public void Join()
        {
            clientThread.Join();
        }

        public Phases Phase { get; protected set; }

        /// <summary>
        /// Special worlds where the entrance region matters
        /// such as banks and warpzones
        /// </summary>
        public void SetWorld(World w)
        {
            lock (sessionLock)
            {
                if (Phase == Phases.FinalClose)
                    w = World.Void;
                WorldSession old = Session;
                Session = World.Void.Join(this);
                clientThread.State = "Void";

                if (old != null)
                    old.Close("going to " + w);
                WorldSession n;
                
                //Banned players
                BadPlayer b = Banned.CheckBanned(this);
                if (b == null)
                {
                    n = w.Join(this);

                    AfkSession afk = n as AfkSession;
                    if (afk != null)
                        afk.OriginalWorld = old.World;
                }
                else
                    n = World.HellBanned.Join(this);                
                
                //If we are denied
                if (n == null)
                    Session = World.Wait.Join(this);
                else
                    Session = n;

                clientThread.State = Session.ToString();
            }
        }

        /// <summary>
        /// Special worlds where the entrance region matters
        /// such as banks and warpzones
        /// </summary>
        public void SetSession(WorldSession session)
        {
            lock (sessionLock)
            {
                WorldSession old = Session;
                Session = session;
                if (old != null)
                    old.Close("going to " + session);
            }
        }

        /// <summary>
        /// Prevent the tp command from being sent 10 times when entering a single portal
        /// </summary>
        DateTime nextWarp = DateTime.Now;

        /// <summary>
        /// Warp Player
        /// </summary>
        public void Warp(CoordDouble destination, Dimensions dimension, World world)
        {
            //Prevent multiple warp commands
            if (nextWarp > DateTime.Now)
                return; //Already warped
            nextWarp = DateTime.Now.AddSeconds(5);

            SendToClient(new SoundEffect("portal.trigger", Session.Position, 0.5, 63));
            SendToClient(new SoundEffect("portal.travel", Session.Position, 0.5, 63));

            ///Vanilla tp is enough
            if (Session.Dimension == dimension &&
                Session.World == world)
            {
                Session.World.Send("tp " + MinecraftUsername + " " + destination.ToStringClean("0.00"));
                return;
            }

            //Change worlds
            if (Session.World != world)
            {
                SetWorld(world);
                return;
            }

            //Error message
            this.TellSystem("", "Warp cross dimension never worked in vanilla");
        }

        void WatchdogKilled()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Send);
            }
            catch
            {
            }
        }

        void ReceiverRunClientWrapper()
        {
            try
            {
                ReceiverRunClient();
            }
            catch (ThreadInterruptedException)
            {
                return;
            }
#if !DEBUG
            catch (Exception e)
            {
                Log.Write(e, this);
            }
#endif
            finally
            {
#if DEBUG
                Console.WriteLine("ClientReceiver Ended: " + this);
#endif
                clientThread.State = "Finally closing down 0";

                Phase = Phases.FinalClose;
                SetWorld(World.Void);

                clientThread.State = "Finally closing down 1";

                SaveProxyPlayer();
               
                clientThread.State = "Finally closing down 2";

                PlayerList.LogoutPlayer(this);

                clientThread.State = "Finally closing down 3";

                RegionCrossing.ClearRegion(this);

                clientThread.State = "Finally closing down 4";

                socket.Close();

                clientThread.State = "Finally closing down 5";

                //clientStream.Flush();
                clientStream.Close();

                clientThread.State = "Finally closing down -all done";
            }
        }

        protected virtual void ReceiverRunClient()
        {
            throw new NotImplementedException();
        }

        public virtual void Close(string message)
        {
            if (Queue != null)
                Queue.Dispose();
            SetWorld(World.Void);
            Log.WritePlayer(this, "Client Closed: " + message);
            if (Phase == Phases.Handshake)
                SendToClient(new DisconnectHandshake(message));
            else
                SendToClient(new Disconnect(message));
            Phase = Phases.FinalClose;
            clientStream.Flush();
            clientStream.Close();
        }

        public void FromClient(PacketFromClient packet)
        {
            if (packet.PacketID == PassThrough.ID)
            {
                Session.FromClient(packet);
                return;
            }

            //Ping update
            if (packet.PacketID == KeepAlivePong.ID)
            {
                var ka = packet as KeepAlivePong;
                lock (pings)
                {
                    if (pings.ContainsKey(ka.KeepAliveID))
                    {
                        Ping = (DateTime.Now - pings[ka.KeepAliveID]).TotalMilliseconds;
                        pings.Remove(ka.KeepAliveID);
                    }
                }
            }

            //Send possessed packets
            SendToPossessors(packet);

            //Check for invalid data, hacked clients(Derp)
            double pitch = 0;
            if (packet.PacketID == PlayerPositionLookClient.ID)
                pitch = ((PlayerPositionLookClient)packet).Pitch;
            if (pitch > 91 || pitch < -91)
            {
                //this.Ban (null, DateTime.Now.AddMinutes (42), "Modified client");
                if (Banned.CheckBanned(MinecraftUsername) == null)
                    Chatting.Parser.TellAdmin(Name + Chat.Gray + " crazy head angle " + pitch.ToString("0.0"));
                SendToClient(new EntityStatus(EntityID, MineProxy.Data.EntityStatuses.EntityHurt));
            }
            
            if (packet.PacketID == Packets.ClientSettings.ID)
            {
                var vd = (Packets.ClientSettings)packet;
                this.Locale = vd.Locale;
                this.ViewDistance = vd.ViewDistance;
            }

            //Check for book signatures
            if (packet.PacketID == MCBook.ID && packet is MCBook)
            {
                var b = packet as MCBook;
                if (b != null)
                {
                    Log.WriteBook(this, b);
                    Debug.WriteLine("Book: " + b);

                    //Check for signatures
                    RuleBook.Rules.Verify(this, b);
                }
            }

            WorldSession s = Session;
            try
            {
                
                switch (packet.PacketID)
                {
                    case ChatMessageClient.ID:
                        Parser.ParseClient(this, (ChatMessageClient)packet);
                        //AfkTime = DateTime.Now;
                        return;
                    case TabCompleteClient.ID:
                        Parser.ParseClient(this, (TabCompleteClient)packet);
                        return;             

                    default:
                        s.FromClient(packet);
                        break;
                }
            }
            catch (SessionClosedException sc)
            {
                SessionClosed(s, sc);
            }
            catch (IOException sc)
            {
                SessionClosed(s, sc);
            }
#if !DEBUG
            catch (Exception e)
            {
                Log.Write(e, this);
                SessionClosed(s, e);
            }
#endif
        }

        public void SendToClient(List<PacketFromServer> packets)
        {
            foreach (var p in packets)
                SendToClient(p);
        }

        public void SendToClient(PacketFromServer packet)
        {
            if (Phase == Phases.FinalClose)
                return;

#if DEBUG
            /*if (packet is PlayerPositionLook)
                Thread.Sleep(50);
            Thread.Sleep(30);*/
#endif

            //Thread.Sleep(100);
            //Debug.WriteLine(packet);

            //Send possessed packets
            SendToPossessors(packet);

            //Ping update
            KeepAlivePing ka = packet as KeepAlivePing;
            if (ka != null)
            {
                lock (pings)
                {
                    if (pings.ContainsKey(ka.KeepAliveID))
                        pings.Remove(ka.KeepAliveID);
                    if (pings.Count > 5)
                        pings.Clear();
                    
                    pings.Add(ka.KeepAliveID, DateTime.Now);
                }
            }

            SendToClientInternal(packet);
        }

        protected virtual void SendToClientInternal(PacketFromServer packet)
        {
            throw new NotImplementedException();
        }
    }
}

