using System;
using System.Collections.Generic;
using System.Threading;
using MineProxy.Chatting;
using MineProxy.Packets;

namespace MineProxy.Worlds
{
    /// <summary>
    /// Waiting hall
    /// </summary>
    public class GreenRoom : Construct
    {
        protected override ChunkData GenerateChunk()
        {
            Random r = new Random();
            ChunkData mc = new ChunkData();
            mc.Complete = true;
            mc.InitContinousChunks(4);
            //Fill with data
            int off = mc.GetIndex(0, 60, 0);
            for (int i = 0; i < off; i++)
                mc.BlockType [i] = (byte)BlockID.Dirt;
            for (int i = 0; i < 16*16; i++)
                mc.BlockType [off + i] = (byte)BlockID.Grass;
            off += 16 * 16;
            for (int i = 0; i < 16*16; i++)
            {
                if (r.Next(4) == 0)
                    mc.BlockType [off + i] = (byte)BlockID.Rose;
                if (r.Next(5) == 0)
                    mc.BlockType [off + i] = (byte)BlockID.Dandelion;
            }
            for (int n = 0; n < mc.BlockLight.Length; n++) 
                mc.BlockLight [n] = 0xff;
            for (int i = 0; i < 16*16; i++)
                mc.Biome [i] = (byte)((i >> 3) % 23);
            return mc;
        }
		
        public GreenRoom()
        {
        }
		
        /// <summary>
        /// Kick one player back to the real world
        /// </summary>
        public void BringBack()
        {
            var list = Players;
            if (list.Length == 0)
                return;
            list [0].Player.SetWorld(World.Main);
        }
		
        public override WorldSession Join(Client player)
        {			
            ConstructSession cs = new GreenSession(player);
            Join(cs, player);
            player.TellSystem(Chat.DarkGreen, "Welcome to the Green Room");
            player.TellSystem(Chat.White, "Type: " + Chat.Blue + "/return" + Chat.White + " when you are ready to return");
            //player.SendToClient (new NewState (NewState.State.BeginRaining));
            return cs;
        }
		
    }
}

