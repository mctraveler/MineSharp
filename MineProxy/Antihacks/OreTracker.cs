using System;
using MineProxy.Packets;

namespace MineProxy.Misc
{
    public class OreTracker
    {
        /// <summary>
        /// Number of diamonds spawned in front.
        /// </summary>
        public int Diamonds { get; set; }
        
        public DateTime DiamondsTime { get; set; }

        const int minuteLimit = 10;
        readonly TimeSpan DiamondsTimeout = TimeSpan.FromMinutes(minuteLimit);
        const int DiamondsLimit = 10;
        
        public DateTime BrokeBlock { get; set; }

        //Last EID being tracked
        int TrackEID;

        Client client;

        public OreTracker(Client player)
        {
            this.client = player;
        }

        public void Spawn(SpawnObject so, CoordDouble playerPos)
        {
            if (so.Type != Vehicles.Item)
                return;
            if (playerPos.Y > 50)
                return; //Above ground probably a self created ore
            //if (so.SourceEntity == 1)
            //break;
            //if (so..Item.Count != 1)
            //    break;
            if (so.Position.DistanceTo(playerPos) > 10)
                return; //Too far away, not your dig
            if (DateTime.Now - BrokeBlock > TimeSpan.FromSeconds(3))
                return; //not from block broken

            //DO track
            TrackEID = so.EID;
        }

        public void Track(EntityMetadata em)
        {
            if (em.EID != TrackEID)
                return;

            Debug.WriteLine(em.Metadata);

            if (em.Metadata.Fields.ContainsKey(10) == false)
#if DEBUG
                throw new Exception("Protocol changed: Metadata: " + em.Metadata);
#else
                return;
#endif
            if (em.Metadata.Fields [10].Type != MetaType.Item)
            {
#if DEBUG
                if (em.Metadata.Fields [10].Type == MetaType.Byte)
                    return;
                throw new Exception("Protocol changed: Metadata: " + em.Metadata);
#else
                return;
#endif
            }

            var si = em.Metadata.Fields [10].Item;

            switch (si.ItemID)
            {
                case BlockID.Diamond:
                case BlockID.DiamondOre:
                case BlockID.Emerald:
                case BlockID.EmeraldOre:
                    if (DateTime.Now - DiamondsTime > DiamondsTimeout)
                        Diamonds = 0;
                    Diamonds += 1;
                    DiamondsTime = DateTime.Now;
#if DEBUG
                    client.TellSystem(Chat.Pink, "Spawned: " + si);
                    Debug.WriteLine("Spawned: " + si);
#endif
                    if (Diamonds > DiamondsLimit)
                    {
                        string msg = "found " + Diamonds + " " + si.ItemID + " last " + minuteLimit + " minutes";
                        Log.WritePlayer(client, msg);
                        Chatting.Parser.TellAdmin(client.MinecraftUsername + " " + msg);
                    }
                    break;
                    
            }
            TrackEID = 0;
        }
    }
}

