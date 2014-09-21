using System;
using System.Windows.Forms;
using MineProxy;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtocolBuffers;

namespace MineControl
{
	public class HistoryControl : Control
	{
		readonly StatusControl players;

		public HistoryControl (StatusControl p)
		{
			this.players = p;
			this.Height = 30;
		}
		
		DateTime center = DateTime.Now;
		TimeSpan width = TimeSpan.FromMinutes (10);
		string path = null;
		List<HistoryNode> list = new List<HistoryNode> ();
		
		public IEnumerable<HistoryNode> GetItems ()
		{
			DateTime start = center - width;
			DateTime end = center + width;
			foreach (HistoryNode l in list) {
				if (l.Timestamp < start)
					continue;
				if (l.Timestamp > end)
					yield break;
				yield return l;
			}
		}
		
		protected override void OnMouseDown (MouseEventArgs e)
		{
			if (list.Count == 0)
				return;
			DateTime s = list [0].Timestamp.Date;
			center = s.AddMinutes (e.X * TimeSpan.FromDays (1).TotalMinutes / Width);
			Refresh ();
			
			players.ClearMarked ();
			foreach (var n in GetItems()) {
				players.Mark (n.Username);
			}
		}
		
		protected override void OnPaint (PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.Clear (Color.LightBlue);
			if (list.Count == 0)
				return;
			
			DateTime s = list [0].Timestamp.Date;
			
			//Selection
			float x1 = (float)(Width * (center - width - s).TotalSeconds / TimeSpan.FromDays (1).TotalSeconds);
			float x2 = (float)(Width * (center + width - s).TotalSeconds / TimeSpan.FromDays (1).TotalSeconds);
			g.FillRectangle (Brushes.White, x1, 0, x2 - x1, Height);
			
			foreach (var l in list) {
				float x = (float)(Width * (l.Timestamp - s).TotalSeconds / TimeSpan.FromDays (1).TotalSeconds);
				//float y = (float)(Height * (1 - l.Position.Y / 128f));
				using (Pen p = new Pen(l.Color))
					g.DrawLine (p, x, 0, x, Height);
			}
			g.DrawString (path + "\n" + s.ToShortDateString () + " " + center.ToShortTimeString (), Font, Brushes.Black, new Point ());
		}

		/// <summary>
		/// Loads all history one date close to center
		/// </summary>
		public void LoadAllRange (DateTime date, CoordDouble pos, int dimension)
		{
			List<HistoryNode> l = new List<HistoryNode> ();
			string[] files = Directory.GetFiles ("log/packet/" + date.ToString ("yyyy-MM-dd") + "/");
			foreach (string f in files) {
				LoadFile (l, f);
			}
			List<HistoryNode> l2 = new List<HistoryNode> ();
			players.ClearMarked ();
			foreach (var n in l) {
				if (n.Dimension != dimension)
					continue;
				if (n.Position.DistanceXZTo (pos) < 10) {
					l2.Add (n);
					if (n.Item != null && n.Color == Color.Yellow)
						Console.WriteLine (n.Username + " picked " + n.Item + " at " + n.Position);
					players.Mark (n.Username);
				}
			}
			list = l2;
			if (list.Count > 0) {
				center = list [0].Timestamp.Date.AddHours (12);
			}
			Invalidate ();
		}
		
		/// <summary>
		/// One days history for one player
		/// </summary>
		public void Load (string username, DateTime date)
		{
			List<HistoryNode> l = new List<HistoryNode> ();
			LoadFile (l, date, username);
			list = l;
			if (l.Count > 0) {
				center = l [0].Timestamp.Date.AddHours (12);
			}
			Invalidate ();
		}
		
		static void LoadFile (List<HistoryNode> l, DateTime date, string username)
		{
			string path = "log/packet/" + date.ToString ("yyyy-MM-dd") + "/" + username;
			if (File.Exists (path))
				LoadFile (l, path);
		}

		static void LoadFile (List<HistoryNode> l, string path)
		{
			string username = Path.GetFileName (path);
			if (File.Exists (path)) {
				using (FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
					while (f.Position < f.Length) {
						try {
							byte[] b = ProtocolParser.ReadBytes (f);
							LogStream ls = LogStream.Deserialize (b);
							
							if (ls.Digging != null && ls.Digging.Status != PlayerDigging.StatusEnum.FinishedDigging)
								continue;
							if (ls.PlaceBlock != null) {
								if (ls.PlaceBlock.Item == null)
									continue;
								if (ls.PlaceBlock.Item.ItemID <= 0)
									continue;
							}

							HistoryNode n = HistoryNode.FromLog (ls, username);
							if (n == null)
								continue;
							if (Math.Abs (n.Position.X) < 2 || Math.Abs (n.Position.Z) < 2)
								continue;								
							l.Add (n);
						} catch (Exception) {
						}
					}
				}
			}
			return;
		}
	}
}

