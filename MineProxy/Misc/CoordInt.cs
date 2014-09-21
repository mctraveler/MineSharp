using System;
using System.Globalization;
using Newtonsoft.Json;
using MiscUtil.IO;

namespace MineProxy
{
    public class CoordInt
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        public static CoordInt Read(EndianBinaryReader r)
        {
            return Decode(r.ReadInt64());
        }

        public static CoordInt Decode(long val)
        {
            var p = new CoordInt();
            p.X = (int)((val >> 38) & 0x3FFFFFF);
            p.Y = (int)((val >> 26) & 0xFFF);
            p.Z = (int)(val & 0x3FFFFFF);
            return p;
        }

        public void Write(EndianBinaryWriter w)
        {
            w.Write((long)Encode());
        }

        public long Encode()
        {
            return  (((long)X & 0x3FFFFFF) << 38) | (((long)Y & 0xFFF) << 26) | ((long)Z & 0x3FFFFFF);
        }

        public CoordInt Modulus(int i)
        {
            return new CoordInt(X % i, Y % i, Z % i);
        }

        [JsonIgnore]
        public double Abs
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        public CoordInt SizeTo(CoordInt other)
        {
            if (other == null)
                return this.Clone();
            
            int dx = X - other.X;
            int dy = Y - other.Y;
            int dz = Z - other.Z;
            return new CoordInt(
                Math.Abs(dx) + 1,
                Math.Abs(dy) + 1,
                Math.Abs(dz) + 1);
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

        public double DistanceTo(CoordInt other)
        {
            if (other == null)
                return this.Abs;
            
            double dx = X - other.X;
            double dy = Y - other.Y;
            double dz = Z - other.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public double DistanceToXZ(CoordInt other)
        {
            if (other == null)
                return this.Abs;
            
            double dx = X - other.X;
            double dz = Z - other.Z;
            return Math.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>
        /// Termvis multiplikation
        /// </summary>
        public CoordInt TermProd(CoordInt o)
        {
            return new CoordInt(X * o.X, Y * o.Y, Z * o.Z);
        }

        public CoordInt(string x, string y, string z)
        {
            this.X = int.Parse(x, System.Globalization.NumberFormatInfo.InvariantInfo);
            this.Y = int.Parse(y, System.Globalization.NumberFormatInfo.InvariantInfo);
            this.Z = int.Parse(z, System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public CoordInt(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public CoordInt()
        {
        }

        public static CoordInt operator +(CoordInt a, CoordInt b)
        {
            return new CoordInt(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static CoordInt operator -(CoordInt a, CoordInt b)
        {
            return new CoordInt(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static CoordInt operator *(CoordInt a, int b)
        {
            return new CoordInt(a.X * b, a.Y * b, a.Z * b);
        }

        public static CoordInt operator /(CoordInt a, int b)
        {
            return new CoordInt(a.X / b, a.Y / b, a.Z / b);
        }

        public static bool operator <(CoordInt a, CoordInt b)
        {
            if (a.X >= b.X)
                return false;
            if (a.Y >= b.Y)
                return false;
            if (a.Z >= b.Z)
                return false;
            return true;
        }

        public static bool operator >(CoordInt a, CoordInt b)
        {
            if (a.X <= b.X)
                return false;
            if (a.Y <= b.Y)
                return false;
            if (a.Z <= b.Z)
                return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            CoordInt b = obj as CoordInt;
            if (b == null)
                return false;			
            if (X != b.X)
                return false;
            if (Y != b.Y)
                return false;
            if (Z != b.Z)
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public CoordInt Clone()
        {
            CoordInt c = new CoordInt();
            c.X = X;
            c.Y = Y;
            c.Z = Z;
            return c;
        }

        public CoordDouble CloneDouble()
        {
            CoordDouble c = new CoordDouble();
            c.X = X;
            c.Y = Y;
            c.Z = Z;
            return c;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", X, Y, Z);
        }

        public CoordInt Offset(Face face)
        {
            CoordInt c = this.Clone();

            switch (face)
            {
                case Face.Down:
                    c.Y--;
                    break;
                case Face.Up:
                    c.Y++;
                    break;
                case Face.North:
                    c.Z--;
                    break;
                case Face.South:
                    c.Z++;
                    break;
                case Face.West:
                    c.X--;
                    break;
                case Face.East:
                    c.X++;
                    break;
            }
            return c;
        }

        /// <summary>
        /// Return true for the "null" coordinate: (-1,-1,-1)
        /// </summary>
        public bool IsNull()
        {
            if (X != -1)
                return false;
            //Since Mojang only keep the Y in a single byte
            if (Y != 255 && Y != -1)
                return false;
            if (Z != -1)
                return false;
            return true;					
        }

        public static CoordInt MinComponent(CoordInt a, CoordInt b)
        {
            CoordInt c = new CoordInt();
            c.X = Math.Min(a.X, b.X);
            c.Y = Math.Min(a.Y, b.Y);
            c.Z = Math.Min(a.Z, b.Z);
            return c;
        }

        public static CoordInt MaxComponent(CoordInt a, CoordInt b)
        {
            CoordInt c = new CoordInt();
            c.X = Math.Max(a.X, b.X);
            c.Y = Math.Max(a.Y, b.Y);
            c.Z = Math.Max(a.Z, b.Z);
            return c;
        }

        public static CoordInt AbsComponent(CoordInt a)
        {
            CoordInt c = new CoordInt();
            c.X = Math.Abs(a.X);
            c.Y = Math.Abs(a.Y);
            c.Z = Math.Abs(a.Z);
            return c;
        }
    }
}

