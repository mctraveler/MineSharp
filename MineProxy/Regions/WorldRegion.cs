using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MineProxy.Chatting;
using MineProxy.Worlds;
using MineProxy.Packets;

namespace MineProxy
{
    public partial class WorldRegion : Region
    {
        /// <summary>Greeting in chat when entering</summary>
        public string Message { get; set; }

        /// <summary>Notify the residents of players entering/leaving</summary>
        public bool ReportVisits { get; set; }

        public RegionStats Stats { get; set; }
        //Set when region is not going to be hit anymore with regioncrossing
        public bool Deleted;

        public string Name { get; set; }

        public List<string> Residents = new List<string>();
        public List<WorldRegion> SubRegions = new List<WorldRegion>();

        public int Dimension { get; set; }

        public string Type { get; set; }

        private WorldRegion()
        {
        }

        public WorldRegion(Dimensions dimension, CoordDouble start, CoordDouble end, string name)
            : this(start.X, end.X, start.Y, end.Y, start.Z, end.Z)
        {
            //Avoid zero height regions
            if (start.Y == end.Y)
                end.Y = start.Y + 1;
            
            Dimension = (int)dimension;
            this.Name = name;
        }

        public WorldRegion(double x1, double x2, double y1, double y2, double z1, double z2)
            : base((int)Math.Floor(Math.Min(x1, x2)), (int)Math.Ceiling(Math.Max(x1, x2)), (int)Math.Floor(Math.Min(y1, y2)), (int)Math.Ceiling(Math.Max(y1, y2)), (int)Math.Floor(Math.Min(z1, z2)), (int)Math.Ceiling(Math.Max(z1, z2)))
        {
            Dimension = 0;
            ReportVisits = true;
        }

        public WorldRegion(int x1, int x2, int y1, int y2, int z1, int z2)
            : base(Math.Min(x1, x2), Math.Max(x1, x2), Math.Min(y1, y2), Math.Max(y1, y2), Math.Min(z1, z2), Math.Max(z1, z2))
        {
            Dimension = 0;
            ReportVisits = true;
        }

        public override string ToString()
        {
            return Name + " " + Coords();
        }

        internal void TellResidentsSystem(Client player, string prefix, string message)
        {
            foreach (string username in Residents.ToArray())
            {
                if (player != null && username.ToLowerInvariant() == player.MinecraftUsername.ToLowerInvariant())
                    continue;
                Client p = PlayerList.GetPlayerByUsername(username);
                if (p != null)
                    p.TellSystem(prefix, message);
            }
        }

        /// <summary>
        /// Return true if player is resident or admin
        /// </summary>
        public bool ResidentPermissions(Client player)
        {
            if (player.Admin(Permissions.Region))
                return true;

            return IsResident(player.MinecraftUsername);
        }

        public bool IsResident(Client client)
        {
            return IsResident(client.MinecraftUsername);
        }

        bool IsResident(string username)
        {
            username = username.ToLowerInvariant();
            foreach (string u in Residents.ToArray())
            {
                if (u.ToLowerInvariant() == username)
                    return true;
            }
            return false;
        }

        public void ClearResidents()
        {
            Residents.Clear();
        }

        public int ResidentCount { get { return Residents.Count; } }

        /// <summary>
        /// Tell all players inside the region
        /// Return number of recipients
        /// </summary>
        internal int TellAll(string prefix, string message)
        {
            int total = 0;
            foreach (Client c in PlayerList.List)
            {
                if (c.Settings.Cloaked != null)
                    continue;
                WorldSession s = c.Session;
                if (s == null)
                    continue;
                if (s.CurrentRegion != this)
                    continue;
                c.TellChat(prefix, message);
                total += 1;
            }
            return total;
        }

        /// <summary>
        /// Return what direction the coord is in the region
        /// </summary>
        string FromDirection(CoordDouble pos)
        {
            //Determine Cardinal or spawn message
            double dx = (pos.X - (MaxX + MinX) / 2) / (MaxX - MinX);
            double dy = (pos.Y - (MaxY + MinY) / 2) / (MaxY - MinY);
            double dz = (pos.Z - (MaxZ + MinZ) / 2) / (MaxZ - MinZ);

            //string d = dx.ToString("0.00") + ", " + dy.ToString("0.00") + ", " + dz.ToString("0.00") + " ";

            double ax = Math.Abs(dx);
            double ay = Math.Abs(dy);
            double az = Math.Abs(dz);

            if (ax > ay && ax > az)
            {
                if (dx > 0)
                    return "in the East";
                else
                    return "in the West";
            }
            if (ay > ax && ay > az)
            {
                if (dy > 0)
                    return "Above";
                else
                    return "Below";
            }
            if (az > ax && az > ay)
            {
                if (dz > 0)
                    return "in the South";
                else
                    return "in the North";
            }

            return "-";
        }

