using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using MineProxy;
using MineProxy.Control;
using System.IO;

namespace MineControl
{
    public class ControlWindow : Form
    {
        public MapControl Map;
        public StatusControl PlayerList;
        //public HistoryControl History;
        public DateTimePicker datePick = new DateTimePicker();

        public ControlWindow()
        {
            Map = new MapControl(this);
            PlayerList = new StatusControl(this);
            //History = new HistoryControl (PlayerList);
			
            Map.Dock = DockStyle.Fill;
            PlayerList.Dock = DockStyle.Fill;
            //History.Dock = DockStyle.Top;
			
            this.Text = "MineProxy";
            this.WindowState = FormWindowState.Maximized;
			
            //Right panel, date picker + player list
            datePick.Value = DateTime.Now.Date;
            datePick.ValueChanged += HandleDatePickValueChanged;
            datePick.Dock = DockStyle.Top;
            PlayerList.Dock = DockStyle.Fill;
            Panel right = new Panel();
            right.Controls.Add(PlayerList);
            right.Controls.Add(datePick);
            right.Dock = DockStyle.Fill;
			
            SplitContainer split = new SplitContainer();
            split.Panel1.Controls.Add(Map);
            split.Panel2.Controls.Add(right);
            split.SplitterWidth = 10;
            split.SplitterDistance = Width / 2;
            split.Orientation = Orientation.Vertical;
            split.Dock = DockStyle.Fill;
            Controls.Add(split);
            //Controls.Add (History);
			
            this.MainMenuStrip = new MenuStrip();
            this.MainMenuStrip.Items.Add("Pardon").Click += HandlePardonClick;
            this.MainMenuStrip.Items.Add("Ban").Click += HandleBanClick;
            this.MainMenuStrip.Items.Add("Overworld").Click += HandleNetherCheckedChanged;
            
            this.MainMenuStrip.Items.Add("Reload").Click += HandleRegionReloadClick;
            //this.MainMenuStrip.Items.Add ("History Search").Click += HistorySearch;
            this.MainMenuStrip.Items.Add("TP-Nuxas").Click += HandleTpNuxasClick;
            ;
            this.MainMenuStrip.Items.Add("TP-Player").Click += HandleTpPlayerClick;
            ;


            this.MainMenuStrip.Visible = true;
            this.MainMenuStrip.BringToFront();
            this.Controls.Add(this.MainMenuStrip);
        }

        void HandleTpNuxasClick(object sender, EventArgs e)
        {
            var c = Map.Position.Clone();
            c.Y = 100;
            ProxyControl.Tp("nuxas", c, Map.Dimension);
        }

        void HandleTpPlayerClick(object sender, EventArgs e)
        {
            var c = Map.Position.Clone();
            c.Y = 100;
            ProxyControl.Tp("Player", c, Map.Dimension);
        }

        void HistorySearch(object sender, EventArgs e)
        {
            //History.LoadAllRange (datePick.Value, Map.Position, 0);
        }

        void HandleDatePickValueChanged(object sender, EventArgs e)
        {
            //History.Load (PlayerList.SelectedUsername, datePick.Value);
        }

        void HandleRegionReloadClick(object sender, EventArgs e)
        {
            Map.LoadRegions();
        }

        void HandleNetherCheckedChanged(object sender, EventArgs e)
        {
            ToolStripItem m = (ToolStripItem)sender;
			
            if (Map.Dimension == Dimensions.Nether)
            {
                Map.Dimension = Dimensions.End;
                Map.Scale *= 8;
            } else if (Map.Dimension == Dimensions.End)
                Map.Dimension = Dimensions.Overworld;
            else if (Map.Dimension == Dimensions.Overworld)
            {
                Map.Dimension = Dimensions.Nether;
                Map.Scale /= 8;
            }
			
            Map.Invalidate();
			
            m.Text = "" + Map.Dimension;
        }

        void HandleBanClick(object sender, EventArgs e)
        {
            string u = TextInput.GetInput(this, "Who");
            if (u == null)
                return;
            string w = TextInput.GetInput(this, "Why");
            if (w == null)
                return;
            ProxyControl.Ban(u, DateTime.MaxValue, w);
        }

        void HandlePardonClick(object sender, EventArgs e)
        {
            string u = TextInput.GetInput(this, "Who");
            if (u == null)
                return;
            ProxyControl.Pardon(u);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ProxyControl.Update += PlayersUpdated;
			
#if DEBUG
            ProxyControl.Connect(25465);
            ServerCommander.Startup();
#else
            ProxyControl.Connect(25465);
            ServerCommander.Startup();
#endif
        }

        private void PlayersUpdated()
        {
            if (ProxyControl.Connected)
                PlayerList.BackColor = Color.White;
            else
                PlayerList.BackColor = Color.Red;
            try
            {
                Invoke(new Action(PlayerList.UpdateList));
                Invoke(new Action(Map.Invalidate));
            } catch (Exception)
            {
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            ProxyControl.Stop();
            base.OnClosing(e);
        }
    }
}

