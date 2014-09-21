using System;
using MineProxy.Data;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class EntityStatus : PacketFromServer, IEntity
    {
        public const byte ID = 0x1A;

        public override byte PacketID { get { return ID; } }

        public int EID { get; set; }

        public EntityStatuses Status { get; set; }

        public EntityStatus()
        {
        }

        public EntityStatus(int eid, EntityStatuses type)
        {
            this.EID = eid;
            this.Status = type;
        }

        public override string ToString()
        {
            return base.ToString() + ": " + EID + " = " + Status;
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = r.ReadInt32();
            Status = (EntityStatuses)r.ReadByte();
#if DEBUG
            if (Status.ToString() == ((int)Status).ToString())
                throw new NotImplementedException(Status.ToString());
#endif
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((int)EID);
            w.Write((byte)Status);
        }
    }
}