        public string ColorName
        {
            get
            {
                string name = Name;
                if (Type == Honeypot.Type)
                    name = Color + name;
                if (Type == PublicType.Type || Type == "")
                    name = Color + name;
                if (Type == Adventure.Type)
                    name = Color + name;
                if (Type == Protected.Type)
                    name = Color + name;
                if (Type == WarZone.Type)
                    name = Color + name;
                if (Type == SpawnRegion.Type)
                    name = Color + name;
                return name;
            }
        }

        public string ColorNameType
        {
            get
            {
                string name = Name;
                if (Type == Honeypot.Type)
                    name = Color + name;
                if (Type == PublicType.Type || Type == "")
                    name = Color + name + " (public)";
                if (Type == Adventure.Type)
                    name = Color + name + " (adventure)";
                if (Type == Protected.Type)
                    name = Color + name + " (protected)";
                if (Type == WarZone.Type)
                    name = Color + name;
                if (Type == SpawnRegion.Type)
                    name = Color + name;
                return name;
            }
        }

        public string ColorType
        {
            get
            {
                if (Type == Honeypot.Type)
                    return Color + "Honeypot";
                if (Type == PublicType.Type || Type == "")
                    return Color + "Public";
                if (Type == Adventure.Type)
                    return Color + "Adventure";
                if (Type == Protected.Type)
                    return Color + "Protected";
                if (Type == WarZone.Type)
                    return Color + "War Zone";
                if (Type == SpawnRegion.Type)
                    return Color + "Spawn";
                return Color + Type;
            }
        }

        public string Color
        {
            get
            {
                if (Type == Honeypot.Type)
                    return Chat.Gold;
                if (Type == PublicType.Type || Type == "")
                    return Chat.Green;
                if (Type == Adventure.Type)
                    return Chat.Yellow;
                if (Type == Protected.Type)
                    return Chat.Purple;
                if (Type == WarZone.Type)
                    return Chat.Red;
                if (Type == SpawnRegion.Type)
                    return Chat.DarkGray;
                return Chat.Gray;
            }
        }

        internal virtual void Entering(Client player)
        {
            if (Type == SpawnRegion.Type)
            {
                SpawnRegion.Entering(player);
            }

            if (Type == SpawnTimeRegion.Type)
            {
                SpawnTimeRegion.Entering(player);
                return;
            }

            Log.WriteRegion(player, this, true);

            Regions.WarpPortalVisuals.EnterRegion(player, this);

            //Region message
            if (Message != null)
                player.TellAbove(Chat.DarkAqua + "[region] ", Message);
            else
                player.TellAbove(Chat.DarkAqua, "Entering " + Name);

            if (player.Settings.Cloaked != null)
                return;

            if (ReportVisits == false)
                return;

            string message = player.Name + " entered " + ColorName + " " + FromDirection(player.Session.Position);
#if DEBUG
            player.TellSystem("DEBUG " + Chat.Blue, message);
#endif
            TellResidentsSystem(player, Chat.Blue, message);
        }

        /// <summary>
        /// Leaving the specified player and forAnotherRegion.
        /// </summary>
        /// <param name='player'>
        /// Player.
        /// </param>
        /// <param name='forAnotherRegion'>
        /// If going into another region dont tell the player, leaving
        /// </param>
        internal virtual void Leaving(Client player, WorldRegion nextRegion)
        {
            if (Type == SpawnRegion.Type)
            {
                SpawnRegion.Leaving(player);
            }
            
            if (Type == SpawnTimeRegion.Type)
            {
                SpawnTimeRegion.Leaving(player);
                return;
            }

            Log.WriteRegion(player, this, false);
            if (nextRegion == null || nextRegion.SubRegions != null && nextRegion.SubRegions.Contains(this))
                player.TellAbove(Chat.Gray, "Leaving " + Name);

            if (player.Settings.Cloaked != null)
                return;
            if (ReportVisits == false)
                return;

            TellResidentsSystem(player, Chat.Gray, player.Name + " left " + Name + " " + FromDirection(player.Session.Position));
        }

        internal virtual bool FilterServer(WorldSession player, Packet packet)
        {
            if (player.Mode != GameMode.Creative)
            {
                if (Type == SpawnRegion.Type)
                    return SpawnRegion.FilterServer(player, packet);
                if (Type == SpawnTimeRegion.Type)
                    return SpawnTimeRegion.FilterServer(player, packet);
            }
            return false;
        }

        /// <summary>
        /// True if w is added to current region anywhere recursively
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance has child the specified w; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='w'>
        /// The width.
        /// </param>
        public bool HasChild(WorldRegion w)
        {
            foreach (WorldRegion r in SubRegions)
            {
                if (r == w)
                    return true;
                if (r.HasChild(w))
                    return true;
            }
            return false;
        }

