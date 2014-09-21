using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class Effect : PacketFromServer
    {
        public override byte PacketID { get { return ID; } }

        public const byte ID = 0x28;

        public SoundEffects EffectID { get; set; }

        public CoordInt Position { get; set; }

        public int SoundData { get; set; }

        /// <summary>
        /// Possibly a bool, disable relative volume
        /// </summary>
        /// <value>The sound byte.</value>
        public byte SoundByte { get; set; }

        public override string ToString()
        {
            return base.ToString() + ": " + EffectID + " at " + Position + " data " + SoundData;
        }

        public Effect()
        {
        }

        public Effect(SoundEffects effect, CoordInt pos) : this(effect, pos, 0)
        {
        }

        public Effect(SoundEffects effect, CoordInt pos, int data = 0)
        {
            this.EffectID = effect;
            this.Position = pos;
            this.SoundData = data;
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EffectID = (SoundEffects)r.ReadInt32();
            Position = CoordInt.Read(r);
            SoundData = r.ReadInt32();
            SoundByte = r.ReadByte();
            //Debug.WriteLine(DebugPacket.Read(r));
#if DEBUG
            if (EffectID.ToString() == ((int)EffectID).ToString())
                throw new NotImplementedException(EffectID.ToString());
#endif
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((int)EffectID);
            Position.Write(w);
            w.Write((int)SoundData);
            w.Write((byte)SoundByte);
        }
    }
}

