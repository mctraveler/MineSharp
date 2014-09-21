using System;
using System.Diagnostics;
using System.Collections.Generic;
using MineProxy.Packets;

namespace MineProxy.Worlds
{
    /// <summary>
    /// Assigned to a player when they enter a world
    /// </summary>
    public abstract partial class WorldSession
    {
        public readonly Client Player;
        
        public virtual World World { get; protected set; }

        public abstract string ShortWorldName { get; }

        public WorldSession(Client player)
        {
            this.Player = player;
            Mode = GameMode.Survival;
            Position = new CoordDouble();               

            if (player.Session != null)
            {
                Dimension = player.Session.Dimension;
                Position = player.Session.Position;
            }
        }

        public override string ToString()
        {
            return ShortWorldName;
        }

        #region Player location
        
        public Dimensions Dimension { get; set; }

        public CoordDouble Position { get; private set; }

        public void SetPositionServer(CoordDouble pos)
        {
            this.SetPosition(pos, false);
        }

        public WorldRegion CurrentRegion { get; set; }
        
        public double Yaw { get; set; }
        
        public double Pitch { get; set; }
        
        public bool OnGround { get; set; }
        
        #endregion

        readonly SpeedGuard SpeedGuard = new SpeedGuard();

        protected virtual void SetPosition(CoordDouble pos, bool speedguard)
        {
            if (World.Regions != null)
                RegionCrossing.SetRegion(this);

            if (Mode == GameMode.Creative)
                speedguard = false;
            if (Mode == GameMode.Spectator)
                speedguard = false;

            if (speedguard)
                SpeedGuard.ClientMovement(this, pos);
            Position = pos;
        }

        public EffectPost EffectSpeed = new EffectPost(PlayerEffects.MoveSpeed);
        public EffectPost EffectSlow = new EffectPost(PlayerEffects.MoveSlowdown);
        
        public bool Sprinting { get; set; }
        
        public bool Crouched { get; set; }
        
        public bool OnFire { get; set; }
        public DateTime OnFireUntil { get; set; }

        public class ChargeState
        {
            public BlockID Item { get; set; }
            public DateTime Timestamp { get; set; }
            public ChargeState(BlockID item)
            {
                this.Item = item;
                this.Timestamp = DateTime.Now;                
            }
        }

        /// <summary>
        /// Bow charge or Sword protection
        /// </summary>
        public ChargeState Charge { get; set; }

        public GameMode Mode { get; set; }
        
        public int AttachedEntity { get; set; }
        
        public SlotItem[] Inventory = new SlotItem[45];
        
        public int ActiveInventoryIndex { get; protected set; }
        
        public SlotItem ActiveItem { get { return Inventory [ActiveInventoryIndex + 36]; } }
        
        public bool Sleeping { get; set; }

        public virtual void SetMode(GameMode mode)
        {
            Mode = mode;
        }

        public virtual void Kill()
        {
        }
        
        public abstract void Close(string message);
        
        public abstract void FromClient(PacketFromClient packet);

        
        #region Send Queue

        public void Send(PacketFromServer p)
        {
            Player.Queue.Queue(p);
        }

        public void Send(List<PacketFromServer> p)
        {
            Player.Queue.Queue(p);
        }

        #endregion

    }
}