        public override bool Cover(Region r)
        {
            WorldRegion w = r as WorldRegion;
            if (w != null && w.Dimension != Dimension)
                return false;
            return base.Cover(r);
        }

        public override bool Overlap(Region r)
        {
            WorldRegion w = r as WorldRegion;
            if (w != null && w.Dimension != Dimension)
                return false;
            return base.Overlap(r);
        }

        #region Residents

        internal void AddPlayer(Client player, string newResident)
        {
            RegionList regions = player.Session.World.Regions;

            if (IsResident(player) == false)
            {
                player.TellSystem(Chat.Yellow, " You are not a resident of this region");
                if (player.Admin(Permissions.Region) == false)
                    return;
            }
            
            AddPlayer(player.MinecraftUsername, newResident, regions);
            player.TellSystem(Chat.Aqua, newResident + " added to " + Name);
        }

        internal void AddPlayer(string residentPlayer, string newResident, RegionList region)
        {
            Residents.Add(newResident);
            RegionLoader.Save(region);
            Log.WritePlayer(residentPlayer, "Region added resident " + newResident);
            Client nr = PlayerList.GetPlayerByName(newResident);
            if (nr != null)
            {
                nr.TellSystem(Chat.Aqua, residentPlayer + " added you to " + Name);
                RegionCrossing.SetRegion(nr.Session);
            }
        }

        internal void Rename(Client player, string name)
        {
            RegionList region = player.Session.World.Regions;
            if (ResidentPermissions(player) == false)
            {
                player.TellSystem(Chat.Yellow, " You are not a resident of this region");
                return;
            }
            
            Log.WritePlayer(player, "Region renamed " + Name + " to " + name);
            TellResidentsSystem(null, Chat.Blue, player.Name + " renamed " + Name + " to " + name);
            Name = name;
            RegionLoader.Save(region);
            Entering(player);
        }

        internal void SetMessage(Client player, string message)
        {
            RegionList regions = player.Session.World.Regions;

            if (ResidentPermissions(player) == false)
            {
                player.TellSystem(Chat.Yellow, " You are not a resident of this region");
                return;
            }

            if (message == "-" || message == "off" || message == "rem" || message == "remove")
                message = null;
            Message = message;
            Log.WritePlayer(player, "Message for " + Name + " set to " + Message);
            RegionLoader.Save(regions);

            TellResidentsSystem(null, Chat.Blue, player.Name + " changed region message to " + Message);
        }

        public void Resize(int minX, int maxX, int minY, int maxY, int minZ, int maxZ, Client player)
        {
            RegionList regions = player.Session.World.Regions;

            Region test = new Region(minX, maxX, minY, maxY, minZ, maxZ);
            WorldRegion parent = RegionCrossing.GetParentRegion(regions.List, this);
            if (parent == null)
            {
                player.TellSystem(Chat.Red, "parent not found");
                return;
            }

            if (player.Admin(Permissions.Region) == false)
            {
                if ((Donors.IsDonor(player) == false))
                {
                    player.TellSystem(Chat.Aqua, "Only for donors and admins may resize a region");
                    return;
                }
                //Useless since when only donors get this far:
                if (minY < 50 && (!player.Donor))
                {
                    player.TellSystem(Chat.Red, "Only admins and donors may make regions below Y=50");
                    return;
                }
                if (ResidentPermissions(player) == false)
                {
                    player.TellSystem(Chat.Yellow, "You are not a resident of this region");
                    return;
                }

                if (parent.ResidentPermissions(player) == false)
                {
                    player.TellSystem(Chat.Yellow, "You are not a resident of the parent region");
                    return;
                }
            }

            List<WorldRegion> list;
            if (parent == this)
                list = player.Session.World.Regions.List;
            else
                list = parent.SubRegions;

            //Make sure the new size overlaps the old one so we don't make huge mistakes
            if (test.Overlap(this) == false)
            {
                player.TellSystem(Chat.Red, "New size must overlap old one");
                player.TellSystem(Chat.Red, "New size " + test);
                player.TellSystem(Chat.Red, "Old size " + this.Coords());
                return;
            }

            //Check that the new size fits in the parent
            if (parent != this)
            {
                if (parent.Cover(test) == false)
                {
                    player.TellSystem(Chat.Red, "parent " + parent.Name + " is too small " + parent.Coords());
                    return;
                }
            }
            //else we are in the top level, no limit there
            
            //Make sure new size does not collide with siblings
            foreach (WorldRegion w in list)
            {
                if (w.Dimension != Dimension) //If toplevel "siblings" are all toplevel regions
                    continue;
                if (w == this)
                    continue;
                if (w.Overlap(test))
                {
                    player.TellSystem(Chat.Red, "new size overlap sibling " + w);
                    return;
                }
            }
            
            //Chech that subregions still fit into the new size
            if (SubRegions != null)
            {
                foreach (WorldRegion w in SubRegions)
                {
                    if (test.Cover(w) == false)
                    {
                        player.TellSystem(Chat.Red, "New size does not cover subregion:");
                        player.TellSystem(Chat.Red, w.ToString());
                        return;
                    }
                }
            }
            
            Log.WritePlayer(player, "Region Resized: from " + this + " to " + test);
            MinX = test.MinX;
            MaxX = test.MaxX;
            MinY = test.MinY;
            MaxY = test.MaxY;
            MinZ = test.MinZ;
            MaxZ = test.MaxZ;
            RegionLoader.Save(regions);
            player.TellSystem(Chat.Purple, "Region resized: " + this);
        }

