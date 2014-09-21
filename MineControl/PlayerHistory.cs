using System;
using MineProxy;
using MineProxy.Control;
using System.Collections.Generic;

namespace MineControl
{
	public class PlayerHistory
	{
		static Dictionary<string, PlayerHistory> list = new Dictionary<string, PlayerHistory> ();
		public class HistoryItem
		{
			public CoordDouble Position;
			public int Dimension;
			public int Attached;
		}
		public HistoryItem[] History = new HistoryItem[100];
		long lastStep;
		public static readonly TimeSpan StepSize = new TimeSpan (TimeSpan.TicksPerSecond / 1);
		
		public static HistoryItem[] GetHistory (MineProxy.Control.Player player)
		{
			lock (list) {
				if (list.ContainsKey (player.Username) == false)
					return null;
				else
					return list [player.Username].History;
			}
		}
		
		public static void Update (MineProxy.Control.Player player)
		{
			if (player.Position == null)
				return;
			PlayerHistory ph;
			lock (list) {
				if (list.ContainsKey (player.Username) == false) {
					ph = new PlayerHistory ();
					list.Add (player.Username, ph);
				} else
					ph = list [player.Username];
			}
			ph.UpdateHistory (player);
		}
		
		void UpdateHistory (MineProxy.Control.Player player)
		{
			long step = DateTime.Now.Ticks / StepSize.Ticks;
			if (lastStep != step) {
				lastStep = step;
			
				for (int n = History.Length - 1; n > 0; n--) {
					History [n] = History [n - 1];
				}
				History [0] = new HistoryItem ();
			}
			if (History [0] == null)
				History [0] = new HistoryItem ();
			History [0].Position = player.Position.Clone ();
			History [0].Dimension = player.Dimension;
			History [0].Attached = player.AttachedTo;
		}

	}
}

