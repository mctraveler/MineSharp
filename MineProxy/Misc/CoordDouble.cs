using System;
using System.Globalization;
using Newtonsoft.Json;

namespace MineProxy
{
    public partial class CoordDouble
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        /// <summary>
        /// Convert from nether coords to overworld coords
        /// </summary>
        public CoordDouble ToMapDimensions(Dimensions fromDim)
        {
            if (fromDim == Dimensions.Nether)
                return new CoordDouble(X * 8, Y, Z * 8);
            return Clone();
        }

        [JsonIgnore]
        public double Abs
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        /// <summary>
        /// Absolute value squared
        /// </summary>
        [JsonIgnore]
        public double Abs2
        {
            get
            {
                return X * X + Y * Y + Z * Z;
            }
        }

        public CoordDouble(string x, string y, string z)
        {
            this.X = double.Parse(x, System.Globalization.NumberFormatInfo.InvariantInfo);
            this.Y = double.Parse(y, System.Globalization.NumberFormatInfo.InvariantInfo);
            this.Z = double.Parse(z, System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public CoordDouble(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public CoordDouble()
        {
        }

        public static CoordDouble operator -(CoordDouble a, CoordDouble b)
        {
            if (a == null && b == null)
                return new CoordDouble();
            if (b == null)
                return a.Clone();
            if (a == null)
                return new CoordDouble(-b.X, -b.Y, -b.Z);
            return new CoordDouble(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static CoordDouble operator -(CoordInt a, CoordDouble b)
        {
            return new CoordDouble(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static CoordDouble operator +(CoordDouble a, CoordDouble b)
        {
            return new CoordDouble(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static CoordDouble operator -(CoordDouble a, CoordInt b)
        {
            return new CoordDouble(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static CoordDouble operator +(CoordDouble a, CoordInt b)
        {
            return new CoordDouble(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static CoordDouble operator /(CoordDouble a, double d)
        {
            return new CoordDouble(a.X / d, a.Y / d, a.Z / d);
        }

        public static CoordDouble operator *(CoordDouble a, double d)
        {
            return new CoordDouble(a.X * d, a.Y * d, a.Z * d);
        }

        public double Scalar(CoordDouble p)
        {
            return p.X * X + p.Y * Y + p.Z * Z;
        }

        public double DistanceTo(CoordDouble other)
        {
            if (other == null)
                return this.Abs;
			
            double dx = X - other.X;
            double dy = Y - other.Y;
            double dz = Z - other.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public double DistanceXZTo(CoordDouble other)
        {
            if (other == null)
                return this.Abs;
			
            double dx = X - other.X;
            double dz = Z - other.Z;
            return Math.Sqrt(dx * dx + dz * dz);
        }

        public CoordDouble Clone()
        {
            CoordDouble c = new CoordDouble();
            c.X = X;
            c.Y = Y;
            c.Z = Z;
            return c;
        }

        public CoordInt CloneInt()
        {
            CoordInt c = new CoordInt();
            c.X = (int)X;
            c.Y = (int)Y;
            c.Z = (int)Z;
            return c;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", X.ToString("0"), Y.ToString("0"), Z.ToString("0"));
        }

        public string ToString(string format)
        {
            return string.Format("({0}, {1}, {2})", X.ToString(format), Y.ToString(format), Z.ToString(format));
        }

        /// <summary>
        /// Clean format used in vanilla tp commands
        /// </summary>
        public string ToStringClean(string format)
        {
            return string.Format("{0} {1} {2}", X.ToString(format), Y.ToString(format), Z.ToString(format));
        }
    }
}

