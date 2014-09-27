using System;
using MineProxy.Data;
using MiscUtil.IO;
using MineProxy.NBT;

namespace MineProxy.Packets
{
    public class UpdateEntityNBT : PacketFromServer, IEntity
    {
        public const byte ID = 0x49;

        public override byte PacketID { get { return ID; } }

        public int EID { get; set; }

        public Tag Tag { get; set; }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Tag = Tag.ReadTag(r);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            Tag.Write(w);
        }
    }
}

