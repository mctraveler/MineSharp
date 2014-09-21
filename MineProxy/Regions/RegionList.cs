using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MineProxy
{
    public partial class RegionList
    {
        public List<WorldRegion> List { get; set; }
    
        [JsonIgnore]
        public string FilePath { get; set; }

        public RegionList()
        {
            this.List = new List<WorldRegion>();
        }
    }
}

