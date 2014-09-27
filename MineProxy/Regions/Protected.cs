using System;
using MineProxy.Chatting;
using MineProxy.Worlds;
using MineProxy.Regions;
using MineProxy.Packets;

namespace MineProxy
{
    /// <summary>
    /// Helper class for Worldregions starting with
    /// </summary>
    public static class Protected
    {
        public const string Type = "protected";

        public static bool IsBlockProtected(WorldSession session, WorldRegion r)
        {
            if (r == null)
                return false;

            if (session.Mode == GameMode.Creative && session.Player.Admin())
                return false;
            if (r.IsResident(session.Player))
                return false;

            switch (r.Type)
            {
                case Protected.Type:
                case Preserved.Type:
                case Adventure.Type:
                case Honeypot.Type:
                case SpawnRegion.Type:
                case WarpZone.Type:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Prevent block modification, aplies to all, residents should never get called here
        /// </summary>
        public static bool ProtectBlockBreak(VanillaSession session, WorldRegion region, PlayerDigging d)
        {
            if (IsBlockProtected(session, region) == false)
                return false;

            //Block single click from outside the adventure mode
            if (d.Status == PlayerDigging.StatusEnum.StartedDigging)
                return true;
            
            if (d.Status != PlayerDigging.StatusEnum.FinishedDigging)
                return false;

            //Old method - not needed
            BlockChange bc = new BlockChange(d.Position, BlockID.Bedrock);
            session.Player.SendToClient(bc);

            //New method - Works
            /*
            PlayerDigging pd = new PlayerDigging();
            pd.Position = d.Position;
            pd.Face = Face.Down;
            pd.Status = PlayerDigging.StatusEnum.CheckBlock;
            session.FromClient(pd);
            */

            return true;
        }

        public static bool ProtectBlockPlace(VanillaSession session, WorldRegion region, PlayerBlockPlacement bp)
        {
            if (IsBlockProtected(session, region) == false)
                return false;

            if (FilterFire(session, bp))
            {
                string i = "";
                if (session.ActiveItem != null)
                    i += "(server)" + session.ActiveItem.ItemID;
                if (bp.Item != null)
                    i += "(client)" + bp.Item.ItemID;

                Chatting.Parser.TellAdmin(session.Player.Name + " tried to use " + i + " in " + region.Name + " at " + session.Position);
                //session.Player.Kick("Arson");
                return true;
            }

            //If adventure protection is active, allow all actions
            if (session.Mode == GameMode.Adventure) //buggy but best bet so far
                return false; //ok, only activate levers and chests

            //Block all placements, non resident must be inside the region
            session.TellSystem(Chat.Aqua, "move inside the region before you open a door or chest");

            //New method, works - no longer works
            /*
            PlayerDigging pd = new PlayerDigging();
            pd.Position = bp.BlockPosition.Offset(bp.FaceDirection);
            pd.Face = Face.Down;
            pd.Status = PlayerDigging.StatusEnum.CheckBlock;
            session.FromClient(pd);
            */

            //Old method - not perfect, disabling:
            /*BlockChange bc = new BlockChange(bp.BlockPosition.Offset(bp.FaceDirection), BlockID.Air);
            session.Player.SendToClient(bc);*/
            return true;
        }

        static bool BlockClick(Client player, WindowClick wc)
        {
            player.SendToClient(new ConfirmTransactionServer(wc, false));
            return true;
        }

        /// <summary>
        /// Return true to block
        /// </summary>
        public static bool ProtectChestsClick(VanillaSession rs, WindowClick wc)
        {
            if (wc.WindowID == 0) //player inventory, allow
                return false;

            if (rs.Player.Admin() && rs.Mode == GameMode.Creative)
                return false;

            if (rs.OpenWindows.ContainsKey(wc.WindowID) == false)
            {
                rs.TellSystem(Chat.Red, "Window not open");
                return BlockClick(rs.Player, wc);
            }
            var w = rs.OpenWindows [wc.WindowID];

            if (w.Region == null)
                return false; //not in a region, allow

            switch (w.Region.Type)
            {
                case Protected.Type:
                case Honeypot.Type:
                    break;
                default:
                    return false;
            }

            if (w.Region.IsResident(rs.Player))
                return false; //residents allow

            //Allow some open types
            if (w.Window.InventoryType == "CraftingTable")
                return false;
            if (w.Window.InventoryType == "Enchant")
                return false;
            if (w.Window.InventoryType == "Anvil")
                return false;
            if (w.Window.WindowTitle == "container.enderchest")
                return false;

            if (w.Region.Type == Protected.Type)
                return BlockClick(rs.Player, wc);

            if (w.Region.Type == Honeypot.Type)
            {
                if (wc.Slot == 11 || wc.Slot == 13 || wc.Slot == 15)
                {
                    if (rs.Player.Admin())
                    {
                        rs.TellSystem(Chat.White, "This spot is protected");
                        return false;
                    }
                    Honeypot.Trigger(rs.Player, w.Region);
                    return true;
                }
            }

            return true;
        }

        static bool FilterFire(VanillaSession session, PlayerBlockPlacement placement)
        {
            //Prevent fire, lava and TNT
            SlotItem i = session.ActiveItem;

            //A bit safer but not correctly implemented need to consider WindowClick
            if (i != null)
            {
                if (i.ItemID == BlockID.FlintandSteel
                    || i.ItemID == BlockID.Fireball
                    || i.ItemID == BlockID.Lava
                    || i.ItemID == BlockID.TNT
                    || i.ItemID == BlockID.Lavabucket
                    || i.ItemID == BlockID.CartWithTNT)
                {
                    return true;
                }
            }

            //Safer but forgable client side
            i = placement.Item;
            if (i != null)
            {
                if (i.ItemID == BlockID.FlintandSteel
                    || i.ItemID == BlockID.Fireball
                    || i.ItemID == BlockID.Lava
                    || i.ItemID == BlockID.TNT
                    || i.ItemID == BlockID.Lavabucket)
                {
                    return true;
                }
            }

            return false;
        }

    }
}

