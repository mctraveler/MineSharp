using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class SpawnPlayer : PacketFromServer, IEntity
    {
        public const byte ID = 0x0C;

        public override byte PacketID { get { return ID; } }

        public int EID { get; set; }

        public Guid PlayerUUID { get; set; }

        public CoordDouble Position { get; set; }

        public double Yaw { get; set; }

        public double Pitch { get; set; }

        /// <summary>
        /// The item the player is currently holding, negative values crashes the client.
        /// </summary>
        public short CurrentItem { get; set; }

        public Metadata Meta { get; set; }

        public SpawnPlayer()
        {
            Debug.WriteLine("Constructed " + this);
        }

        public SpawnPlayer(int eid, Client player)
        {
            EID = eid;
            PlayerUUID = player.UUID;
            Position = new CoordDouble();
            Meta = new Metadata();
            Meta.SetFloat(17, 0);
            Meta.SetByte(0, 0);
            Meta.SetByte(16, 0);
            Meta.SetShort(1, 300);
            Meta.SetString(2, "");
            Meta.SetInt(18, 0);
            Meta.SetByte(3, 0);
            Meta.SetByte(4, 0);
            Meta.SetFloat(6, 10);
            Meta.SetInt(7, 0);
            Meta.SetByte(8, 0);
            Meta.SetByte(9, 0);
            Meta.SetByte(10, 0);
        }

        public override string ToString()
        {
            return string.Format("[SpawnPlayer: EID={0}, UUID={1}, Position={2}]", EID, PlayerUUID, Position);
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            PlayerUUID = new Guid(r.ReadBytesOrThrow(16));
            Position = ReadAbsInt(r);
            Yaw = r.ReadSByte() * Math.PI / 128;
            Pitch = r.ReadSByte() * Math.PI / 128;
            CurrentItem = r.ReadInt16();
            try
            {
                Meta = Metadata.Read(r);
            }
            catch (Exception)
            {
                Console.WriteLine("BadMeta: " + BitConverter.ToString(PacketBuffer));
            }
            Debug.WriteLine("Parsed " + this);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            Debug.WriteLine("Prepare " + this);
            WriteVarInt(w, EID);
            w.Write(PlayerUUID.ToByteArray());
            WriteAbsInt(w, Position);
            w.Write((sbyte)(Yaw * 128 / Math.PI));
            w.Write((sbyte)(Pitch * 128 / Math.PI));
            w.Write((short)CurrentItem);
            Metadata.Write(w, Meta);
        }
    }
}

