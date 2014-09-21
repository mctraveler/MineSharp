using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using MineProxy;
using MineProxy.Control;

namespace MineControl
{
    public class StatusControl : ListBox
    {
        public string SelectedUsername { get; set; }
		
        ControlWindow main;
        ToolStripDropDownMenu dropDown;

        public StatusControl(ControlWindow main)
        {
            this.main = main;
            this.SelectionMode = SelectionMode.MultiExtended;
			
            DrawMode = DrawMode.OwnerDrawFixed;

            ItemHeight = 30;
			
            dropDown = new ToolStripDropDownMenu();
            dropDown.Items.Add("Clear Disconnected").Click += HandleClearClick;
            dropDown.Items.Add("Sort").Click += HandleSortClick;
            ;
            dropDown.Items.Add("Group on top").Click += HandleTopClick;
            //dropDown.Items.Add ("History").Click += ShowHistory;
            dropDown.Items.Add("Follow").Click += FollowPlayer;
            //dropDown.Items.Add ("Tp nuxas to ").Click += TpNuxasTo;
            dropDown.Items.Add("Tp Player to ").Click += TpPlayerTo;
            dropDown.Items.Add("Pardon").Click += PardonPlayer;
            dropDown.Items.Add(new ToolStripSeparator());
            dropDown.Items.Add("Kick").Click += KickPlayer;
            dropDown.Items.Add("Ban 30 min").Click += BanPlayer30;
            dropDown.Items.Add("Ban").Click += BanPlayer;
			
        }

        void HandleSortClick(object sender, EventArgs e)
        {
            List<PlayerListItem> all = new List<PlayerListItem>();
            foreach (PlayerListItem i in Items)
                all.Add(i);
			
            all.Sort((a, b) => {
                return string.Compare(a.Item.Username, b.Item.Username); }
            );
			
            Items.Clear();
            foreach (var i in all)
                Items.Add(i);			
        }

        void HandleTopClick(object sender, EventArgs e)
        {
            List<PlayerListItem> top = new List<PlayerListItem>();
            foreach (PlayerListItem i in SelectedItems)
                top.Add(i);
            foreach (var i in top)
                Items.Remove(i);
            foreach (var i in top)
                Items.Insert(0, i);			
        }
        /*
		void ShowHistory (object sender, EventArgs e)
		{
			main.History.Load (SelectedUsername, main.datePick.Value);
		}
		*/

        void TpNuxasTo(object sender, EventArgs e)
        {
            if (SelectedItem == null)
                return;
			
            MineProxy.Control.Player p = ((PlayerListItem)SelectedItem).Item;
            //player.Session.World.Send("tp nuxas " + p.Username);			
        }

        void TpPlayerTo(object sender, EventArgs e)
        {
            if (SelectedItem == null)
                return;
			
            MineProxy.Control.Player p = ((PlayerListItem)SelectedItem).Item;
            //player.Session.World.Send("tp Player " + p.Username);			
        }

        void PardonPlayer(object sender, EventArgs e)
        {
            foreach (PlayerListItem i in SelectedItems)
                ProxyControl.Pardon(i.Item.Username);
        }

        void HandleClearClick(object sender, EventArgs e)
        {
            lock (this)
            {
				
                List<PlayerListItem > rem = new List<PlayerListItem>();
                foreach (PlayerListItem pli in Items)
                {
                    if (pli.Online == false)
                        rem.Add(pli);
                }
                foreach (PlayerListItem p in rem)
                {
                    Items.Remove(p);
                    intermediate.Remove(p.Item.Username);
                }
            }
        }

        void FollowPlayer(object sender, EventArgs e)
        {
            if (SelectedItem == null)
                return;
			
            MineProxy.Control.Player p = ((PlayerListItem)SelectedItem).Item;
            main.Map.Follow(p);
        }
		
        void BanPlayer(object sender, EventArgs e)
        {
            string players = "";
            foreach (PlayerListItem i in SelectedItems)
                players += " " + i.Item.Username;
				
            string reason = TextInput.GetInput(this, "Why Ban" + players);
            if (reason == null)
                return;
			
            foreach (PlayerListItem i in SelectedItems)
                ProxyControl.Ban(i.Item, DateTime.MaxValue, reason);
        }

