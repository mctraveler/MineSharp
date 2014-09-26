using System;
using System.Collections.Generic;
using MineProxy.Plugins;
using MineProxy.Regions;
using MineProxy.Commands;
using MineProxy.Packets;

namespace MineProxy.Commands
{
    public class RegionCommands : CommandManager
    {
        readonly Dictionary<string, CoordDouble> regionStart = new Dictionary<string, CoordDouble>();

        public RegionCommands(CommandManager c)
        {
            c.AddTab(RegionTab, "region", "reg");
            c.AddCommand(RegionCommand, "region", "reg", "faction", "f");

            AddTab(TabSet, "set");
            AddTab(TabRename, "rename", "ren");
            AddTab(TabMessage, "message");
            AddTab(TabResize, "resize", "res");

            AddCommand(SetRegionStart, "start", "create");
            AddCommand(Set, "set");
            AddCommand(Rename, "rename", "ren");
            AddCommand(Message, "message");
            AddCommand(Type, "type");
            AddCommand(AddResident, "add");
            AddCommand(RemoveResident, "remove", "rem");
            AddCommand(Resize, "resize", "res");
            AddCommand(Report, "report");
            AddCommand(ShowSidebar, "show", "on", "off");

            AddAdminCommand(Delete, "delete", "del");
        }

        void RegionTab(Client player, string[] cmd, int iarg, TabComplete tab)
        {
            base.ParseTab(player, cmd, iarg + 1, tab);
        }

        void RegionCommand(Client player, string[] cmd, int iarg)
        {
            if (cmd.Length == iarg)
            {
                //Single /reg, no arguments
                WorldRegion region = player.Session.CurrentRegion;
                if (region == null)
                    throw new ErrorException("No region here");

                    
                player.TellSystem(Chat.Aqua, "Region: " + region.ColorName);
                if (player.Admin())
                {
                    player.TellSystem(Chat.Gray, region.Type + ": " + region);
                }   
                player.TellSystem(Chat.DarkAqua + "Residents: ", region.GetResidents());
                player.TellSystem(Chat.DarkAqua + "Last visited by resident: ", (DateTime.UtcNow - region.Stats.LastVisitResident).TotalDays.ToString("0.0") + " days ago");
                return;
            }

            if (base.ParseCommand(player, cmd, iarg + 1) == false)
                throw new ShowHelpException();
        }

        void TabRename(Client player, string[] cmd, int iarg, TabComplete tab)
        {
            var r = player.Session.CurrentRegion;

            if (r == null)
                player.TellSystem(Chat.Red, "No region here");
            else
                tab.Alternatives.Add(player.Session.CurrentRegion.Name);
            return;
        }

        void TabMessage(Client player, string[] cmd, int iarg, TabComplete tab)
        {
            var r = player.Session.CurrentRegion;
            if (r == null)
                player.TellSystem(Chat.Red, "No region here");
            else
                tab.Alternatives.Add(player.Session.CurrentRegion.Message);
        }

        void TabResize(Client player, string[] cmd, int iarg, TabComplete tab)
        {
            var r = player.Session.CurrentRegion;
            if (r == null)
                player.TellSystem(Chat.Red, "No region here");
            else
                tab.Alternatives.Add(r.MinX + " " + r.MaxX + " " + r.MinY + " " + r.MaxY + " " + r.MinZ + " " + r.MaxZ);
        }

        void TabSet(Client player, string[] cmd, int iarg, TabComplete tab)
        {
            if (regionStart.ContainsKey(player.MinecraftUsername))
            {
                int a = (int)regionStart [player.MinecraftUsername].Y;
                int b = (int)player.Session.Position.Y;
                if (a > b)
                {
                    int t = a;
                    a = b;
                    b = t;
                }
                tab.Alternatives.Add(a + " " + b);
            } else
            {
                tab.Alternatives.Add("50 256");
            }
        }

        void ShowSidebar(Client player, string[] cmd, int iarg)
        {
            if (cmd.Length <= iarg)
                throw new ShowHelpException();

            string setting = cmd [iarg].ToLowerInvariant();

            if (setting == "off")
            {
                player.Settings.ShowRegionScoreboard = false;
                player.Score = null;
                player.TellSystem(Chat.Purple, "Region reporting off");
                return;
            }
            if (setting == "on")
            {
                player.Settings.ShowRegionScoreboard = true;
                ScoreboardRegionManager.UpdateRegion(player);
                player.TellSystem(Chat.Purple, "Region reporting on");
                return;
            }
        }

        void Resize(Client player, string[] cmd, int iarg)
        {
            WorldRegion region = player.Session.CurrentRegion;

            if (region == null)
                throw new ErrorException("No region here");

            player.TellSystem(Chat.Gray, "Resizing: " + region);
            if (cmd.Length != 8)
                throw new ShowHelpException();

            region.Resize(int.Parse(cmd [2]), int.Parse(cmd [3]), int.Parse(cmd [4]), int.Parse(cmd [5]), int.Parse(cmd [6]), int.Parse(cmd [7]), player);
            ScoreboardRegionManager.UpdateAllPlayersRegion();
        }

