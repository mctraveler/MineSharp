using System;
using MiscUtil.IO;
using System.IO;
using MiscUtil.Conversion;
using MineProxy.Packets.FromServer;
using MineProxy.Packets.Plugins;

namespace MineProxy.Packets
{
    public abstract class PacketFromServer : Packet
    {
        public static PacketFromServer ReadServer(Stream stream)
        {
            int uncompressedLength;
            byte[] buffer = PacketReader.Read(stream, out uncompressedLength);
            var type = buffer[0];
            if (type > 0x80)
                throw new NotImplementedException("Varint encoding larger than 128");

            //Debug.WriteLine("Packet Reading... 0x" + type.ToString("X2"));
            byte id = uncompressedLength == 0 ? buffer[0] : Compression.ReadCompressedID(buffer);

            var packet = ReadServer(id);
            packet.SetPacketBuffer(buffer, uncompressedLength);
            if (packet.PacketID != PassThrough.ID)
                packet.Parse();

            #if DEBUG

            if (packet.PacketID != PassThrough.ID)
            {
                switch (packet.PacketID)
                {
                    default:
                        packet.Parse();
                        packet.Prepare();
                        string orig = Convert.ToString(buffer);
                        string prep = Convert.ToString(packet.PacketBuffer);
                        if (orig != prep)
                            throw new InvalidOperationException("Format mismatch orig: " + orig + " prep " + prep);
                        break;
                    case PlayerListItem.ID:
                        break;
                }
            }
            #endif
            return packet;
        }

        static PacketFromServer ReadServer(byte type)
        {   
            switch (type)
            {
                case KeepAlivePing.ID: //00
                    return new KeepAlivePing();
                case JoinGame.ID: //01
                    return new JoinGame();
                case ChatMessageServer.ID: //02
                    return new ChatMessageServer();
                case TimeUpdate.ID: //03
                    return new PassThroughServer();//return new TimeUpdate();
                case EntityEquipment.ID: //04
                    return new EntityEquipment();
                case SpawnPosition.ID:
                    return new SpawnPosition();
                case UpdateHealth.ID:
                    return new UpdateHealth();
                case Respawn.ID:
                    return new Respawn();
                case PlayerPositionLookServer.ID:
                    return new PlayerPositionLookServer();
                case HeldItemServer.ID:
                    return new HeldItemServer();
                case UseBed.ID:
                    return new UseBed();
                case Animation.ID:
                    return new Animation();
                case SpawnPlayer.ID:
                    return new SpawnPlayer();
                case CollectItem.ID:
                    return new CollectItem();
                case SpawnObject.ID:
                    return new PassThroughServer();//return new SpawnObject();
                case SpawnMob.ID:
                    return new PassThroughServer();//return new SpawnMob();
                case SpawnPainting.ID:
                    return new PassThroughServer();//return new SpawnPainting();
                case SpawnExperienceOrb.ID:
                    return new SpawnExperienceOrb();
                case EntityVelocity.ID:
                    return new EntityVelocity();
                case DestroyEntities.ID:
                    return new PassThroughServer();//return new DestroyEntities();
                case EntityRelativeMove.ID:
                    return new EntityRelativeMove();
                case EntityLook.ID:
                    return new EntityLook();
                case EntityLookRelativeMove.ID:
                    return new EntityLookRelativeMove();
                case EntityTeleport.ID:
                    return new EntityTeleport();
                case EntityHeadYaw.ID:
                    return new EntityHeadYaw();
                case EntityStatus.ID:
                    return new EntityStatus();
                case AttachEntity.ID:
                    return new AttachEntity();
                case EntityMetadata.ID:
                    return new EntityMetadata();
                case EntityEffect.ID:
                    return new EntityEffect();
                case RemoveEntityEffect.ID:
                    return new RemoveEntityEffect();
                case SetExperience.ID:
                    return new PassThroughServer();//return new SetExperience();
                case EntityProperties.ID:
                    return new EntityProperties();
                case ChunkData.ID:
                    return new PassThroughServer();//return new ChunkData();
                case MultiBlockChange.ID:
                    return new PassThroughServer();//return new MultiBlockChange();
                case BlockChange.ID:
                    return new PassThroughServer();//return new BlockChange();
                case BlockAction.ID:
                    return new PassThroughServer();//return new BlockAction();
                case BlockBreakAnimation.ID:
                    return new BlockBreakAnimation();
                case ChunkDataBulk.ID:
                    return new PassThroughServer();//return new ChunkDataBulk();
                case Explosion.ID:
                    return new PassThroughServer();//return new Explosion();
                case Effect.ID:
                    return new PassThroughServer();//return new Effect();
                case SoundEffect.ID:
                    return new PassThroughServer();//return new SoundEffect();
                case Particle.ID:
                    return new PassThroughServer();//return new Particle();
                case ChangeGameState.ID:
                    return new ChangeGameState();
                case SpawnGlobalEntity.ID:
                    return new PassThroughServer();//return new SpawnGlobalEntity();
                case WindowOpen.ID:
                    return new WindowOpen();
                case WindowCloseServer.ID:
                    return new WindowCloseServer();
                case SetSlot.ID:
                    return new SetSlot();
                case WindowItems.ID:
                    return new PassThroughServer();
                case WindowProperty.ID:
                    return new PassThroughServer();//return new WindowProperty();
                case ConfirmTransactionServer.ID:
                    return new PassThroughServer();//return new ConfirmTransactionServer();
                case UpdateSignServer.ID:
                    return new PassThroughServer();//return new UpdateSignServer();
                case Maps.ID:
                    return new PassThroughServer();//return new Maps();
                case UpdateBlockEntity.ID:
                    return new PassThroughServer();
                case SignEditorOpen.ID:
                    return new PassThroughServer();//return new SignEditorOpen();
                case IncrementStatistic.ID:
                    return new PassThroughServer();//return new IncrementStatistic();
                case PlayerListItem.ID: //0x38
                    return new PlayerListItem();
                case PlayerAbilitiesServer.ID:
                    return new PassThroughServer();//return new PlayerAbilitiesServer();
                case TabComplete.ID:
                    return new PassThroughServer();//return new TabComplete();
                case ScoreboardObjective.ID:
                    return new ScoreboardObjective();
                case ScoreboardUpdate.ID:
                    return new PassThroughServer();//return new ScoreboardUpdate();
                case ScoreboardShow.ID:
                    return new PassThroughServer();//return new ScoreboardShow();
                case TeamUpdate.ID:
                    return new PassThroughServer();//return new TeamUpdate();
                case PluginMessageFromServer.ID://0x3F
                    return new PassThroughServer();//return new PluginMessageFromServer();
                case Disconnect.ID://0x40
                    return new Disconnect(); //Ignore why client disconnects
                case ServerDifficulty.ID:
                    return new PassThroughServer();//return new ServerDifficulty();
                case CombatEvent.ID: //0x42: //CombatEvent:
                case Camera.ID: //0x43 Camera
                case WorldBorder.ID: //0x44 WorldBorder
                case 0x45: //Title
                    return new PassThroughServer();
                case PlayerListHeaderFooter.ID: //0x47
                    return new PlayerListHeaderFooter();
                case ResourcePackSend.ID: //0x48
                case UpdateEntityNBT.ID: //0x49
                    return new PassThroughServer();

                default:
                    #if DEBUGPACKET
                    throw new NotImplementedException("Not Implemented packet: 0x" + type.ToString("X"));
                    #else
                    return new PassThroughServer();
                    #endif
            }
        }
    }
}

