using System;
using MineProxy.Chatting;
using MineProxy.Packets;

namespace MineProxy.Worlds
{
    public class TheConstruct : Construct
    {
        public TheConstruct()
        {
        }

        protected override Dimensions Dimension { get { return MineProxy.Dimensions.Overworld; } }

        protected override ChunkData GenerateChunk()
        {
            ChunkData mc = new ChunkData();
            mc.Complete = true;
            mc.InitContinousChunks(4);
            //Fill with data
            int off = mc.GetIndex(0, 0, 0);
            for (int i = off; i < off + 64 * 16 * 16; i++)
                mc.BlockType [i] = (byte)BlockID.IronBlock;
            for (int n = 0; n < mc.BlockLight.Length; n++)
                mc.BlockLight [n] = 0xff;
            for (int n = 0; n < mc.BlockSkyLight.Length; n++)
                mc.BlockSkyLight [n] = 0x00;
            for (int n = 0; n < mc.Biome.Length; n++)
                mc.Biome [n] = 9;

            return mc;
        }

        public override WorldSession Join(Client player)
        {
            ConstructSession cs = new TheConstructSession(player);
            Join(cs, player);
            player.Queue.Queue(new TimeUpdate(6000));
            player.TellSystem(Chat.White, "Welcome to the Construct");
            player.TellSystem(Chat.White, "Type: " + Chat.Blue + "/blue" + Chat.White + " to return");
            player.Queue.Queue(new PlayerAbilitiesServer());
            player.Queue.Queue(ChangeGameState.ChangeGameMode(GameMode.Creative));
            return cs;
        }
    }
}

