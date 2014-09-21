using System;
using System.Collections.Generic;
using MineProxy.Plugins;
using MineProxy.Regions;
using MineProxy.Worlds;

namespace MineProxy
{
    public static class RegionCrossing
    {
        
        /// <summary>
        /// Find region and send message
        /// </summary>
        internal static void SetRegion(WorldSession session)
        {
            RegionList regions = session.World.Regions;
            if (regions == null)
                return;

            try
            {
                //This is called every single player movement, 20 times a second, some optimization might be needed.
                WorldRegion w = null;
                //First test if we are in the same as before
                WorldRegion current = session.CurrentRegion;
                if (current != null)
                {
                    if (current.Deleted == false && current.Contains(session.Position))
                    {
                        w = GetRegion(current.SubRegions, session.Position, session.Dimension);
                        if (w == null)
                            w = current;
                    }
                }
                if (w == null)
                    w = GetRegion(regions.List, session.Position, session.Dimension);
            
                //Debug.Write("Setregion: " + w);

                //If different
                if (session.CurrentRegion != w)
                {
                    //Leaving
                    if (session.CurrentRegion != null && (session.CurrentRegion.HasChild(w) == false))
                        session.CurrentRegion.Leaving(session.Player, w);

                    //Entering
                    if (w != null && w.HasChild(session.CurrentRegion) == false)
                        w.Entering(session.Player);

                    //Stats
                    SetStats(session.CurrentRegion, session.Player);
                    SetStats(w, session.Player);
                }
                //Adjust Survival/Adventure mode
                if (session.Mode != GameMode.Creative &&
                    session.Mode != GameMode.Spectator)
                {
                    bool protect = Protected.IsBlockProtected(session, w);
                    if (protect && w.IsResident(session.Player))
                        protect = false;
                    
                    if (protect)
                        session.SetMode(GameMode.Adventure);
                    else
                        session.SetMode(GameMode.Survival);
                }

                if (w != null && w.Type == WarpZone.Type)
                {
                    if (session.Mode != GameMode.Creative)
                    {
                        WarpZone wz = new WarpZone(w.Name);
                        session.Player.Warp(wz.Destination, (Dimensions)wz.DestinationDimension, wz.DesinationWorld);
                    }
                }

                bool update = (w != session.CurrentRegion);
                session.CurrentRegion = w;
                if (update)
                {
                    ScoreboardRegionManager.UpdateRegion(session.Player);
                }
#if !DEBUG
            } catch (Exception e)
            {
                Log.WriteServer(e);
                return;
#endif
            } finally
            {
            }
        }

        static void SetStats(WorldRegion r, Client player)
        {
            if (r == null)
                return;
            if (r.Stats == null)
                r.Stats = new RegionStats();
            r.Stats.LastVisit = DateTime.UtcNow;
            if (r.IsResident(player))
                r.Stats.LastVisitResident = DateTime.UtcNow;
        }

        public static WorldRegion GetRegion(CoordDouble pos, Dimensions dimension, RegionList regions)
        {
            if (regions == null)
                return null;
            return GetRegion(regions.List, pos, dimension);
        }
        
        public static WorldRegion GetRegion(List<WorldRegion> list, CoordDouble pos, Dimensions dimension)
        {
            if (list == null)
                return null;

            foreach (WorldRegion r in list)
            {
                if (r.Dimension != (int)dimension)
                    continue;
                if (r.Contains(pos))
                {
                    if (r.SubRegions == null) 
                        return r;

                    WorldRegion w = GetRegion(r.SubRegions, pos, dimension);
                    if (w == null)
                        return r;
                    else
                        return w;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Return best matching parent region for our new region or return an interferring region if found
        /// Check for Cover to make sure the returning region is not crossing edges
        /// </summary>
        public static WorldRegion GetBaseRegion(List<WorldRegion> list, Region reg, Dimensions dimension)
        {
            foreach (WorldRegion r in list)
            {
                if ((int)dimension != r.Dimension)
                    continue;
                if (reg.Overlap(r) == false)
                    continue;   
                
                if (r.Cover(reg) == false)
                    return r; // we will catch this later
                        
                if (r.SubRegions == null) 
                    return r;

                WorldRegion w = GetBaseRegion(r.SubRegions, reg, dimension);
                        
                if (w == null)
                    return r;
                else
                    return w;
            }
            return null;
        }
        
        //Return null if no match is found
        //Return current if the list is the parent
        //Return the parent region if found
        public static WorldRegion GetParentRegion(List<WorldRegion> list, WorldRegion current)
        {
            foreach (WorldRegion r in list)
            {
                if (r.Dimension != current.Dimension)
                    continue;
                
                if (current.Overlap(r) == false) 
                    continue;   
                
                if (r == current)
                    return r;   //Signal that the parent is the list holder
                        
                if (r.SubRegions == null) 
                    throw new InvalidOperationException("Expected subregions");

                WorldRegion w = GetParentRegion(r.SubRegions, current);
                        
                if (w == current)
                    return r;   //return true parent
                if (w != null)
                    return w; //return parent found in sub
                //if null continue
            }
            return null;
        }
        
        /// <summary>
        /// Make sure the region can be resized into the new region size
        /// </summary>
        
        /// <summary>
        /// For when the player is leaving the region without crossing the border, such as logging out
        /// Find region and send leave message
        /// </summary>
        internal static void ClearRegion(Client player)
        {
            try
            {
                if (player.Session.CurrentRegion != null)
                    player.Session.CurrentRegion.Leaving(player, null);

                player.Session.CurrentRegion = null;
#if !DEBUG
            } catch (Exception e)
            {
                Log.WriteServer(e);
                return;
#endif
            } finally
            {
            }
        }
    }
}

