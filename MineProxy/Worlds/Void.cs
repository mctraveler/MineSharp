using System;
using MineProxy.Packets;

namespace MineProxy.Worlds
{
    /// <summary>
    /// this dimention is the equivalent to null when the client is about to disconnect
    /// </summary>
    public class Void : World
    {
        public override WorldSession Join(Client player)
        {
            return new VoidSession(player);
        }
		
        class VoidSession : WorldSession
        {
            public override World World { get { return World.Void; } }
			
            public override string ShortWorldName { get { return "Void"; } }

            public VoidSession(Client player) : base(player)
            {
            }
			
            public override void Kill()
            {
            }
		
            public override void Close(string message)
            {
            }
		
            public override void FromClient(PacketFromClient packet)
            {
            }
        }
    }
}

