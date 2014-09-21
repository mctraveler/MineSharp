using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class UpdateBlockEntity : PacketFromServer
    {
        public const byte ID = 0x35;

        public override byte PacketID { get { return ID; } }
        
        public CoordInt Pos { get; set; }
        
        public enum Actions
        {
            /// <summary>
            /// Mob inside spawner defined in data?
            /// </summary>
            MobSpawner = 1,
            Unknown2 = 2,
            Unknown3 = 3,
            /// <summary>
            /// From server after player placed block.
            /// When wither spawn after block placed
            /// </summary>
            Unknown4 = 4,
        }
        
        public Actions Action { get; set; }

        public byte[] Data { get; set; }

        public override string ToString()
        {
            return string.Format("[UpdateTileEntity: Pos={1}, Action={2}, Data={3}]", PacketID, Pos, Action, Data.Length);
        }

        public UpdateBlockEntity()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Pos = CoordInt.Read(r);
            Action = (Actions)r.ReadByte();
            int length = r.ReadInt16(); //Changes: compressed length
            if (length >= 0)
                Data = r.ReadBytesOrThrow(length);

#if DEBUG
            if (Action == Actions.Unknown2)
                Console.WriteLine(this);
            if (Action == Actions.Unknown3)
                Console.WriteLine(this);
            if (Action == Actions.Unknown4)
                Console.WriteLine(this);

            if (Action.ToString() == ((int)Action).ToString())
                throw new NotImplementedException(Action.ToString());
#endif
        }
        
        protected override void Prepare(EndianBinaryWriter w)
        {
            Pos.Write(w);
            w.Write((byte)Action);

            if (Data == null)
                w.Write((short)-1);
            else
            {
                w.Write((short)Data.Length);
                w.Write(Data);
            }
        }
        
    }
}

