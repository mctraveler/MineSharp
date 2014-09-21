using System;

namespace MineProxy
{
    public class Block
    {
        public BlockID ID { get; set; }
        public int Meta { get; set; }
        public int Light { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, Light={2}]", ID, Meta, Light);
        }

        public Block(BlockID id, int meta, int light)
        {
            this.ID = id;
            this.Meta = meta;
            this.Light = light;
        }

        public Block(SlotItem s)
        {
            if (s == null)
            {
                ID = BlockID.Air;
                return;
            }
            ID = s.ItemID.ToBlock();
            Meta = s.Uses;
            Light = Block.LightLevel((byte)ID);
        }

        public static bool IsTransparent(byte id)
        {
            switch (id)
            {
                case (byte)BlockID.Air:
                case (byte)BlockID.Sapling:
                case (byte)BlockID.Water:
                case (byte)BlockID.Stationarywater:
                case (byte)BlockID.Lava:
                case (byte)BlockID.Stationarylava:
                case (byte)BlockID.Leaves:
                case (byte)BlockID.Glass:
                case (byte)BlockID.BedBlock:
                case (byte)BlockID.PoweredRail:
                case (byte)BlockID.DetectorRail:
                case (byte)BlockID.Cobweb:
                case (byte)BlockID.TallGrass:
                case (byte)BlockID.DeadShrubs:
                case (byte)BlockID.Piston:
                case (byte)BlockID.PistonExtension:
                case (byte)BlockID.BlockmovedbyPiston:
                case (byte)BlockID.Dandelion:
                case (byte)BlockID.Rose:
                case (byte)BlockID.BrownMushroom:
                case (byte)BlockID.RedMushroom:
                case (byte)BlockID.Slabs:
                case (byte)BlockID.OakWoodSlab:
                case (byte)BlockID.Torch:
                case (byte)BlockID.Fire:
                case (byte)BlockID.MonsterSpawner:
                case (byte)BlockID.WoodenStairsOak:
                case (byte)BlockID.RedstoneWire:
                case (byte)BlockID.SeedsBlock:
                case (byte)BlockID.SignPostI:
                case (byte)BlockID.WoodenDoorBlock:
                case (byte)BlockID.Ladders:
                case (byte)BlockID.Rails:
                case (byte)BlockID.CobblestoneStairs:
                case (byte)BlockID.WallSign:
                case (byte)BlockID.Lever:
                case (byte)BlockID.StonePressurePlate:
                case (byte)BlockID.IronDoorBlock:
                case (byte)BlockID.WoodenPressurePlate:
                case (byte)BlockID.RedstoneTorchOff:
                case (byte)BlockID.RedstoneTorchOn:
                case (byte)BlockID.StoneButton:
                case (byte)BlockID.Snow:
                case (byte)BlockID.Ice:
                case (byte)BlockID.Cactus:
                case (byte)BlockID.SugarCaneBlock:
                case (byte)BlockID.Fence:
                case (byte)BlockID.NetherPortal:
                case (byte)BlockID.CakeBlock:
                case (byte)BlockID.RedstoneRepeaterOff:
                case (byte)BlockID.RedstoneRepeaterOn:
                case (byte)BlockID.Trapdoor:
                case (byte)BlockID.IronBars:
                case (byte)BlockID.GlassPane:
                case (byte)BlockID.MelonPlant:
                case (byte)BlockID.PumpkinPlant:
                case (byte)BlockID.Vines:
                case (byte)BlockID.FenceGate:
                case (byte)BlockID.BrickStairs:
                case (byte)BlockID.StoneBrickStairs:
                case (byte)BlockID.Mycelium:
                case (byte)BlockID.LilyPad:
                case (byte)BlockID.NetherFence:
                case (byte)BlockID.NetherStairs:
                case (byte)BlockID.NetherWartBlock:
                case (byte)BlockID.EnchantmentTable:
                case (byte)BlockID.BrewingStandBlock:
                case (byte)BlockID.CauldronBlock:
                case (byte)BlockID.EndPortal:
                case (byte)BlockID.EndPortalFrame:
                case (byte)BlockID.DragonEgg:
                case (byte)BlockID.Beacon:
                case (byte)BlockID.GlassStained:
                case (byte)BlockID.GlassPaneStained:
                    return true;
                default:
                    return false;
            }
        }

        public static byte LightLevel(byte id)
        {
            switch ((BlockID)id)
            {
                case BlockID.Beacon:
                case BlockID.EndPortal:
                case BlockID.Fire:
                case BlockID.GlowstoneBlock:
                case BlockID.JackOLantern:
                case BlockID.Lava:
                case BlockID.Stationarylava:
                case BlockID.LanternLit:
                    return 15;
                case BlockID.Torch:
                    return 14;
                case BlockID.BurningFurnace:
                    return 13;
                case BlockID.NetherPortal:
                    return 11;
                case BlockID.RedstoneRepeaterOn:
                    return 9;
                case BlockID.EnderChest:
                case BlockID.RedstoneTorchOn:
                    return 7;
                case BlockID.BrewingStandBlock:
                case BlockID.MushroomBlock:
                case BlockID.DragonEgg:
                case BlockID.EndPortalFrame:
                    return 1;
                default:
                    return 0;
            }
        }

    }
}

