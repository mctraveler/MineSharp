using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
	public class EnchantItem : PacketFromClient
	{
        public const byte ID = 0x11;

        public override byte PacketID { get { return ID; } }
		
		sbyte WindowID;
		/// <summary>
		/// 0-2 where 0 is the topmost
		/// </summary>
		sbyte EnchantmentIndex;
		
		public override string ToString ()
		{
			return string.Format ("[EnchantItem: {0}, {1}]", WindowID, EnchantmentIndex);
		}
		
        protected override void Parse(EndianBinaryReader r)
        {
			this.WindowID = r.ReadSByte ();
			this.EnchantmentIndex = r.ReadSByte ();
		}
		
		protected override void Prepare (EndianBinaryWriter w)
		{
			w.Write ((sbyte)WindowID);
			w.Write ((sbyte)EnchantmentIndex);
		}
	}
}

