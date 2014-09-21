using System;
using System.Drawing;
using MineProxy;
using MineProxy.NBT;
using MineProxy.Control;
using System.IO;
using System.IO.Compression;

using MiscUtil.IO;
using MiscUtil.Conversion;

namespace MineControl
{
    public class TiledMapPainter
    {
        static Font font = new Font("Verdana", 12, GraphicsUnit.Pixel);
        const int regionSize = 16 * 32; //chunks(32) blocks/chunk(16)
        const int chunkSize = 16; //chunks(32) blocks/chunk(16)

        static int lastCX = -100;
        static int lastCY = -100;
        static McaFile lastReg = null;
		
        public static void Paint(Graphics g, MapControl map, int Width, int Height, CoordDouble center, float scale)
        {
            if (map.Dimension > Dimensions.End)
                return;
			
            CoordDouble topleft = center - new CoordDouble(Width / 2 * scale, 0, Height / 2 * scale);
			
            //Determine center region
            int rx = (int)center.X & -512;
            int rz = (int)center.Z & -512;
            //Sub region chunk index
            int scx = (((int)center.X) >> 4) & 0x1F;
            int scz = (((int)center.Z) >> 4) & 0x1F;
            //Region changed
            if (rx != lastCX || rz != lastCY)
            {
                lastCX = rx;
                lastCY = rz;
                //Load region
                //lastReg = McaFile.Load(map.Dimension, rx, rz);
            }
			
            //..
            if (scale < 1)
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            else
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;


            float drawSize = regionSize / scale - 1;

            int xStart = (int)Math.Floor(topleft.X / regionSize) * regionSize;
            int xEnd = (int)Math.Ceiling((topleft.X + Width * scale) / regionSize) * regionSize;
            int yStart = (int)Math.Floor(topleft.Z / regionSize) * regionSize;
            int yEnd = (int)Math.Ceiling((topleft.Z + Height * scale) / regionSize) * regionSize;
	
            for (int x = xStart; x <= xEnd; x += regionSize)
            {
                for (int z = yStart; z <= yEnd; z += regionSize)
                {
                    float px = ((x - (float)topleft.X) / scale);
                    float py = ((z - (float)topleft.Z) / scale);
					
                    if (lastReg != null)
                    {
                        if (x == rx && z == rz)
                        {
                            int subSize = (int)(drawSize / 32) - 1;
                            for (int dx = 0; dx < 32; dx++)
                            {
                                for (int dz = 0; dz < 32; dz++)
                                {
                                    float pdx = ((x + (dx << 4) - (float)topleft.X) / scale);
                                    float pdy = ((z + (dz << 4) - (float)topleft.Z) / scale);
    				
                                    if (lastReg.HasChunk(dx, dz))
                                    {
                                        if (dx == scx && dz == scz)
                                        {
                                            //Highlighted
                                            g.FillRectangle(Brushes.Orange, pdx, pdy, subSize, subSize);
                                            McaChunk c = lastReg.chunks [dx, dz];
                                            g.DrawString(c.ToString(), font, Brushes.Black, pdx, pdy);
                                        } else
                                            g.FillRectangle(Brushes.Green, pdx, pdy, subSize, subSize);
                                    }
                                }
                            }
                            //Console.WriteLine (lastReg);
                            //using (Image b = Bitmap.FromFile(basePath+path))
                            //	g.DrawImage (b, px, py, drawSize, drawSize);
                        }
                    }

                    string path = McaFile.Path(map.Dimension, x, z);
                    if (File.Exists(path))
                    {
                        //Draw Age border
                        Color c = Color.FromArgb(128, AgeColor(File.GetLastWriteTime(path)));
                        using (Pen p = new Pen(c))
                        {
                            p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                            g.DrawRectangle(p, px, py, drawSize, drawSize);
                        }
                        g.DrawString((x >> 9) + "," + (z >> 9), font, Brushes.GreenYellow, px, py);
                    } else
                    {
                        //g.DrawRectangle (Pens.Orange, px, py, drawSize, drawSize);
                        //g.DrawString (path, font, Brushes.Purple, px, py);
                        //g.DrawString (scale.ToString (), font, Brushes.Purple, px, py);
                    }
                }
            }
        }

        public static Color AgeColor(DateTime date)
        {
            double age = (DateTime.Now - date).TotalDays;
            if (age < 1)
                return Color.Lime;
            if (age < 2)
                return Color.Green;
            if (age < 3)
                return Color.YellowGreen;
            if (age < 4)
                return Color.Yellow;
            if (age < 5)
                return Color.Orange;
            if (age < 6)
                return Color.OrangeRed;
            if (age < 7)
                return Color.Red;
            if (age < 8)
                return Color.MediumPurple;
            if (age < 500)
                return Color.FromArgb(128, Color.DarkGray);
            return Color.DarkGray;
        }
    }
}

