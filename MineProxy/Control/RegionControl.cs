using System;

namespace MineProxy.Control
{
    public partial class RegionControl
    {
        public enum RegionCommand
        {
            Create = 1,
            Delete = 2,
            Rename = 3,
            Resize = 4,
            AddPlayer = 10,
            RemovePlayer = 11,
        }

        public MineProxy.Control.RegionControl.RegionCommand Command { get; set; }

        public MineProxy.CoordDouble Start { get; set; }

        public int Dimension { get; set; }

        public MineProxy.CoordDouble End { get; set; }

        public string RegionName { get; set; }

        public string Username { get; set; }
    }
}

