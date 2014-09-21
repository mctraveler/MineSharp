using System;
using System.Drawing;
using MineProxy;

namespace MineControl
{
	public class HistoryNode
	{
		public string Username { get; set; }
		
		public DateTime Timestamp { get; set; }
		public CoordDouble Position { get; set; }
		public int Dimension { get; set; }
		public Color Color { get; set; }

		public SlotItem Item { get; set; }
		
		private HistoryNode ()
		{
		}
		
		public static HistoryNode FromLog (LogStream l, string username)
		{
			if(l.Position == null)
				return null;
			
			HistoryNode n = new HistoryNode ();
			n.Username = username;
			n.Dimension = l.Position.Dimension;
			n.Timestamp = l.Timestamp;
			
			if (l.Digging != null) {
				if (l.Digging.Status != PlayerDigging.StatusEnum.FinishedDigging)
					return null;
				n.Position = l.Digging.Position.CloneDouble ();
				n.Color = Color.Red;
				return n;
			}
			if (l.PlaceBlock != null) {
				n.Position = l.PlaceBlock.BlockPosition.CloneDouble ();
				if (l.PlaceBlock.Item == null || l.PlaceBlock.Item.ItemID <= 0) {
					n.Color = Color.Transparent;
					return n;
				}
				n.Color = Color.GreenYellow; //All other blocks
				n.Item = l.PlaceBlock.Item;
				if (l.PlaceBlock.Item.ItemID == BlockID.Waterbucket)
					n.Color = Color.Blue;
				if (l.PlaceBlock.Item.ItemID == BlockID.FlintandSteel)
					n.Color = Color.Orange;
				if (l.PlaceBlock.Item.ItemID == BlockID.Lavabucket)
					n.Color = Color.Orange;
				if (l.PlaceBlock.Item.ItemID == BlockID.TNT)
					n.Color = Color.Red;
				return n;
			}
			
			if (l.Window != null && l.Window.WindowID >= 0)
			{
				n.Position = l.Position.Position;
				n.Color = Color.Yellow;
				n.Item = l.Window.Item;
				return n;
			}
			
			return null;
		}
	}
}

