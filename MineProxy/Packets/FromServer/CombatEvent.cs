using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class CombatEvent : PacketFromServer
    {
        public const byte ID = 0x42;

        public override byte PacketID { get { return ID; } }

        public enum Action
        {
            Enter = 0,
            End = 1,
            Dead = 2,
        }

        public Action Event { get; set; }

        public int Duration { get; set; }

        public int EntityID { get; set; }

        public int PlayerID { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return string.Format("[CombatEvent: PacketID={0}, Event={1}, Duration={2}, EntityID={3}, PlayerID={4}, Message={5}]", PacketID, Event, Duration, EntityID, PlayerID, Message);
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Event = (Action)r.ReadByte();
            if (Event == Action.End)
            {
                Duration = ReadVarInt(r);
                EntityID = r.ReadInt32();
            }
            if (Event == Action.Dead)
            {
                PlayerID = ReadVarInt(r);
                EntityID = r.ReadInt32();
                Message = ReadString8(r);
            }
        }

        protected override void Prepare(EndianBinaryWriter w)
        {			
            w.Write((byte)Event);
            if (Event == Action.End)
            {
                WriteVarInt(w, Duration);
                w.Write((int)EntityID);
            }
            if (Event == Action.Dead)
            {
                WriteVarInt(w, PlayerID);
                w.Write((int)EntityID);
                WriteString8(w, Message);
            }
        }
    }
}