        void BanPlayer30(object sender, EventArgs e)
        {
            string players = "";
            foreach (PlayerListItem i in SelectedItems)
                players += " " + i.Item.Username;
			
            string reason = TextInput.GetInput(this, "Why Ban" + players);
            if (reason == null)
                return;
			
            foreach (PlayerListItem i in SelectedItems)
                ProxyControl.Ban(i.Item, DateTime.Now.AddMinutes(30), reason);
        }
		
        void KickPlayer(object sender, EventArgs e)
        {
            string players = "";
            foreach (PlayerListItem i in SelectedItems)
                players += " " + i.Item.Username;
			
            string reason = TextInput.GetInput(this, "Why Kick" + players);
            if (reason == null)
                return;
			
            foreach (PlayerListItem i in SelectedItems)
                ProxyControl.Kick(i.Item, reason);
        }
		
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            foreach (PlayerListItem i in Items)
                i.Selected = false;
            foreach (PlayerListItem i in SelectedItems)
                i.Selected = true;
			
            base.OnSelectedIndexChanged(e);
            if (SelectedItem == null)
                return;
            SelectedUsername = ((PlayerListItem)SelectedItem).Item.Username;
            Rectangle r = GetItemRectangle(SelectedIndex);
            dropDown.Show(this, r.Location, ToolStripDropDownDirection.BelowLeft);
            Refresh();
        }
		
        public class PlayerListItem
        {
            public MineProxy.Control.Player Item { get; set; }
			
            public bool Selected { get; set; }

            public bool Online { get; set; }
        }
		
        Dictionary<string, PlayerListItem> intermediate = new Dictionary<string, PlayerListItem>();

        public void UpdateList()
        {
            lock (this)
            {
                List<string > gone = new List<string>();
                foreach (var kvp in intermediate)
                    gone.Add(kvp.Value.Item.Username);
			
                foreach (MineProxy.Control.Player p in ProxyControl.Players.List)
                {
                    if (intermediate.ContainsKey(p.Username) == false)
                    {
                        var i = new PlayerListItem();
                        i.Item = p;
                        intermediate.Add(p.Username, i);
                        Items.Add(i);
                    }
                    intermediate [p.Username].Item = p;
                    intermediate [p.Username].Online = true;
                    gone.Remove(p.Username);
                }

                foreach (string u in gone)
                {
                    if (intermediate.ContainsKey(u) == false)
                        continue;
				
                    var pli = intermediate [u];
                    pli.Online = false;
                    //intermediate.Remove (u);
                    //Items.Remove (pli);
                }
            }
            Invalidate();
        }
		
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            PlayerListItem item = (PlayerListItem)Items [e.Index];
            var p = item.Item;
            Rectangle r = e.Bounds;
			
            if (p.BannedUntil > DateTime.Now)
            {
                g.FillRectangle(Brushes.Red, e.Bounds);
            } else
            {
                if (item.Online)
                {
                    switch (p.Session)
                    {
                        case "VanillaSession":
                        case "RealSession":
                            switch (p.Dimension)
                            {
                                case -1:
                                    g.FillRectangle(Brushes.LightPink, e.Bounds);
                                    break;
                                case 1:
                                    g.FillRectangle(Brushes.Yellow, e.Bounds);
                                    break;
                                case 0:
                                    g.FillRectangle(Brushes.White, e.Bounds);
                                    break;
                                default:
                                    throw new NotImplementedException("Dimension: " + p.Dimension);
                            }
                            break;
                        case "HellSession":
                            g.FillRectangle(Brushes.Red, e.Bounds);
                            break;
                        case "GreenSession":
                            g.FillRectangle(Brushes.LightGreen, e.Bounds);
                            break;
                        case "WarpSession":
                            g.FillRectangle(Brushes.Cyan, e.Bounds);
                            break;
                        case "TheConstructSession":
                        case "CastleSession":
                        case "AfkSession":
                            g.FillRectangle(Brushes.LightGray, e.Bounds);
                            break;
                        case "VoidSession":
                        case "PossessSession":
                            g.FillRectangle(Brushes.DarkGray, e.Bounds);
                            break;
                        default:
                            throw new NotImplementedException("Session: " + p.Session);
                    }
                } else
                    g.FillRectangle(Brushes.Gray, e.Bounds);
            }
            if (item.Selected)
                g.FillRectangle(Brushes.LightBlue, e.Bounds);
			
