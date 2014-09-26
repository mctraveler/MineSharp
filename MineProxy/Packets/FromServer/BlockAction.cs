using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class BlockAction : PacketFromServer
    {
        public override byte PacketID { get { return ID; } }

        public const byte ID = 0x24;

        public CoordInt Position { get; set; }

        public enum Instrument
        {
            Harp = 0,
            DoubleBass = 1,
            SnareDrum = 2,
            ClicksSticks = 3,
            BassDrum = 4,
        }

        /// <summary>
        /// Instrument type, Piston state, Unused(1) for chests
        /// </summary>
        public byte TypeState { get; set; }

        /// <summary>
        /// Instument pitch(0-24) or piston Direction, Chest state change
        /// </summary>
        public byte PitchDirection { get; set; }

        public BlockID BlockType { get; set; }

        public override string ToString()
        {
            return string.Format("[BlockAction: Position={1}, TypeState={2}, PitchDirection={3}]", PacketID, Position, TypeState, PitchDirection);
        }

        public static BlockAction Chest(CoordInt pos, bool open)
        {
            BlockAction ba = new BlockAction();
            ba.Position = pos;
            ba.TypeState = 1;
            ba.PitchDirection = open ? (byte)1 : (byte)0;
            return ba;
        }

        /// <param name='note'>
        /// 0 - 24
        /// </param>
        public static BlockAction NoteBlock(CoordInt pos, Instrument ins, byte note)
        {
            BlockAction ba = new BlockAction();
            ba.Position = pos;
            ba.TypeState = (byte)ins;
            if (note < 0 || note > 24)
                ba.PitchDirection = 0;
            else
                ba.PitchDirection = (byte)note;
            return ba;
        }

        public static BlockAction Piston(CoordInt pos, Face face, bool extend)
        {
            BlockAction ba = new BlockAction();
            ba.Position = pos;
            ba.TypeState = extend ? (byte)0 : (byte)1;
            ba.PitchDirection = (byte)face;
            return ba;
        }

        public BlockAction()
        {
            Position = new CoordInt();
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Position = CoordInt.Read(r);
            TypeState = r.ReadByte();
            PitchDirection = r.ReadByte();
            BlockType = (BlockID)ReadVarInt(r);

            #if DEBUGPACKET
            if (BlockType.ToString() == ((int)BlockType).ToString())
                throw new NotImplementedException(BlockType.ToString());
            #endif
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            Position.Write(w);
            w.Write((byte)TypeState);
            w.Write((byte)PitchDirection);
            WriteVarInt(w, (int)BlockType);
        }
    }
}

