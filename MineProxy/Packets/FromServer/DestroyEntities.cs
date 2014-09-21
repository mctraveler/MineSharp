using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class DestroyEntities : PacketFromServer
    {
        public const byte ID = 0x13;

        public override byte PacketID { get { return ID; } }

        public int[] EIDs { get; set; }

        public override string ToString()
        {
            return string.Format("[DestroyEntity: EID={0}]", EIDs.Length);
        }

        public DestroyEntities()
        {
        }

        public DestroyEntities(int eid)
        {
            EIDs = new int[]{ eid };
        }

        protected override void Parse(EndianBinaryReader r)
        {
            int count = ReadVarInt(r);
            EIDs = new int[count];
            for (int i = 0; i < count; i++)
                EIDs [i] = ReadVarInt(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            //New format
            WriteVarInt(w, EIDs.Length);
            foreach (int eid in EIDs)
                WriteVarInt(w, eid);
        }
    }
}

