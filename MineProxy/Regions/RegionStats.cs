using System;

namespace MineProxy
{
    public partial class RegionStats
    {
        /// <summary>Last time visited by any player</summary>
        public DateTime LastVisit { get; set; }

        /// <summary>Last time visited by a resident, in UTC</summary>
        public DateTime LastVisitResident { get; set; }
    }
}

