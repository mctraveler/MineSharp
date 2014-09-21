using System;
using MineProxy.Packets.Plugins;

namespace MineProxy.Commands
{
    public class TexturePackSetter
    {
        public static void Init(CommandManager c)
        {
            c.AddCommand(GetTexture, "texturepack");
        }

        static void GetTexture(Client player, string[] cmd, int iarg)
        {
            if (MinecraftServer.TexturePack == null || MinecraftServer.TexturePack == "")
                throw new ErrorException("We currenlty don't have any texturepack");

            var tp = new TexturePackMessage(MinecraftServer.TexturePack);
            player.Queue.Queue(tp);
            player.TellSystem(Chat.Blue, "Texturepack sent... If you did not get a prompt you already have it installed");

            player.TellSystem(Chat.Blue, "You can also download it from " + tp.Url);
        }
    }
}

