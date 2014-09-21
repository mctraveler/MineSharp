using System;
using MineProxy.Data;
using MineProxy.Worlds;
using MineProxy.Packets;

namespace MineProxy
{
    public static class PlayerInteraction
    {
        public static void Prod(Client sender, Client to)
        {
            Prod(to);
            to.TellChat(Chat.Green, sender.Name + " gave you a friendly push");
            sender.TellChat(Chat.Green, "You gave " + to.Name + " a friendly push");
			
            VanillaSession rs = to.Session as VanillaSession;
            if (rs != null)
            {
                //Sender see hurt
                sender.Queue.Queue(new EntityStatus(rs.EID, EntityStatuses.EntityHurt));
            }
        }

        public static void Prod(Client to)
        {
            //Target hurt
            to.Queue.Queue(new EntityStatus(to.EntityID, EntityStatuses.EntityHurt));
        }
    }
}

