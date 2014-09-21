using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class RemoveEntityEffect : PacketFromServer, IEntity
    {
        public const byte ID = 0x1E;

        public override byte PacketID { get { return ID; } }
		
        public int EID { get; set; }
		
        public PlayerEffects Effect { get; set; }

        public RemoveEntityEffect()
        {
        }

        public RemoveEntityEffect(int eid, PlayerEffects effect)
        {
            this.EID = eid;
            this.Effect = effect;
        }
		
        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Effect = (PlayerEffects)r.ReadSByte();
#if DEBUG
            if (Effect.ToString() == ((int)Effect).ToString())
                throw new NotImplementedException(Effect.ToString());
#endif
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            w.Write((sbyte)Effect);
        }
    }
}

