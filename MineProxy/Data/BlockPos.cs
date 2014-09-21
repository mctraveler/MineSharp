using System;

namespace MineProxy.Data
{
    public class BlockPos : Block
    {
        public CoordInt Position { get; set; }

        public BlockPos(BlockID id, int meta, int light, CoordInt pos) : base(id, meta, light)
        {
            this.Position = pos;
        }

    }
}

