using System;
using MineProxy.Packets;

namespace MineProxy
{
    public class Mob : Entity
    {
        public Mob(int eid, MobType type) : base (eid)
        {
            this.Type = type;
        }
		
        public readonly MobType Type;
		
        public sbyte CreeperFuse { get; set; }

        public bool CreeperCharged { get; set; }
		
        /// <summary>
        /// Wolf and Ocelot
        /// </summary>
        public string Owner = "";
		
        /// <summary>
        /// Update local state of entity
        /// </summary>
        public void Update(SpawnMob spawn)
        {
            UpdateMeta(spawn.Metadata);			
        }
		
        protected override bool ImportMeta(int id, string val)
        {
            if (id == 17 && (Type == MobType.Wolf || Type == MobType.Ocelot))
            {
                Owner = val;
                return true; //Owner
            }
            return false;
        }

        protected override bool ImportMeta(int id, int val)
        {
            if (id == 12)
            {
                switch (Type)
                {
                    case MobType.Cow:
                    case MobType.Mooshroom:
                    case MobType.Hen:
                    case MobType.Ocelot:
                    case MobType.Sheep:
                    case MobType.Pig:
                    case MobType.Wolf:
                    case MobType.Villager:
                        Debug.Assert(val >= -24000 && val <= 6000);
                        Maturity = val;
                        return true;
                }
                return false;
            }
            if (id == 16 && Type == MobType.Villager)
            {
                //Debug.Assert (val == 0 || val == 4);
                return true; //Unknown, 0
            }
            if (id == 18 && Type == MobType.Wolf)
            {
                Debug.Assert(val >= 0 && val <= 20);
                return true; //Health, start at 8, 0 is never sent, dead by then
            }
            return false;			
        }

        protected override bool ImportMeta(int id, short val)
        {
            if (id == 16 && Type == MobType.EnderDragon)
            {
                Debug.Assert(val > 0 && val <= 200);
                return true; //Health, 200 == full
            }
            return false;			
        }

        protected override bool ImportMeta(int id, sbyte val)
        {
            switch (Type)
            {
                case MobType.Creeper: 
                    if (id == 16)
                    {
                        Debug.Assert(val >= -1 && val <= 1);
                        CreeperFuse = val;
                        return true;
                    }
                    if (id == 17)
                    {
                        if (val == 0)
                        {
                            CreeperCharged = false;
                            return true;
                        } 
                        if (val == 1)
                        {
                            CreeperCharged = true;
                            return true;
                        }
                    }
                    return false;
					
                case MobType.Spider:
                case MobType.CaveSpider:
                    if (id == 16)
                    {
                        Debug.Assert(val == 0 || val == 1 || val == -1);
                        return true;
                    }
                    return false;
                case MobType.Slime:
                case MobType.MagmaCube:
				//Size
                    if (id == 16)
                    {
                        Debug.Assert(val == 0 | val == 1 | val == 2 | val == 4);
                        return true;
                    }
                    return false;
                case MobType.Ghast:
				//Agression
                    if (id == 16)
                    {
                        Debug.Assert(val == 0 || val == 1);
                        return true;
                    }
                    return false;
                case MobType.Enderman:
                    if (id == 16) //Item in hand
                        return true;
                    if (id == 17)
                    {//Agression
                        Debug.Assert(val == 0);
                        return true;
                    }
                    return false;
                case MobType.Blaze:
				//Agression
                    if (id == 16)
                    {
                        Debug.Assert(val == 0 || val == 1);
                        return true;
                    }
                    return false;
                case MobType.IronGolem:
                    if (id == 16)
                    {
                        Debug.Assert(val == 1 || val == 0);
                        return true;
                    }
                    return false;			
                case MobType.Sheep:
				//0x10 ==sheared, 0x0F == color
                    if (id == 16)
                    {
                        Debug.Assert((val & 0xE0) == 0);
                        return true;
                    }
                    return false;
                case MobType.Wolf:
				//Flags: 1 sitting, 2 agressive, 4 tamed
                    if (id == 16)
                    {
                        Debug.Assert((val & 0xF8) == 0);
                        return true;
                    }
                    return false;
                case MobType.Ocelot:
                    if (id == 16)
                    {
                        Debug.Assert((val & 0xFA) == 0);
                        return true;
                    }
                    if (id == 18)
                    {
                        Debug.Assert(val == 0 || val == 3);
                        return true;
                    }
                    return false;
                case MobType.Pig:
                    if (id == 16)
                    {
                        Debug.Assert(val == 0 || val == 1 || val == -1);
                        return true; //1==saddle
                    }
                    return false;
            }
            return false;
        }
		
        protected override void ExportMeta(Metadata meta)
        {
            meta.SetByte(0, (sbyte)(
			         (Onfire ? 0x01 : 0) |
                (Crouched ? 0x02 : 0) |
                (Riding ? 0x04 : 0) |
                (Sprinting ? 0x08 : 0) |
                (Eating ? 0x10 : 0)
			         ));
            meta.SetInt(1, Oxygen);
            meta.SetInt(8, (Potion.R << 16) | (Potion.G << 8) | Potion.B);
            meta.SetInt(12, Maturity);
        }
		
        public override string ToString()
        {
            return "Mob: " + Type;
        }
    }
}

