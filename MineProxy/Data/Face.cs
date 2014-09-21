using System;

namespace MineProxy
{
    public enum Face
    {
        None = 255,
        Down = 0,  //-Y
        Up = 1,    //+Y
        North = 2, //-Z
        South = 3,  //+Z
        West = 4, //-X
        East = 5,  //+X	
    }

    public static class FaceExt
    {
        /// <summary>
        /// return the new f mirrored in mirror
        /// </summary>
        public static Face MirrorSurface(this Face f, Face mirror)
        {
            int mi = (int)mirror;
            int fi = (int)f;

            int mh = mi / 2;
            int fh = fi / 2;

            //No need to mirror
            if (mh != fh)
                return f;

            fi = fi ^ 1;
            return (Face)fi;
        }

        /// <summary>
        /// Return the normal vector
        /// </summary>
        /// <param name="f">F.</param>
        public static CoordInt Normal(this Face f)
        {
            switch (f)
            {
                case Face.Up:
                    return new CoordInt(0, 1, 0);
                case Face.Down:
                    return new CoordInt(0, -1, 0);
                case Face.East:
                    return new CoordInt(1, 0, 0);
                case Face.West:
                    return new CoordInt(-1, 0, 1);
                case Face.North:
                    return new CoordInt(0, 0, -1);
                case Face.South:
                    return new CoordInt(0, 0, 1);
            }
            return null;
        }

    }
}

