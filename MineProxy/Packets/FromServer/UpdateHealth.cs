using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class UpdateHealth : PacketFromServer
    {
        public const byte ID = 0x06;

        public override byte PacketID { get { return ID; } }
		
        /// <summary>
        /// dead if <= 0, 20 == full
        /// </summary>
        /// <value>
        /// The health.
        /// </value>
        public float Health { get; set; }
		
        /// <summary>
        /// 0 - 20
        /// </summary>
        public int Food { get; set; }
		
        /// <summary>
        /// Between 0.0 and 5.0.
        /// Food overcharge that will decrease all the way to 0 before Food will decrease
        /// </summary>
        public float FoodSaturation { get; set; }
		
        public override string ToString()
        {
            return string.Format("[UpdateHealth: Health={1}, Food={2}, {3}]", PacketID, Health, Food, FoodSaturation);
        }

        /// <summary>
        /// Set healt or kill if 0.
        /// </summary>
        /// <param name='health'>
        /// dead if <= 0, 20 == full
        /// </param>
        /// <param name='food'>
        /// 0 - 20
        /// </param>
        public UpdateHealth(int health, int food)
        {
            this.Health = (short)health;
            this.Food = food;
            this.FoodSaturation = 5.0f;
        }
		
        public UpdateHealth()
        {
            
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Health = r.ReadSingle();
            Food = ReadVarInt(r);
            FoodSaturation = r.ReadSingle();
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((float)Health);
            WriteVarInt(w, Food);
            w.Write((float)FoodSaturation);
        }
    }
}

