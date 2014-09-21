using System;
using MiscUtil.IO;
using System.IO;
using MiscUtil.Conversion;
using MineProxy.Packets.Plugins;

namespace MineProxy.Packets
{
    public abstract class PacketFromClient : Packet
    {
        public static PacketFromClient ReadClient(Stream s)
        {
            int uncompressedSize;
            byte[] buffer = PacketReader.Read(s, out uncompressedSize);
            var type = buffer[0];
            if (type > 0x80)
                throw new NotImplementedException("Varint encoding larger than 128");

            //Debug.WriteLine("Packet Reading... 0x" + type.ToString("X2"));
            var packet = Read(buffer[0]);
            packet.SetPacketBuffer(buffer, uncompressedSize);
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
                        string orig = BitConverter.ToString(buffer);
                        string prep = BitConverter.ToString(packet.PacketBuffer);
                        if (orig != prep)
                            throw new InvalidOperationException("Format mismatch\norig: " + orig + "\nprep " + prep);
                        break;
                    case PluginMessageFromClient.ID:
                        break;
                }
            }
            #endif
            return packet;
        }

        static PacketFromClient Read(byte type)
        {
            switch (type)
            {
                case KeepAlivePong.ID:
                    return new KeepAlivePong();
                case ChatMessageClient.ID:
                    return new ChatMessageClient();
                case UseEntity.ID:
                    return new UseEntity();
                case PlayerGround.ID:
                    return new PlayerGround();
                case PlayerPosition.ID:
                    return new PlayerPosition();
                case PlayerLook.ID:
                    return new PlayerLook();
                case PlayerPositionLookClient.ID:
                    return new PlayerPositionLookClient();
                case PlayerDigging.ID:
                    return new PlayerDigging();
                case PlayerBlockPlacement.ID:
                    return new PlayerBlockPlacement();
                case HeldItemClient.ID:
                    return new HeldItemClient();
                case AnimationClient.ID:
                    return new PassThroughClient(); //return new AnimationClient();
                case EntityAction.ID:
                    return new EntityAction();
                case Steer.ID:
                    return new PassThroughClient(); //return new Steer();
                case WindowCloseClient.ID:
                    return new WindowCloseClient();
                case WindowClick.ID:
                    return new WindowClick();
                case ConfirmTransactionClient.ID:
                    return new PassThroughClient(); //return new ConfirmTransactionClient();
                case CreativeInventory.ID:
                    return new CreativeInventory();
                case EnchantItem.ID:
                    return new PassThroughClient(); //return new EnchantItem();
                case UpdateSignClient.ID:
                    return new PassThroughClient(); //return new UpdateSignClient();
                case PlayerAbilities.ID:
                    return new PlayerAbilities();
                case TabCompleteClient.ID:
                    return new TabCompleteClient();
                case ClientSettings.ID:
                    return new PassThroughClient(); //return new ClientSettings();
                case ClientStatus.ID:
                    return new ClientStatus();
                case PluginMessageFromClient.ID:
                    return new PassThroughClient(); //return new PluginMessageFromClient();
                case Spectate.ID: //0x18
                    return new PassThroughClient(); //return new Spectate();
                case ResourcePackStatus.ID: //0x19
                    return new PassThroughClient();

                default:
                    throw new NotImplementedException("Not Implemented packet: " + type);
            }
        }
    }
}

