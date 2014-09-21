using System;

namespace MineProxy.NBT
{
    public class PlayerDat
    {
        private TagCompound tc;
        public short SleepTimer;
        public double[] Motion = new double[3];
        public byte OnGround;
        public short HurtTime;
        public short Health;
        public int Dimension;
        public short Air;
        public SlotItem[] InventoryWear = new SlotItem[4];
        public SlotItem[] InventoryCraft = new SlotItem[4];
        /// <summary>
        /// 0-35
        /// </summary>
        public SlotItem[] Inventory = new SlotItem [36];
        public CoordDouble Pos = new CoordDouble();
        public short AttackTime;
        public byte Sleeping;
        public short Fire;
        public float FallDistance;
        public float[] Rotation = new float[2];
        public short DeathTime;
        public CoordInt Spawn = null;
        public float foodExhaustionLevel;
        public int foodTickTimer;
        public float foodSaturationLevel;
        public int foodLevel;
        public int XpLevel;
        public int XpTotal;
        public int Xp;
        public float XpP;
        public int playerGameType;
		
        public PlayerDat(Tag tag)
        {
            if (tag == null)
            {
                tc = new TagCompound();
                return;
            }
			
            tc = (TagCompound)tag;
			
            foreach (var tp in tc.CompoundList)
            {
                Tag v = tp.Value;
                switch (tp.Key)
                {
                    case "SleepTimer":
                        SleepTimer = v.Short;
                        break;
                    case "Motion":
                        Motion [0] = ((TagList<TagDouble>)v) [0].Double;
                        Motion [1] = ((TagList<TagDouble>)v) [1].Double;
                        Motion [2] = ((TagList<TagDouble>)v) [2].Double;
                        break;
                    case "OnGround":
                        OnGround = v.Byte;
                        break;
                    case "HurtTime":
                        HurtTime = v.Short;
                        break;
                    case "Health":
                        Health = v.Short;
                        break;
                    case "Dimension":
                        Dimension = v.Int;
                        break;
                    case "Air":
                        Air = v.Short;
                        break;
                    case "Inventory":
                        if (v is TagList<TagCompound>)
                        {
                            foreach (TagCompound ti in ((TagList<TagCompound>)v))
                            {
                                byte slot = ti ["Slot"].Byte;
                                SlotItem item = new SlotItem((BlockID)ti ["id"].Short, ti ["Count"].Byte, ti ["Damage"].Short);

                                if (100 <= slot && slot < 104)
                                {
                                    InventoryWear [slot - 100] = item;
                                }
                                if (80 <= slot && slot < 84)
                                {
                                    InventoryCraft [slot - 80] = item;
                                }
                                if (0 <= slot && slot < 36)
                                {
                                    Inventory [slot] = item;
                                }
                            }
                        } else if ((v is TagList<TagByte>) == false)
                        {
                            Console.Error.WriteLine("Player.dat: WARNING: No inventory");
                            Console.Error.WriteLine("Inventory: " + v.GetType() + "\t" + v);
                        }
                        break;
                    case "Pos":
                        Pos.X = ((TagList<TagDouble>)v) [0].Double;
                        Pos.Y = ((TagList<TagDouble>)v) [1].Double;
                        Pos.Z = ((TagList<TagDouble>)v) [2].Double;
                        break;
                    case "AttackTime":
                        AttackTime = v.Short;
                        break;
                    case "Sleeping":
                        Sleeping = v.Byte;
                        break;
                    case "Fire":
                        Fire = v.Short;
                        break;
                    case "FallDistance":
                        FallDistance = v.Float;
                        break;
                    case "Rotation":
                        Rotation [0] = ((TagList<TagFloat>)v) [0].Float;
                        Rotation [1] = ((TagList<TagFloat>)v) [1].Float;
                        break;
                    case "DeathTime":
                        DeathTime = v.Short;
                        break;
                    case "SpawnX":
                        if (Spawn == null)
                            Spawn = new CoordInt();
                        Spawn.X = v.Int;
                        break;
                    case "SpawnY":
                        if (Spawn == null)
                            Spawn = new CoordInt();
                        Spawn.Y = v.Int;
                        break;
                    case "SpawnZ":
                        if (Spawn == null)
                            Spawn = new CoordInt();
                        Spawn.Z = v.Int;
                        break;
                    case "foodExhaustionLevel":
                        foodExhaustionLevel = v.Float;
                        break;
                    case "foodTickTimer":
                        foodTickTimer = v.Int;
                        break;
                    case "foodSaturationLevel":
                        foodSaturationLevel = v.Float;
                        break;
                    case "foodLevel":
                        foodLevel = v.Int;
                        break;
                    case "XpLevel":
                        XpLevel = v.Int;
                        break;
                    case "XpTotal":
                        XpTotal = v.Int;
                        break;
                    case "Xp": //not used anymore
					//Debug.Assert (false);
                        Xp = v.Int;
                        break;
                    case "XpP":
                        XpP = v.Float;
                        break;
                    case "playerGameType":
                        playerGameType = v.Int;
                        break;
                    case "abilities":
					//TODO: booleans
					//flying, instabuild, mayfly, invulnerable
                        break;
                    case "EnderItems":
                        //A list
                        break;
                    case "Attributes":
                        //A list
                        break;
                    case "SelectedItemSlot":
                        //integer
                        break;
                    case "UUIDLeast"://long
                        break;
                    case "UUIDMost"://long
                        break;
                    case "HealF"://float
                        break;
                    case "AbsorptionAmount"://float
                        break;
                    case "SpawnForced": //bool
                        break;
                    case "Score": //int
                        break;
                    case "PortalCooldown": //int
                        break;
                    case "Invulnerable": //bool
                        break;
#if DEBUG
                    default:
                        Console.WriteLine("Unknown: " + tp.Key + ": " + v);
                        throw new NotImplementedException();
#endif
                }				
            }
        }

