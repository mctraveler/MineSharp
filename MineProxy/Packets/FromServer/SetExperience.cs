using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
	public class SetExperience : PacketFromServer
	{
        public const byte ID = 0x1F;
		public override byte PacketID { get { return ID; } }
		
		/// <summary>
		/// Experience within the current level
		/// </summary>
		public float LevelExperience;
        public int Level;
        public int TotalExperience;
		
		public override string ToString ()
		{
			return string.Format ("[ExperienceUpdate: {0}, {1}, {2}]", LevelExperience, Level, TotalExperience);
		}
				
        protected override void Parse(EndianBinaryReader r)
        {
			//Console.WriteLine (BitConverter.ToString (r.ReadBytes (50)));
			this.LevelExperience = r.ReadSingle ();
            this.Level = ReadVarInt(r);
            this.TotalExperience = ReadVarInt(r);
		}
		
		protected override void Prepare (EndianBinaryWriter w)
		{
			w.Write (LevelExperience);
            WriteVarInt(w, Level);
            WriteVarInt(w, TotalExperience);
		}
	}
}

