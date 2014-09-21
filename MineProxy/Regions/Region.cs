using System;

namespace MineProxy
{
    public class Region
    {
        public int MinX { get; set; }

        public int MaxX { get; set; }

        public int MinY { get; set; }

        public int MaxY { get; set; }
		
        public int MinZ { get; set; }

        public int MaxZ { get; set; }
		
        public Region()
        {
			
        }

        public override string ToString()
        {
            return Coords();
        }

        public string Coords()
        {
            return "([" + MinX + ", " + MaxX + "], [" + MinY + ", " + MaxY + "], [" + MinZ + ", " + MaxZ + "])";
        }

        public Region(int xMin, int xMax, int yMin, int yMax, int zMin, int zMax)
        {
            if (xMin > xMax || yMin > yMax || zMin > zMax)
                throw new FormatException("MUST: min <= max");
            this.MinX = xMin;
            this.MaxX = xMax;
            this.MinY = yMin;
            this.MaxY = yMax;
            this.MinZ = zMin;
            this.MaxZ = zMax;
        }
		
        public virtual bool Contains(CoordDouble p)
        {
            return p.X >= MinX && p.X < MaxX
                && p.Y >= MinY && p.Y < MaxY
                && p.Z >= MinZ && p.Z < MaxZ;
        }

        /// <summary>
        /// true if r is completely within this
        /// </summary>
        public virtual bool Cover(Region r)
        {
            if (r.MinX < MinX)
                return false;
            if (r.MinY < MinY)
                return false;
            if (r.MinZ < MinZ)
                return false;
            if (r.MaxX > MaxX)
                return false;
            if (r.MaxY > MaxY)
                return false;
            if (r.MaxZ > MaxZ)
                return false;
			
            return true;			
        }
		
        /// <summary>
        /// Milder comparer than Cover, only parts need to be overlapping to return true
        /// </summary>
        public virtual bool Overlap(Region r)
        {
            if (r.MinX >= MaxX)
                return false;
            if (r.MinY >= MaxY)
                return false;
            if (r.MinZ >= MaxZ)
                return false;
            if (r.MaxX <= MinX)
                return false;
            if (r.MaxY <= MinY)
                return false;
            if (r.MaxZ <= MinZ)
                return false;
			
            return true;			
        }
    }
}

