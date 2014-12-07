using System;
using System.Collections.Generic;
using System.Threading;
using MineProxy.Packets.FromServer;
using MineProxy.Packets;

namespace MineProxy.Worlds
{
    /// <summary>
    /// A template for proxy side worlds
    /// </summary>
    public abstract class Construct : World
    {
        private ChunkData ChunkData;
        
        protected virtual Dimensions Dimension { get { return Dimensions.Overworld; } }
        
        public Construct()
        {
            pingThread = new Timer(PingThread, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            ChunkData = GenerateChunk();
        }
        
        protected abstract ChunkData GenerateChunk();

        readonly Timer pingThread;
        Random r = new Random();

        void PingThread(object state)
        {
            if (Program.Active == false)
            {
                pingThread.Dispose();
                return;
            }
            //Debug.WriteLine(ToString() + " Ping Thread Run");

            foreach (ConstructSession cs in Players)
            {
                cs.Player.Queue.Queue(new KeepAlivePing(r.Next()));
                //cs.SendToClient (new TimeUpdate (18000));
            }
        }
                
        /// <summary>
        /// To be called by Join(Client player)
        /// </summary>
        protected void Join(ConstructSession cs, Client player)
        {
            base.Join(cs);
            
            SendStartup(cs, player);
        
            //Dont show cloaked
            if (player.Settings.Cloaked != null)
                return;
            
            //Send named entity
            SpawnPlayer n = new SpawnPlayer(player.EntityID, player);
            n.Position = cs.Position;
            
            foreach (ConstructSession c in Players)
            {
                if (c == cs)
                    continue;
                
                //Spawn new player in front of c
                c.Player.Queue.Queue(n);              
                Debug.WriteLine("Spawn " + n.PlayerUUID + " in front of " + c.Player.MinecraftUsername);
                
                //Spawn c in front of new player
                SpawnPlayer nes = new SpawnPlayer(
                    c.Player.MinecraftUsername.GetHashCode(),
                    c.Player);
                nes.Position = c.Position;
                nes.Pitch = c.Pitch;
                nes.Yaw = c.Yaw;
                
                cs.Player.Queue.Queue(nes);               
            }
        }

        protected void SendStartup(ConstructSession cs, Client player)
        {
            //First another dimension to trigger full drop of old map
            Respawn r = new Respawn();
            if (Dimension == Dimensions.Overworld)
                r.Dimension = Dimensions.Nether;
            else
                r.Dimension = Dimensions.Overworld;
            r.Mode = 0;
            r.Difficulty = 3;
            player.Queue.Queue(r);

            r = new Respawn();
            r.Dimension = (Dimensions)Dimension;
            r.Mode = 0;
            r.Difficulty = 3;
            player.Queue.Queue(r);

            //player.Queue.Queue(new SpawnPosition(cs.Position.CloneInt()));
            player.Queue.Queue(new TimeUpdate(18000));
            
            cs.Position.Y = 66;
            
            var ppl = new PlayerPositionLookServer(cs.Position);
            ppl.Yaw = cs.Yaw;
            ppl.Pitch = cs.Pitch;
            player.Queue.Queue(ppl);
            
            player.Queue.Queue(new TimeUpdate(1000));
            
            //player.Queue.Queue(new NewState(NewState.State.EndRaining));
            
            int min = -1;
            int max = 1;
            for (int cx = min; cx < max; cx++)
            {
                for (int cz = min; cz < max; cz++)
                {
                    ChunkData mc = new ChunkData();
                    mc.ChunkBitMap = ChunkData.ChunkBitMap;
                    mc.Complete = ChunkData.Complete;
                    mc.X = cx;
                    mc.Z = cz;
                    mc.BlockType = ChunkData.BlockType;
                    mc.BlockMeta = ChunkData.BlockMeta;
                    mc.BlockLight = ChunkData.BlockLight;
                    mc.BlockSkyLight = ChunkData.BlockSkyLight;
                    mc.Biome = ChunkData.Biome;
                    player.Queue.Queue(mc);
                }
            }

            r = new Respawn();
            r.Dimension = (Dimensions)Dimension;
            r.Mode = 0;
            r.Difficulty = 3;
            player.Queue.Queue(r);

            player.Queue.Queue(new UpdateHealth(20, 20));

            var properties = new EntityProperties(player.EntityID, 0.10, 20.0);
            player.Queue.Queue(properties);
        }
    }
}

