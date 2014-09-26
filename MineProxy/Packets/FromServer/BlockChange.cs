using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class BlockChange : PacketFromServer
    {
        public const byte ID = 0x23;

        public override byte PacketID { get { return ID; } }
		
        public CoordInt Position { get; set; }
		
        public BlockID BlockType { get; set; }

        public byte Metadata { get; set; }

        public override string ToString()
        {
            return string.Format("[BlockChange: X={0}, Y={1}, Z={2}, Type={3}]", Position.X, Position.Y, Position.Z, BlockType);
        }
		
        public BlockChange()
        {
        }

        public BlockChange(CoordInt pos, Block block)
        {
            if (pos == null)
                throw new ArgumentNullException("pos");

            Position = pos;

            if (block != null)
            {
                BlockType = block.ID;
                Metadata = (byte)block.Meta;
            }
        }
        
        public BlockChange(CoordInt pos, byte blockID, byte meta)
        {
            if (pos == null)
                throw new ArgumentNullException("pos");
            Position = pos;
            BlockType = (BlockID)blockID;
            Metadata = meta;
        }
        
        public BlockChange(CoordInt pos, BlockID block)
        {
            if (pos == null)
                throw new ArgumentNullException("pos");
            Position = pos;
            BlockType = block;
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Position = CoordInt.Read(r);
            BlockType = (BlockID)ReadVarInt(r);
            Metadata = r.ReadByte();
            #if DEBUGPACKET
            if (BlockType.ToString() == ((int)BlockType).ToString())
                throw new NotImplementedException(BlockType.ToString());
            #endif
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            Position.Write(w);
            WriteVarInt(w, (int)BlockType);
            w.Write(Metadata);
        }
    }
}

