using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class SpawnObject : PacketFromServer
    {
        public const byte ID = 0x0E;
        public override byte PacketID { get { return ID; } }
		
        public int EID;
		
        public Vehicles Type { get; set; }

        public CoordDouble Position { get; set; }
		
        public sbyte Yaw { get; set; }

        public sbyte Pitch { get; set; }

        /// <summary>
        /// OLD: Who emitted the item
        /// Depending on type:
        /// ItemFrame: 0-3: South, West, North, East
        /// FallingBlock: BlockID | (Metadata << 0xC)
        /// Projectiles: thrower EID
        /// SplashPotion: potion data value
        /// </summary>
        public int SourceEntity { get; set; }

        public CoordInt Velocity { get; set; }

        public override string ToString()
        {
            return string.Format("[SpawnObject: EID={0}, Type={1}, Pos={2}, SourceEntity={3}, Yaw={4}, Pitch={5}, Vel={6} ]", EID, Type, Position, SourceEntity, Yaw, Pitch, Velocity);
        }
		
        public SpawnObject()
        {
            Velocity = new CoordInt();
        }
		
        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Type = (Vehicles)r.ReadByte();
#if DEBUG
            if (Type.ToString() == ((int)Type).ToString())
                throw new NotImplementedException(Type.ToString());
#endif
            if (Type == Vehicles.Unknown)
                Console.WriteLine();
            Position = ReadAbsInt(r);

            Yaw = r.ReadSByte();
            Pitch = r.ReadSByte();

            SourceEntity = r.ReadInt32();
            if (SourceEntity <= 0)
                return;
            short vx = r.ReadInt16();
            short vy = r.ReadInt16();
            short vz = r.ReadInt16();			
            Velocity = new CoordInt(vx, vy, vz);
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            w.Write((byte)Type);
            WriteAbsInt(w, Position);
            w.Write((sbyte)Yaw);
            w.Write((sbyte)Pitch);
            w.Write((int)SourceEntity);
            if (SourceEntity <= 0)
                return;
            w.Write((short)Velocity.X);
            w.Write((short)Velocity.Y);
            w.Write((short)Velocity.Z);
        }
    }
}

