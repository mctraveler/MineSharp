using System;
using System.IO;
using MineProxy.Data;
using MineProxy.Chatting;
using MineProxy.Packets;

namespace MineProxy.Worlds
{
    public abstract class ConstructSession : WorldSession
    {
        Construct world { get { return (Construct)this.World; } }

        public ConstructSession(Client player) : base(player)
        {
        }

        public override void Close(string message)
        {
            world.Leave(this);	
			
            QueueToAllButMe(new DestroyEntities(Player.EntityID));
        }

        protected void QueueToAllButMe(PacketFromServer p)
        {
            foreach (ConstructSession c in world.Players)
            {
                if (c == this)
                    continue;
                c.Player.Queue.Queue(p);
            }
        }

        public void QueueToAll(PacketFromServer p)
        {
            foreach (ConstructSession c in world.Players)
            {
                c.Player.Queue.Queue(p);
            }
        }

        public override void FromClient(PacketFromClient packet)
        {
            switch (packet.PacketID)
            {
            #region Block place/break

                case PlayerDigging.ID:
                    PlayerDigging pd = packet as PlayerDigging;
                    if (pd.Status == PlayerDigging.StatusEnum.FinishedDigging)
                        QueueToAll(new BlockChange(pd.Position, BlockID.Air));
                    return;
            /* TODO: fix: crashes with new items such as pot
                case PlayerBlockPlacement.ID:
                    PlayerBlockPlacement pbp = packet as PlayerBlockPlacement;
                    if(pbp.Item == null)
                        return;
                    var bc = new BlockChange(pbp.BlockPosition.Offset(pbp.FaceDirection), pbp.Item.ItemID);
                    bc.Metadata = (byte)pbp.Item.Uses;
                    QueueToAll(bc);
                    return;*/

            #endregion

			#region Moving Looking
			
                case PlayerPositionLookClient.ID:
                    {
                        PlayerPositionLookClient p = (PlayerPositionLookClient)packet;
    			
                        SetPosition(p.Position, true);
                        Pitch = p.Pitch;
                        Yaw = p.Yaw;
    			
                        if (Player.Settings.Cloaked == null)
                        {
                            EntityTeleport et = new EntityTeleport(Player.EntityID, p.Position);
                            et.Yaw = p.Yaw;
                            et.Pitch = p.Pitch;
                            QueueToAllButMe(et);

                            QueueToAllButMe(new EntityHeadYaw(Player.EntityID, Yaw));
                        }
                        CheckPosition();
                        return;
                    }
                
			#endregion
			
                case UseEntity.ID:
                    //UseEntity ue = (UseEntity)packet;
                    //Sender see hurt
                    //New: hurt attacker, old: ue.Target
                    QueueToAll(new EntityStatus(this.Player.EntityID, EntityStatuses.EntityHurt));
                    QueueToAll(new Effect(SoundEffects.MobSpawn, Position.CloneInt()));
                    return;

#if DEBUG
                case PlayerGround.ID:
                case KeepAlivePing.ID://case KeepAlivePong.ID:
                    return;
			
                default:
                    //Console.WriteLine("Unhandled: " + packet);
                    return;
#endif
            }
        }

        private void CheckPosition()
        {
            if (this.Position.Y < -50)
            {
                SetPosition(new CoordDouble(0, 128, 0), false);
                this.Pitch = 60;
            }
			/*
			else if (this.Position.X < 0)
				this.Position.X += 16;
			else if (this.Position.Z < 0)
				this.Position.Z += 16;
			else if (this.Position.X > 16)
				this.Position.X -= 16;
			else if (this.Position.Z > 16)
				this.Position.Z -= 16;
			*/ else
                return;
						
            if (this.Position.Y < 65.01)
                this.Position.Y = 65.05;
			
            var ppl = new PlayerPositionLookServer(this.Position);
            ppl.Yaw = this.Yaw;
            ppl.Pitch = this.Pitch;
            this.Player.SendToClient(ppl);
        }

        public override void Kill()
        {
            Player.TellAbove(Chat.Red, "You are already dead, muahaha");
        }
    }
}

