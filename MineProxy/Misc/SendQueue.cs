using System;
using System.Collections.Generic;
using System.Threading;
using MineProxy.Packets;

namespace MineProxy.Misc
{
    public class SendQueue
    {
        protected readonly Client player;
        /// <summary>
        /// Packets pending to the client
        /// </summary>
        List<PacketFromServer> packetQueue = new List<PacketFromServer>();
        bool sendQueueActive = false;
        /// <summary>
        /// Statistik
        /// </summary>
        int maxPackets = 0;
        int packets = 0;
        bool disposed = false;

        public SendQueue(Client player)
        {
            this.player = player;
        }

        public void Dispose()
        {
            disposed = true;
            //lock (packetQueue)
            {

            }
        }

        public void Queue(PacketFromServer p)
        {
            if (disposed)
                return;
            lock (packetQueue)
            {
                packetQueue.Add(p);

                packets += 1;
                if (maxPackets < packets)
                    maxPackets = packets;

                if (sendQueueActive == false)
                {
                    sendQueueActive = true;
                    ThreadPool.QueueUserWorkItem(SendAll);
                }
            }
        }

        public void Queue(List<PacketFromServer> list)
        {
            if (disposed)
                return;

            lock (packetQueue)
            {
                foreach (var p in list)
                {
                    packetQueue.Add(p);
                }

                packets += list.Count;
                if (maxPackets < packets)
                    maxPackets = packets;

                if (sendQueueActive == false)
                {
                    sendQueueActive = true;
                    ThreadPool.QueueUserWorkItem(SendAll);
                }
            }
        }

        void SendAll(object state)
        {
            if (disposed)
                return;

            PacketFromServer[] packetList;
            while (true)
            {
                lock (packetQueue)
                {
                    if (packetQueue.Count == 0)
                    {
                        sendQueueActive = false;
                        //Debug.WriteLine("SendQueue: Closing");
                        return;
                    }
                    
                    packetList = packetQueue.ToArray();
                    packetQueue.Clear();

                    packets = 0;
                }
                
                //Debug.WriteLine("SendQueue: " + packetList.Length);
                
                try
                {
                    foreach (var packet in packetList)
                    {
                        player.SendToClient(packet);
                    }
                }
                #if !DEBUG
                catch (Exception e)
                {
                    Log.WriteServer(e);
                    this.player.Close("Internal error, check server log: " + e.Message);
                }
                #else
                finally
                {
                }
                #endif
                
            }
        }

        public string SendQueueStatus()
        {
            int packets = 0;
            lock (packetQueue)
            {
                foreach (var p in packetQueue)
                {
                    packets += 1;
                }
            }
            return packets + " packets, max " + maxPackets + " packets";
        }
    }
}

