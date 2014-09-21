using System;
using System.Collections.Generic;
using System.Threading;
using MineProxy.Chatting;
using MineProxy.Packets;

namespace MineProxy.Worlds
{
    /// <summary>
    /// A template for proxy side worlds
    /// </summary>
    public class Hell : Construct
    {
        protected override Dimensions Dimension { get { return Dimensions.Nether; } }

        protected override ChunkData GenerateChunk()
        {
            Random r = new Random();
            ChunkData mc = new ChunkData();
            mc.Complete = true;
            mc.InitContinousChunks(4);
            //Fill with data
            int off = mc.GetIndex(0, 13, 0);
            for (int i = 0; i < 16*16; i++)
                mc.BlockType [off + i] = (byte)BlockID.SoulSand;
            off += 16 * 16;
            for (int i = 0; i < 16*16; i++)
            {
                if (r.Next(4) > 0)
                    mc.BlockType [off + i] = (byte)BlockID.SoulSand;
                else
                    mc.BlockType [off + i] = (byte)BlockID.Lava;
            }
            off += 16 * 16;
            for (int i = 0; i < 16*16; i++)
            {
                if (r.Next(4) == 0)
                    mc.BlockType [off + i] = (byte)BlockID.Fire;
            }
            for (int n = 0; n < mc.BlockLight.Length; n++)
                mc.BlockLight [n] = 0x88;
            for (int n = 0; n < mc.Biome.Length; n++)
                mc.Biome [n] = 9;
			
            return mc;
        }
		
        public Hell()
        {
        }
		
        public override WorldSession Join(Client player)
        {
            ConstructSession cs = new HellSession(player);
            Join(cs, player);
            BadPlayer b = Banned.CheckBanned(player);
            if (b == null)
            {
                player.TellSystem(Chat.White, "Welcome to the world of banned players");
                player.TellSystem(Chat.White, "You are not banned so you can /return");
            } else
            {
                player.TellSystem(Chat.White, "You have been banned!");
                player.TellSystem(Chat.White, b.ToString());
            }
			
            UpdateHealth uh = new UpdateHealth(1, 0);
            player.Queue.Queue(uh);				
            player.Queue.Queue(new TimeUpdate(14000));
			
            player.Queue.Queue(new EntityEffect(player.EntityID, PlayerEffects.Blindness, 10, 20 * 60));

            return cs;
        }		
    }
}