        /// <summary>
        /// Change the region protection type
        /// </summary>
        public void SetType(Client player, string type)
        {
            if (ResidentPermissions(player) == false)
            {
                player.TellSystem(Chat.Yellow, " You are not a resident of this region");
                return;
            }

            type = type.ToLowerInvariant().Trim();

            switch (type)
            {
                case "": //public
                case PublicType.Type:
                case Protected.Type:
                case Adventure.Type:
                case SpawnRegion.Type:
                    break; //OK
                default:
                    if (player.Admin(Permissions.Region) == false)
                    {
                        player.TellSystem(Chat.Red, "Unknown type: " + type);
                        player.TellSystem(Chat.Yellow, "Choose one of: public, protected, adventure, night");
                        return;
                    }
                    else
                        player.TellSystem(Chat.Yellow, "Custom type: " + type);
                    break;
            }
            Type = type;
            player.TellSystem(Chat.Aqua, "Region type is now: " + type);
            RegionLoader.Save(player.Session.World.Regions);

            //Update all players
            foreach (Client c in PlayerList.List)
            {
                if (c.Session.CurrentRegion == this)
                    RegionCrossing.SetRegion(c.Session);
            }
        }

        internal void RenameByMineControl(string name, RegionList regions)
        {
            Log.WritePlayer("Mine Control", "Region renamed " + Name + " to " + name);
            TellResidentsSystem(null, Chat.Blue, "Server renamed " + Name + " to " + name);
            Name = name;
            RegionLoader.Save(regions);
        }

        internal void RemovePlayer(Client player, string removeUsername)
        {
            RegionList regions = player.Session.World.Regions;

            if (ResidentPermissions(player) == false)
            {
                Log.WritePlayer(player, "Failed to remove " + removeUsername + " from region " + Name);
                player.TellSystem(Chat.Yellow, " You are not a resident of this region");
                return;
            }
            
            removeUsername = removeUsername.ToLowerInvariant();
            foreach (string s in Residents)
            {
                if (s.ToLowerInvariant() == removeUsername)
                {
                    removeUsername = s;
                    break;
                }
            }
            if (Residents.Remove(removeUsername) == false)
            {
                player.TellSystem(Chat.Red, removeUsername + " not found in region " + Name);
                return;
            }
            
            Log.WritePlayer(player, "Region: removed resident " + removeUsername);
            RegionLoader.Save(regions);
            player.TellSystem(Chat.Aqua, removeUsername + " removed from " + Name);
            Client p = PlayerList.GetPlayerByName(removeUsername);
            if (p != null && p != player)
                p.TellSystem(Chat.Aqua, player.Name + " removed you from " + Name);
        }

        internal void SetReport(Client player, string state)
        {
            RegionList regions = player.Session.World.Regions;

            if (ResidentPermissions(player) == false)
            {
                player.TellSystem(Chat.Yellow, " You are not a resident of this region");
                return;
            }

            if (state == "on")
                ReportVisits = true;
            if (state == "off")
                ReportVisits = false;
            RegionLoader.Save(regions);
        }

        public string GetResidents()
        {
            string r = "";
            foreach (string s in Residents.ToArray())
            {
                r += s + ", ";
            }
            return r;
        }

        #endregion

        #region Static helpers

        /// <summary>
        /// return the list "region" is located in
        /// </summary>
        public static List<WorldRegion> GetParentList(RegionList regions, WorldRegion region)
        {
            return GetParentList(regions.List, region);
        }

        static List<WorldRegion> GetParentList(List<WorldRegion> list, WorldRegion region)
        {
            if (list == null)
                return null;

            foreach (WorldRegion r in list)
            {
                if (r.Dimension != region.Dimension)
                    continue;

                if (r.Overlap(region) == false)
                    continue;   
                
                if (r == region)
                    return list;

                //Search subregions
                return GetParentList(r.SubRegions, region);
            }
            return null;
        }

        #endregion

    }
}

