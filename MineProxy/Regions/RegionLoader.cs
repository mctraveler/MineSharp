using System;
using System.IO;
using System.Collections.Generic;
using MineProxy.Chatting;
using MineProxy.Regions;

namespace MineProxy
{
    public static class RegionLoader
    {
        public static RegionList Load(string regionPath)
        {
            RegionList r = Json.Load<RegionList>(regionPath);

            if (r == null)
                r = new RegionList();
            r.FilePath = regionPath;
            Update(r.List);
            return r;
        }

        static void Update(List<WorldRegion> list)
        {
            foreach (var r in list)
            {
                if (r.Stats == null)
                    r.Stats = new RegionStats();
                Update(r.SubRegions);
            }
        }

        public static void Save(RegionList regions)
        {
            if (regions == null)
                return;

            lock (regions)
            {
                //Remove all empty regions
                List<WorldRegion > rem = new List<WorldRegion>();
                foreach (WorldRegion w in regions.List)
                    if (Empty(w))
                        rem.Add(w);
                foreach (WorldRegion r in rem)
                    regions.List.Remove(r);
			
                //Only save WorldREgion no static regions
                RegionList saveList = new RegionList();
                foreach (WorldRegion w in regions.List)
                {
                    if (w.GetType() == typeof(WorldRegion))
                        saveList.List.Add(w);
                }

                Json.Save(regions.FilePath, saveList);

                MineProxy.Regions.WarpPortalVisuals.RegionsUpdated(regions.List);
            }
        }

        static bool Empty(WorldRegion r)
        {
            //To be removed
            List<WorldRegion > rem = new List<WorldRegion>();
			
            //Empty subregi ons
            foreach (WorldRegion wr in r.SubRegions)
            {
                if (Empty(wr))
                    rem.Add(wr);
            }
            foreach (WorldRegion tr in rem)
                r.SubRegions.Remove(tr);
			
            return r.Residents.Count == 0 && r.SubRegions.Count == 0;
        }

        public static void CleanBanned(RegionList regions)
        {
            lock (regions)
            {
                int rem = CleanBanned(regions.List);
                Save(regions);
                Chatting.Parser.TellAdmin(Permissions.Ban, "Removed banned residents: " + rem);
            }
        }

        static int CleanBanned(List<WorldRegion> list)
        {
            int removed = 0;

            foreach (WorldRegion w in list)
            {
                var rem = new List<string>();

                foreach (string res in w.Residents)
                {
                    var b = Banned.CheckBanned(res);
                    if (b == null)
                        continue;
                    if ((b.BannedUntil - DateTime.Now).TotalDays < 30)
                        continue;

                    rem.Add(res);
                }
                removed += rem.Count;

                foreach (var r in rem)
                    w.Residents.Remove(r);

                removed += CleanBanned(w.SubRegions);
            }
            return removed;
        }
    }
}

