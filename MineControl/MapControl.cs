using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using MineProxy;
using MineProxy.Control;

namespace MineControl
{
    public class MapControl : Control
    {
        public RegionList Regions;

        readonly ControlWindow MainWindow;
		
        public MapControl(ControlWindow main)
        {
            this.MainWindow = main;
            this.DoubleBuffered = true;
            this.BackColor = System.Drawing.Color.Black;
            LoadRegions();

            this.Position.X = - Width / 2;
            this.Position.Z = - Height / 2;
        }

        public void LoadRegions()
        {
            Regions = RegionLoader.Load("main/regions.json");
        }

        public Dimensions Dimension { get ; set ; }

        public new float Scale = 1;
        /// <summary>
        /// Topleft Coordinate
        /// </summary>
        public CoordDouble Position = new CoordDouble();
        Point dragStart;
        bool dragging = false;
        MouseButtons dragButton;
		
        protected override void OnMouseDown(MouseEventArgs e)
        {
            dragStart = e.Location;
            dragButton = e.Button;
            dragging = true;
			
            if (e.Button == MouseButtons.Left)
                FollowUser = null;
			
            if (e.Button == MouseButtons.Right)
            {
				
            }
			
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (dragging == false)
                return;
		
            int dx = e.X - dragStart.X;
            int dy = e.Y - dragStart.Y;
            dragStart = e.Location;
			
            if (dragButton == MouseButtons.Left)
            {
                Position.X -= dx * Scale;
                Position.Z -= dy * Scale;
            }

            if (dragButton == MouseButtons.Right)
            {
                if (dy > 0)
                    Scale *= (float)(1.05 * Math.Exp(dy / 100));
                else
                    Scale /= (float)(1.05 * Math.Exp(-dy / 100));
                if (Scale < 0.05)
                    Scale = 0.05f;
            }

            Invalidate();
        }
		
        protected override void OnMouseUp(MouseEventArgs e)
        {
            dragging = false;
        }
		
        private string FollowUser = "Player";
		
        public void Follow(MineProxy.Control.Player p)
        {
            FollowUser = p.Username;
        }
		
        protected override void OnPaint(PaintEventArgs e)
        {
            //Prepare follow
            if (FollowUser != null)
            {
                foreach (MineProxy.Control.Player p in ProxyControl.Players.List)
                {
                    if (p.Username == FollowUser)
                    {
                        Position.X = p.Position.X;
                        Position.Z = p.Position.Z;
                        if (p.Dimension == -1)
                        {
                            Position.X *= 8;
                            Position.Z *= 8;
                        }
                    }
                }
            }
			
            base.OnPaint(e);
            Graphics g = e.Graphics;
            int r = 10;
			
            //Map
            TiledMapPainter.Paint(g, this, Width, Height, Position, Scale);
			
            //Regions
            foreach (WorldRegion w in Regions.List)
            {
                DrawRegion(g, w);
            }
		
            //Players
            foreach (StatusControl.PlayerListItem p in MainWindow.PlayerList.Items)
            {
                if (p.Online == false)
                    continue;
				
                DrawPlayer(g, p);
            }
		
            //History
            //DrawHistory (g);
			
            //Crosshair
            g.DrawRectangle(Pens.Yellow, Width / 2, Height / 2 - r, 0, r * 2 + 1);
            g.DrawRectangle(Pens.Yellow, Width / 2 - r, Height / 2, r * 2 + 1, 0);
			

            g.FillRectangle(Brushes.White, 0, 0, 100, 50);
            g.DrawString(DateTime.Now.ToString("HH:mm:ss") + "\n" + Position.X.ToString("0") + ", " + Position.Z.ToString("0"), Font, Brushes.Black, 0, 0);
        }
		
