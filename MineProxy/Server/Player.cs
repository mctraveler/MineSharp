using System;
using MineProxy.Packets;

namespace MineProxy
{
	public class Player : Entity
	{
        public Guid ID { get; set; }
		
        public Player (int eid, Guid id) : base(eid)
		{
            this.ID = id;
		}
		
		public void Update (SpawnPlayer spawn)
		{
		}
	}
}

