using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class EntityHeadYaw : PacketFromServer, IEntity
    {
        public const byte ID = 0x19;

        public override byte PacketID { get { return ID; } }

        public int EID { get; set; }

        public double Yaw { get; set; }

        public override string ToString()
        {
            return string.Format("[EntityHeadYaw: {0}]", Yaw);
        }

        public EntityHeadYaw()
        {
        }

        public EntityHeadYaw(int eid, double yaw)
        {
            this.EID = eid;
            this.Yaw = yaw;
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Yaw = r.ReadSByte() * Math.PI / 128;
#if DEBUG
            //Console.WriteLine("===== " + EID + ": " + Yaw.ToString("0.0"));
#endif
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            w.Write((sbyte)(Yaw * 128 / Math.PI));
        }
    }
}

