using System;
using MiscUtil.IO;
using System.IO;

namespace MineProxy.Packets
{
    public class WorldBorder : PacketFromServer
    {
        public const byte ID = 0x44;

        public override byte PacketID { get { return ID; } }

        public enum Actions
        {
            SetSize = 0,
            LerpSize = 1,
            SetCenter = 2,
            Initialize = 3,
            WarningTime = 4,
            WarningBlocks = 5,
        }

        public Actions Action { get; set; }

        public double Radius { get; set; }

        public double OldRadius { get; set; }

        public double NewRadius { get; set; }

        public long Speed { get; set; }

        public double X { get; set; }

        public double Z { get; set; }

        public int WarningTime { get; set; }

        public int WarningBlocks { get; set; }

        public int U { get; set; }

        public override string ToString()
        {
            switch (Action)
            {
                case Actions.SetSize:
                    return string.Format("[WorldBorder: {0}, Radius={1}]", Action, Radius);
                case Actions.LerpSize:
                    return string.Format("[WorldBorder: {0}, OldRadius={1}, NewRadius={2}, Speed={3}]", Action, OldRadius, NewRadius, Speed);
                case Actions.SetCenter:
                    return string.Format("[WorldBorder: {0}, X={1}, Y={2}]", Action, X, Z);
                case Actions.Initialize:
                    return string.Format("[WorldBorder: {0}, A={1}, B={2}, C={3}, D={4}, E={5}, F={6}]", Action, X, Z, OldRadius, NewRadius, WarningTime, WarningBlocks);
                case Actions.WarningTime:
                    return string.Format("[WorldBorder: {0}, {1}]", Action, WarningTime);
                case Actions.WarningBlocks:
                    return string.Format("[WorldBorder: {0}, {1}]", Action, WarningBlocks);
                default:
                    return "Unknown action: " + Action;
            }
        }

        protected override void Parse(EndianBinaryReader r)
        {
            #if DEBUG
            Action = (Actions)ReadVarInt(r);
            switch (Action)
            {
                case Actions.SetSize:
                    Radius = r.ReadDouble();
                    break;

                case Actions.LerpSize:
                    OldRadius = r.ReadDouble();
                    NewRadius = r.ReadDouble();
                    Speed = ReadVarLong(r);
                    break;

                case Actions.SetCenter:
                    X = r.ReadDouble();
                    Z = r.ReadDouble();
                    break;

                case Actions.Initialize:
                    X = r.ReadDouble();
                    Z = r.ReadDouble();
                    OldRadius = r.ReadDouble();
                    NewRadius = r.ReadDouble();
                    Speed = ReadVarLong(r);
                    U = ReadVarInt(r);
                    //We got 7 bytes left here, more than 2 varint
                    WarningTime = ReadVarInt(r);
                    WarningBlocks = ReadVarInt(r);
                    break;

                case Actions.WarningTime:
                    WarningTime = ReadVarInt(r);
                    break;

                case Actions.WarningBlocks:
                    WarningBlocks = ReadVarInt(r);
                    break;

                default:
                    throw new NotImplementedException();
            }
            #endif
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