        void Set(Client player, string[] cmd, int iarg)
        {
            int ymin, ymax;
            if (cmd.Length == iarg + 2)
            {
                if (int.TryParse(cmd [iarg], out ymin) == false)
                    throw new ErrorException("argument not an integer");
                if (int.TryParse(cmd [iarg + 1], out ymax) == false)
                    throw new ErrorException("argument not an integer");
            } else if (cmd.Length == iarg)
            {
                ymin = 50;
                ymax = 256;
            } else
                throw new ShowHelpException();
            SetRegionEnd(player, ymin, ymax);
            RegionCrossing.SetRegion(player.Session);
        }

        void Rename(Client player, string[] cmd, int iarg)
        {
            WorldRegion region = player.Session.CurrentRegion;
            if (region == null)
                throw new ErrorException("No region here");

            if (cmd.Length < 3)
                throw new ErrorException("Too few arguments");

            region.Rename(player, cmd.JoinFrom(2));
            ScoreboardRegionManager.UpdateAllPlayersRegion();
        }

        void Message(Client player, string[] cmd, int iarg)
        {
            WorldRegion region = player.Session.CurrentRegion;
            if (region == null)
                throw new ErrorException("No region here");

            if (cmd.Length < 3)
                throw new ErrorException("Too few arguments");

            region.SetMessage(player, cmd.JoinFrom(2));
            return;

        }

        void Type(Client player, string[] cmd, int iarg)
        {
            WorldRegion region = player.Session.CurrentRegion;
            if (region == null)
                throw new ErrorException("No region here");

            if (cmd.Length == 2)
            {
                player.TellSystem(Chat.Purple, "Region is of type " + region.Type);
                return;
            }
            region.SetType(player, cmd.JoinFrom(2));
            ScoreboardRegionManager.UpdateAllPlayersRegion();
            return;

        }

        void AddResident(Client player, string[] cmd, int iarg)
        {
            WorldRegion region = player.Session.CurrentRegion;
            if (region == null)
                throw new ErrorException("No region here");

            if (cmd.Length != 3)
                throw new ErrorException("Too few arguments");
            region.AddPlayer(player, cmd [2]);
            ScoreboardRegionManager.UpdateAllPlayersRegion();
            return;

        }

        void RemoveResident(Client player, string[] cmd, int iarg)
        {
            WorldRegion region = player.Session.CurrentRegion;
            if (region == null)
                throw new ErrorException("No region here");

            if (cmd.Length != 3)
                throw new ErrorException("Too few arguments");

            region.RemovePlayer(player, cmd [2]);
            ScoreboardRegionManager.UpdateAllPlayersRegion();
            return;

        }

        void Report(Client player, string[] cmd, int iarg)
        {
            WorldRegion region = player.Session.CurrentRegion;
                
            if (region == null)
                throw new ErrorException("No region here");


            if (cmd.Length == 3)
                region.SetReport(player, cmd [2].ToLowerInvariant());

            if (region.ReportVisits)
                player.TellSystem(Chat.Yellow, "Reporting is on");
            else
                player.TellSystem(Chat.Yellow, "Reporting is off");
            return;

        }

        static void DeleteRecursive(WorldRegion r)
        {
            r.Residents.Clear();
            r.SubRegions.Clear();
        }

        static void DeleteSingle(WorldRegion r, RegionList regions)
        {
            lock (regions)
            {
                var parent = WorldRegion.GetParentList(regions, r);
                if (parent == null)
                    return;

                //Move all subregions to parent list
                if (r.SubRegions != null)
                {
                    foreach (var s in r.SubRegions)
                        parent.Add(s);
                }

                r.Residents.Clear();
                r.SubRegions.Clear();
            }
            RegionLoader.Save(regions);
        }

        static void DeleteSubregions(WorldRegion r)
        {
            r.SubRegions.Clear();
        }

        void SetRegionStart(Client player, string[] cmd, int iarg)
        {
            if (Donors.IsDonor(player) == false && player.Uptime.TotalDays < 2 && (player.Admin() == false))
            {
                player.TellSystem(Chat.Red, "You must have been playing here at least 48 hours before you can create custom regions");
                player.TellSystem(Chat.Gray, "Use " + Chat.Yellow + "/ticket region" + Chat.Gray + " to create your first region");
                player.TellSystem(Chat.Gold, "See /donate, if you donate you can make one immediately.");
                return;
            }
            
            regionStart.Remove(player.MinecraftUsername);
            regionStart.Add(player.MinecraftUsername, player.Session.Position.Clone());
            player.TellSystem(Chat.Aqua, "Region start set at " + player.Session.Position.CloneInt());
            player.TellSystem(Chat.Aqua, "Go to other corner and type:");
            player.TellSystem("", "    /region set [ymin] [ymax]");
        }

