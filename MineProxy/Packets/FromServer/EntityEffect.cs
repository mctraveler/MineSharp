using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class EntityEffect : PacketFromServer, IEntity
    {
        public const byte ID = 0x1D;

        public override byte PacketID { get { return ID; } }
		
        public int EID { get; set; }
			
        public PlayerEffects Effect{ get; set; }

        public sbyte Amplifier { get; set; }

        public int Duration { get; set; }
		
        public bool HideParticles { get; set; }

        public override string ToString()
        {
            return string.Format("[EntityEffect: EID={1}, EffectID={2}, Amplifier={3}, Duration={4} s]", PacketID, EID, Effect, Amplifier, Duration / 20);
        }
		
        public EntityEffect()
        {

        }

        /// <param name="effect"></param>
        /// <param name="amplifier">sbyte</param>
        /// <param name="duration">in seconds, more than 20 minutes might not work</param>
        public EntityEffect(int eid, PlayerEffects effect, int amplifier, int duration)
        {
            if ((int)effect < 1)
                effect = PlayerEffects.MoveSpeed;
            if ((int)effect > 20)
                effect = PlayerEffects.Wither;
            this.EID = eid;
            this.Effect = effect;
            this.Amplifier = (sbyte)amplifier;
            this.Duration = (short)(duration * 20);
        }
		
        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Effect = (PlayerEffects)r.ReadSByte();
            Amplifier = r.ReadSByte();
            Duration = ReadVarInt(r);
            HideParticles = r.ReadBoolean();
            #if DEBUGPACKET
            if (Effect.ToString() == ((int)Effect).ToString())
                throw new NotImplementedException(Effect.ToString());
            #endif
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            w.Write((sbyte)Effect);
            w.Write(Amplifier);
            WriteVarInt(w, Duration);
            w.Write((bool)HideParticles);
        }
		
    }
}