        protected void DrawRegion(Graphics g, WorldRegion w)
        {
            if (w.Dimension != (int)Dimension)
                return;
			
            Point topleft = FromCoord(w.MinX, w.MinZ);
            Point bottomright = FromCoord(w.MaxX, w.MaxZ);
            Size size = new Size(bottomright.X - topleft.X, bottomright.Y - topleft.Y);
            Rectangle rect = new Rectangle(topleft, size);
			
            if (size.Width < 1 || size.Height < 1)
                return;
            if (w.MaxX < w.MinX || w.MaxY < w.MinY || w.MaxZ < w.MinZ)
                g.FillRectangle(Brushes.Red, new Rectangle(topleft, size));
			
            var pi = Position.CloneInt();
            if (!w.Overlap(new MineProxy.Region(pi.X, pi.X, 0, 10000, pi.Z, pi.Z)))
            {
                //Draw defaullt content
                using (Pen p = new Pen(TiledMapPainter.AgeColor(w.Stats.LastVisitResident)))
                    g.DrawRectangle(p, new Rectangle(topleft, size));
                g.DrawString(w.Name, Font, Brushes.LimeGreen, rect);
                return;
            }

            //Draw selected region
            g.DrawRectangle(Pens.Orange, new Rectangle(topleft, size));

            Point bottomleft = FromCoord(w.MinX, w.MaxZ);
            TimeSpan lastVisit = DateTime.UtcNow - w.Stats.LastVisitResident;
            string lastVisitText = lastVisit.TotalDays.ToString("0.0") + " days ago";
            if (lastVisit.TotalDays > 500)
                lastVisitText = "not visited";

            string text = w + "\n" + lastVisitText + "\n" + w.GetResidents();
            size = g.MeasureString(text, Font, 300).ToSize();
            size.Width += 6;
            size.Height += 6;
            rect = new Rectangle(bottomleft, size);
            g.FillRectangle(Brushes.DarkGreen, rect);
            g.DrawRectangle(Pens.LightGreen, rect);
            rect.X += 3;
            rect.Y += 3;
            g.DrawString(text, Font, Brushes.WhiteSmoke, rect);

            if (w.SubRegions == null)
                return;
            foreach (WorldRegion s in w.SubRegions)
                DrawRegion(g, s);
        }
		
        /// <summary>
        /// Screen point from world coords
        /// </summary>
        public Point FromCoord(CoordDouble c)
        {
            return FromCoord(c.X, c.Z);
        }

        public Point FromCoord(double x, double z)
        {
            int px = (int)((x - Position.X) / Scale) + Width / 2;
            int py = (int)((z - Position.Z) / Scale) + Height / 2;
			
            return new Point(px, py);
        }

        void DrawPlayer(Graphics g, StatusControl.PlayerListItem i)
        {
            MineProxy.Control.Player p = i.Item;
            if (p.Position == null)
                return;
			
            CoordDouble np = p.Position.ToMapDimensions((Dimensions)p.Dimension);
            Point pp = FromCoord(np);
			
            //Röd Cirkel
            int r = 2;
            if (i.Selected)
            {
                g.DrawEllipse(Pens.Red, pp.X - r, pp.Y - r, r * 2, r * 2);
			
                //Historik
                var hist = PlayerHistory.GetHistory(p);
                foreach (var hi in hist)
                {
                    if (hi == null)
                        continue;
                    Point hp = FromCoord(hi.Position.ToMapDimensions((Dimensions)hi.Dimension));
                    g.DrawEllipse(Pens.Red, hp.X - r, hp.Y - r, r * 2, r * 2);
                }
            }
            //Namnbricka
            Size size = g.MeasureString(p.Username, Font, 500).ToSize();
            Color back = Color.White;
            if (p.Dimension == -1)
                back = Color.Red;
            if (p.Dimension == 1)
                back = Color.Yellow;
            if (p.Session != "RealSession" && p.Session != "VanillaSession")
                back = Color.Gray;
            using (Brush b = new SolidBrush (System.Drawing.Color.FromArgb (128, back)))
                g.FillRectangle(b, pp.X, pp.Y, size.Width, size.Height);
            if (i.Selected)
                g.DrawRectangle(Pens.White, pp.X, pp.Y, size.Width, size.Height);
            


            g.DrawString(p.Username, this.Font, Brushes.Black, pp.X, pp.Y);

            //Höjd
            int h = (int)(p.Position.Y / 8);
            g.DrawRectangle(Pens.Black, pp.X, pp.Y - 32, r, 32);
            using (Brush b = new SolidBrush (back))
                g.FillRectangle(b, pp.X, pp.Y - h, r, h);
			
			
            //g.DrawLine (Pens.Red, Width / 2, Height / 2, (float)x, (float)y);
        }
    }
}

