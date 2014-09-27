using System;
using System.Diagnostics;
using MineProxy.Packets;

namespace MineProxy
{
    public static class Debug
    {
        [Conditional("DEBUG")]
        public static void WriteLine()
        {
#if DEBUG
            Console.WriteLine();
#endif
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }

        [Conditional("DEBUG")]
        public static void Write(string message)
        {
#if DEBUG
            Console.Write(message);
#endif
        }

        [Conditional("DEBUG")]
        public static void WriteLine(object message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }

        [Conditional("DEBUG")]
        public static void Write(object message)
        {
#if DEBUG
            Console.Write(message);
#endif
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition)
        {
#if DEBUG
            if (condition == false)
                throw new NotImplementedException();
#endif
        }
                
        #if DEBUG
        #pragma warning disable 162
        static bool Show(Packet packet)
        {
            byte id = packet.PacketID;
            if (id == PassThrough.ID)
                id = packet.PacketBuffer[0];

            switch (id)
            {
                case EntityHeadYaw.ID:
                case EntityRelativeMove.ID:
                case ChunkData.ID:
                case ChunkDataBulk.ID:
                case EntityVelocity.ID:
                case PlayerGround.ID:
                case EntityLookRelativeMove.ID:
                    return false;
                default:
                case LoginSuccess.ID:
                case SpawnPlayer.ID:
                case PlayerListItem.ID:
                    return true;
            }
        }
        #pragma warning restore 162
        #endif

        [Conditional("DEBUG")]
        public static void FromServer(PacketFromServer packet, Client player)
        {
            #if DEBUG
            if (Show(packet))
                Console.WriteLine("S-> : " + packet);
            #endif
        }

        [Conditional("DEBUG")]
        public static void ToServer(PacketFromClient packet)
        {
            #if DEBUG
            if (Show(packet))
                Console.WriteLine(" ->S: " + packet);
            //System.Threading.Thread.Sleep(50);

            #endif
        }

        [Conditional("DEBUG")]
        public static void FromClient(Client c, PacketFromClient packet)
        {
            #if DEBUG
            if (Show(packet))
                Console.WriteLine("C-> : " + packet);
            #endif
        }

        [Conditional("DEBUG")]
        public static void ToClient(Client c, PacketFromServer packet)
        {
            #if DEBUG
            if (Show(packet))
                Console.WriteLine(" ->C: " + packet.PacketBuffer.Length + ", " + packet);
            //System.Threading.Thread.Sleep(500);
            #endif
        }

    }
}