            //e.DrawFocusRectangle ();
            g.DrawRectangle(Pens.Black, r);
			
            int hScale = 1;
            int vScale = 1;
            //Calculate Speed
            PlayerHistory.HistoryItem[] hist = PlayerHistory.GetHistory(p);
            //double lastHSpeed = 0;
            //double lastVSpeed = 0;
            if (hist != null)
            {
                int dx = Width / hist.Length;
			
                //int hlw = (int)(hScale * LawsOfMinecraft.LimitRunning);
                //int hlb = (int)(hScale * LawsOfMinecraft.LimitBoat);
                int hlc = (int)(hScale * LawsOfMinecraft.LimitCart);
                //int hll = (int)(vScale * LawsOfMinecraft.LimitStairs);

                //Background
                if (item.Online)
                {
                    //g.FillRectangle (Brushes.Green, r.X, r.Bottom - hlw, r.Width, hlw);
                    //g.FillRectangle (Brushes.Blue, r.X, r.Bottom - hlb, r.Width, hlb - hlw);
                    //g.FillRectangle (Brushes.Gray, r.X, r.Bottom - hlc, r.Width, hlc - hlb);
                    //g.FillRectangle (Brushes.LightCyan, r.X, r.Bottom - hlc - hll, r.Width, hll);
                }
                for (int n = 1; n < hist.Length - 1; n++)
                {
				
                    CoordDouble speed;
                    if (hist [n + 1] == null || hist [n] == null)
                        speed = new CoordDouble();
                    else
                        speed = (hist [n].Position - hist [n + 1].Position) / PlayerHistory.StepSize.TotalSeconds;
				
                    double hspeed = Math.Sqrt(speed.X * speed.X + speed.Z * speed.Z);
                    if (n == 1)
                    {
                        //lastHSpeed = hspeed;
                        //lastVSpeed = speed.Y;
                    }
				
                    double limit = 5;
                    if (p.AttachedTo > 0) //Boat or Cart
                        limit = LawsOfMinecraft.LimitCart;

                    if (hspeed > 12)
                        hspeed = 12;
				
                    int hs = (int)(hScale * hspeed);
                    int hl = (int)(hScale * limit);

                    Brush hBrush = Brushes.LightGreen;
                    if (p.AttachedTo == 333) //Boat
                        hBrush = Brushes.LightBlue;
                    if (p.AttachedTo == 328) //Cart
                        hBrush = Brushes.LightGray;
				
                    //Bars
                    if (hs > hl)
                    {
                        g.FillRectangle(hBrush, dx * n, r.Bottom - hl, dx, hl);
                        g.FillRectangle(Brushes.Red, dx * n, r.Bottom - hs, dx, hs - hl);
                    } else
                        g.FillRectangle(hBrush, dx * n, r.Bottom - hs, dx, hs);

                    //Y-speed
                    int ycenter = r.Bottom - hlc;
                    int vs = ycenter - (int)(speed.Y * vScale);
                    if (vs <= r.Y)
                        vs = r.Y + 1;
                    if (vs >= r.Bottom)
                        vs = r.Bottom - 1;
                    //g.DrawLine (Pens.Black, dx * n, ycenter, dx * (n + 1), ycenter);
                    Pen pen = Pens.Gold;
                    if (speed.Y > limit)
                        pen = Pens.Red;
                    g.DrawLine(pen, dx * n, vs, dx * (n + 1), vs);
                }
            }			
            string t = p.Username;
            if (p.Chat != null && p.Chat.Timestamp.AddMinutes(1) > DateTime.Now)
                t += "\n" + p.Chat;

            g.DrawString(t, Font, Brushes.Black, r);

            t = p.Session.Replace("Session", "") + ": " + p.Uptime.TotalDays.ToString("0.00") + " D";
            var m = g.MeasureString(t, Font);
            g.DrawString(t, Font, Brushes.Black, Width - m.Width, r.Y);
        }
    }
}