        void SetRegionEnd(Client player, int ymin, int ymax)
        {
            if (regionStart.ContainsKey(player.MinecraftUsername) == false)
                throw new UsageException("You must type /region start");
            if (ymin > ymax)
                throw new ErrorException(ymin + " must be less than " + ymax);
            if (ymin < 50 && (!player.Admin()) && (!player.Donor))
                throw new ErrorException("Only admins and donors may make regions below Y=50");
            if (player.Session.Position.Y + 1 < ymin)
                throw new ErrorException("You are below the region you tried to create, move up and run the command again");

            if (ymax > 256)
                ymax = 256;

            if (player.Session.Dimension == Dimensions.End)
                throw new ErrorException("No regions in The End");

            CoordDouble start = regionStart [player.MinecraftUsername];
            CoordDouble end = player.Session.Position.Clone();
            
            start.Y = ymin;
            end.Y = ymax;

            CreateRegion(player, start, end);
        }

        /// <summary>
        /// Return true if region was created
        /// </summary>
        void CreateRegion(Client player, CoordDouble start, CoordDouble end)
        {
            RegionList regions = player.Session.World.Regions;
            try
            {
                WorldRegion wr = Create(player.Session.Dimension, start, end, player.Name + "'s place", player, player.MinecraftUsername, regions);
                if (wr == null)
                    return;

                regionStart.Remove(player.MinecraftUsername);

                Log.WritePlayer(player, "Region Created: " + wr);
                player.TellSystem(Chat.Aqua, "Region created");
                return;
            } catch (InvalidOperationException ioe)
            {
                player.TellSystem(Chat.Red, ioe.Message);
                return;
            }
        }

        internal static WorldRegion Create(Dimensions dimension, CoordDouble start, CoordDouble end, string regionName, Client player, string resident, RegionList regions)
        {
            if (regions == null)
                throw new ErrorException("No regions in this world");

            WorldRegion w = new WorldRegion(dimension, start, end, regionName);
            w.Residents.Add(resident);
            w.Type = "protected";

            lock (regions)
            {
                //Test for overlapping regions
                WorldRegion existing = RegionCrossing.GetBaseRegion(regions.List, w, dimension);
                if (existing == null)
                {
                    //New Top level region
                    regions.List.Add(w);
                    RegionLoader.Save(regions);
                    return w;
                } 

                //Make sure you already are a part of the region
                if (existing.ResidentPermissions(player) == false)
                    throw new ErrorException("You can't set a subregion unless you are a resident in the parent region.");

                //All inside, make subregion
                if (existing.Cover(w))
                {
                    if (existing.SubRegions == null)
                        existing.SubRegions = new List<WorldRegion>();
                    existing.SubRegions.Add(w);
                    RegionLoader.Save(regions);
                    return w;
                }
                //Need to make a wrapping region

                //Only admins may create wrapping regions
                if (player != null && player.Admin() == false)
                    throw new ErrorException("New region must be fully inside " + existing);

                //New region covering several old ones?
                var parentList = WorldRegion.GetParentList(regions, existing);
                if (parentList == null)
                    throw new ErrorException("Parent list not found for " + existing);

                //regions to move inside the new one
                var moving = new List<WorldRegion>();

                //Determine if we are breaking any boundaries
                bool breaking = false;
                foreach (var s in parentList)
                {
                    if (w.Cover(s))
                    {
                        moving.Add(s);
                    } else if (w.Overlap(s)) //Overlap but not cover completely
                    {
                        player.TellSystem(Chat.Red, "Breaking boundaries: " + s);
                        breaking = true;
                    }
                }
                if (breaking)
                {
                    player.TellSystem(Chat.Red, "Failed creating: " + w);
                    player.TellSystem(Chat.Red, "Aborting: Broke region boundaries");
                    return null;
                }

                //Move subregions into new region and remove from parentlist
                w.SubRegions = moving;
                foreach (var s in moving)
                {
                    player.TellSystem(Chat.Aqua, "New subregion: " + s);
                    parentList.Remove(s);
                }
                parentList.Add(w);
                RegionLoader.Save(regions);
                return w;
            }
        }

        void Delete(Client player, string[] cmd, int iarg)
        {
            if (player.Admin() == false)
                throw new ErrorException("Disabled");
            WorldRegion region = player.Session.CurrentRegion;
            if (region == null)
                throw new ErrorException("No region here");

            if (cmd.Length <= iarg)
                throw new ShowHelpException(); //Need more arguments

            RegionList regions = player.Session.World.Regions;

            string subdel = cmd [iarg].ToLowerInvariant();
            switch (subdel)
            {
                case "single":
                    DeleteSingle(region, regions);
                    player.TellSystem(Chat.Purple, region.Name + " removed");
                    player.Session.CurrentRegion = null;
                    break;
                case "recursive":
                    DeleteRecursive(region);
                    player.TellSystem(Chat.Purple, region.Name + " and subregions removed");
                    region.Deleted = true;
                    player.Session.CurrentRegion = null;
                    break;
                case "subonly":
                    DeleteSubregions(region);
                    player.TellSystem(Chat.Purple, "Subregions of " + region.Name + " removed");                        
                    break;
                default:
                    throw new ShowHelpException();
            }
            RegionLoader.Save(regions);
            
            //Update all players regions
            foreach (var p in PlayerList.List)
                RegionCrossing.SetRegion(p.Session);
            
            ScoreboardRegionManager.UpdateAllPlayersRegion();
        }
    }
}

