using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class EntityMetadata : PacketFromServer, IEntity
    {
        public const byte ID = 0x1C;

        public override byte PacketID { get { return ID; } }
		
        public int EID { get; set; }
		
        public Metadata Metadata { get; set; }
		
        public override string ToString()
        {
            return string.Format("[EntityMetadata: EID={0}, Metadata={1}]", EID, Metadata);
        }
		
        public EntityMetadata ()
        {
            Metadata = new MineProxy.Metadata();
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            Metadata = Metadata.Read(r);

/*#if DEBUG
            MineProxy.Debug.WriteLine(Metadata);
            if (Metadata.Fields.ContainsKey(0))
            {
                if ((Metadata.Fields [0].Byte & 0xE0) != 0)
                    throw new NotImplementedException();
            }
#endif*/
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            Metadata.Write(w, Metadata);
        }
    }
}

