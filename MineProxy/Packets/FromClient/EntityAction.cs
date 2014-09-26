using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class EntityAction : PacketFromClient, IEntity
    {
        public const byte ID = 0x0B;

        public override byte PacketID { get { return ID; } }

        public int EID { get; set; }

        public enum Actions
        {
            Crounch = 0,
            Uncrounch = 1,
            LeaveBed = 2,
            StartSprinting = 3,
            StopSprinting = 4,
            /// <summary>
            /// When space is release during horseride
            /// </summary>
            HorseJump = 5,
            ShowInventory = 6,
        }

        public Actions Action { get; set; }

        public int JumpBoost { get; set; }

        public override string ToString()
        {
            return string.Format("[EntityAction: EID={0}, Action={1}]", EID, Action);
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Action = (Actions)r.ReadByte();
            JumpBoost = ReadVarInt(r);
            #if DEBUGPACKET
            if (Action.ToString() == ((int)Action).ToString())
                throw new NotImplementedException(Action.ToString());
            #endif
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            w.Write((byte)Action);
            WriteVarInt(w, JumpBoost);
        }
    }
}

