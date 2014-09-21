using System;
using MineProxy.Packets;

namespace MineProxy
{
	public class Vehicle : Entity
	{
		public readonly Vehicles Type;
		
		public Vehicle (int eid, Vehicles type) : base(eid)
		{
			this.Type = type;
		}
		
		public void Update (SpawnObject spawn)
		{
		}
	}
}

