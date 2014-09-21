using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
	public class EntityLookRelativeMove : EntityRelativeMove, IEntity
	{
        public new const byte ID = 0x17;

		public override byte PacketID { get { return ID; } }
	
		public double Yaw { get; set; }
		
		public double Pitch { get; set; }
		
        public EntityLookRelativeMove ()
        {
        }
		
        protected override void Parse(EndianBinaryReader r)
        {
			EID = ReadVarInt(r);
			Delta = new CoordDouble ();
			Delta.X = ((double)r.ReadSByte ()) / 32;
			Delta.Y = ((double)r.ReadSByte ()) / 32;
			Delta.Z = ((double)r.ReadSByte ()) / 32;
			Yaw = r.ReadSByte () * Math.PI / 128;
			Pitch = r.ReadSByte () * Math.PI / 128;
            OnGround = r.ReadBoolean();
		}
		
		protected override void Prepare (EndianBinaryWriter w)
		{
			WriteVarInt(w, EID);
			w.Write ((sbyte)(Delta.X * 32));
			w.Write ((sbyte)(Delta.Y * 32));
			w.Write ((sbyte)(Delta.Z * 32));
			w.Write ((sbyte)(Yaw * 128 / Math.PI));
			w.Write ((sbyte)(Pitch * 128 / Math.PI));
            w.Write((bool)OnGround);
		}
	}
	
}

