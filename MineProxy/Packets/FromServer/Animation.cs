using System;
using MineProxy.Data;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class Animation : PacketFromServer, IEntity
    {
        public const byte ID = 0x0B;

        public override byte PacketID { get { return ID; } }

        public int EID { get; set; }

        public Animations Animate { get; set; }

        public override string ToString()
        {
            return string.Format("[Animation: {0}, {1}]", EID, Animate);
        }

        public Animation()
        {
            
        }

        public Animation(int eid, Animations anim)
        {
            EID = eid;
            Animate = anim;
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Animate = (Animations)r.ReadByte();

            #if DEBUGPACKET
            if (Animate.ToString() == ((int)Animate).ToString())
                throw new NotImplementedException(Animate.ToString());
            #endif

        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            w.Write((byte)Animate);
        }
    }
}