        public Tag ExportTag()
        {
            tc ["SleepTimer"] = new TagShort(SleepTimer);

            TagList<TagDouble > motion = new TagList<TagDouble>();
            motion [0] = new TagDouble(Motion [0]);
            motion [1] = new TagDouble(Motion [1]);
            motion [2] = new TagDouble(Motion [2]);
            tc ["Motion"] = motion;
            tc ["OnGround"] = new TagByte(){Byte = OnGround};
            tc ["HurtTime"] = new TagShort(HurtTime);
            tc ["Health"] = new TagShort(Health);

            tc ["Dimension"] = new TagInt(Dimension);
            tc ["Air"] = new TagShort(Air);
			
            if (tc ["Inventory"] is TagList<TagCompound> == false)
            {
                tc ["Inventory"] = new TagList<TagCompound>();
            }
            TagList<TagCompound > inv = tc ["Inventory"] as TagList<TagCompound>;
			
            for (byte n = 0; n < 104; n++)
            {
                SlotItem item = null;
                if (n < 36)
                    item = Inventory [n];
                if (n >= 80 && n < 84)
                    item = InventoryCraft [n - 80];
                if (n >= 100)
                    item = InventoryWear [n - 100];

                TagCompound ti = null;
				
                //Find slot item
                foreach (TagCompound itc in inv)
                {
                    if (itc ["Slot"].Byte == n)
                    {
                        ti = itc;
                        break;
                    }
                }
				
                if (item == null)
                {
                    if (ti != null)
                        inv.Remove(ti);
                    continue;
                }
                if (ti == null)
                {
                    ti = new TagCompound();
                    inv.Add(ti);
                }
				
                ti ["id"] = new TagShort((short)item.ItemID);
                ti ["Damage"] = new TagShort((short)item.Uses);
                ti ["Count"] = new TagByte((byte)item.Count);
                ti ["Slot"] = new TagByte(n);
            }
            inv.Sort((x, y) => x ["Slot"].Byte - y ["Slot"].Byte);

            TagList<TagDouble > p = new TagList<TagDouble>();
            p [0] = new TagDouble(Pos .X);
            p [1] = new TagDouble(Pos .Y);
            p [2] = new TagDouble(Pos .Z);
            tc ["Pos"] = p;
            tc ["AttackTime"] = new TagShort(AttackTime);
            tc ["Sleeping"] = new TagByte(Sleeping);
            tc ["Fire"] = new TagShort(Fire);
            tc ["FallDistance"] = new TagFloat(FallDistance);
            TagList<TagFloat > rot = new TagList<TagFloat>();
            rot [0] = new  TagFloat(Rotation [0]);
            rot [1] = new  TagFloat(Rotation [1]);
            tc ["Rotation"] = rot;
            tc ["DeathTime"] = new TagShort(DeathTime);

            if (Spawn != null)
            {
                tc ["SpawnX"] = new TagInt(Spawn.X);
                tc ["SpawnY"] = new TagInt(Spawn.Y);
                tc ["SpawnZ"] = new TagInt(Spawn.Z);
            }
			
            tc ["foodExhaustionLevel"] = new TagFloat(foodExhaustionLevel);
            tc ["foodTickTimer"] = new TagInt(foodTickTimer);
            tc ["foodSaturationLevel"] = new TagFloat(foodSaturationLevel);
            tc ["foodLevel"] = new TagInt(foodLevel);
            tc ["XpLevel"] = new TagInt(XpLevel);
            tc ["XpTotal"] = new TagInt(XpTotal);
            tc ["Xp"] = new TagInt(Xp);
            tc ["playerGameType"] = new TagInt(playerGameType);
			
            return tc;
        }
    }
}

