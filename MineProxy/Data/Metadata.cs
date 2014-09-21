using System;
using System.Collections.Generic;
using MineProxy;
using System.IO;
using System.Drawing;
using MiscUtil.IO;
using MineProxy.Packets;

namespace MineProxy
{
    public enum MetaType
    {
        Byte = 0,
        Short = 1,
        Int = 2,
        Float = 3,
        String = 4,
        Item = 5,
        Vector = 6,
        Bytes12 = 7,
    }
	
    public class MetaField
    {
        public MetaType Type { get; set; }

        public int ID { get; set; }
		
        public sbyte Byte { get; set; }

        public short Short { get; set; }

        public int Int { get; set; }

        public float Float { get; set; }

        public string String { get; set; }

        public byte[] Data { get; set; }

        public SlotItem Item { get; set; }

        public CoordInt Vector { get; set; }

        public MetaField(int id, MetaType type)
        {
            this.ID = id;
            this.Type = type;
        }
		
        public override string ToString()
        {
            switch (Type)
            {
                case MetaType.Byte:
                    return ID + ": " + Byte + "b";
                case MetaType.Short:
                    return ID + ": " + Short + "s";
                case MetaType.Int:
                    return ID + ": " + Int.ToString("X") + "ix";
                case MetaType.Float:
                    return ID + ": " + Float + "f";
                case MetaType.String:
                    return ID + ": \"" + String + "\"";
                case MetaType.Item:
                    return ID + " Item: " + Item;
                case MetaType.Vector:
                    return ID + " Vector: " + Vector;
                default:
                    throw new NotImplementedException();
            }
        }
    }
	
    public class Metadata
    {
        public readonly Dictionary<int, MetaField> Fields = new Dictionary<int, MetaField>();

		#region Helpers Setter/Getters
        public void SetString(int id, string value)
        {
            MetaField f = new MetaField(id, MetaType.String);
            f.String = value;
            Fields.Remove(id);
            Fields.Add(id, f);
        }
				
        public void SetInt(int id, int value)
        {
            MetaField f = new MetaField(id, MetaType.Int);
            f.Int = value;
            Fields.Remove(id);
            Fields.Add(id, f);
        }
		
        public void SetShort(int id, short value)
        {
            MetaField f = new MetaField(id, MetaType.Short);
            f.Short = value;
            Fields.Remove(id);
            Fields.Add(id, f);
        }
		
        public void SetByte(int id, sbyte value)
        {
            MetaField f = new MetaField(id, MetaType.Byte);
            f.Byte = value;
            Fields.Remove(id);
            Fields.Add(id, f);
        }

        public void SetFloat(int id, float value)
        {
            MetaField f = new MetaField(id, MetaType.Float);
            f.Float = value;
            Fields.Remove(id);
            Fields.Add(id, f);
        }

		#endregion
		
        public override string ToString()
        {
            string s = "";
            foreach (MetaField f in Fields.Values)
            {
                s += f + ", ";
            }
            if (s == "")
                return "-";
            return s;			
        }
		
        public static Metadata Read(EndianBinaryReader r)
        {
            Metadata m = new Metadata();
            
            while (true)
            {
                byte f = r.ReadByte();
                if (f == 0x7F)
                    return m;
                
                MetaField mf = new MetaField(f & 0x1F, (MetaType)(f >> 5));
                switch (mf.Type)
                {
                    case MetaType.Byte:
                        mf.Byte = r.ReadSByte();
                        break;
                    case MetaType.Short:
                        mf.Short = r.ReadInt16();
                        break;
                    case MetaType.Int:
                        mf.Int = r.ReadInt32();
                        break;
                    case MetaType.Float:
                        mf.Float = r.ReadSingle();
                        break;
                    case MetaType.String:
                        mf.String = Packet.ReadString8(r);
                        break;
                    case MetaType.Item:
                        mf.Item = SlotItem.Read(r);
                        break;
                    case MetaType.Vector:
                        mf.Vector = new CoordInt();
                        mf.Vector.X = r.ReadInt32();
                        mf.Vector.Y = r.ReadInt32();
                        mf.Vector.Z = r.ReadInt32();
                        break;
                    case MetaType.Bytes12:
                        mf.Data = r.ReadBytesOrThrow(12);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                m.Fields.Add(mf.ID, mf);				
            }
        }
		
        public static void Write(EndianBinaryWriter w, Metadata m)
        {
            if (m == null)
            {
                w.Write((byte)0x7F);
                return;
            }
            foreach (MetaField mf in m.Fields.Values)
            {
                w.Write((byte)((int)mf.Type << 5 | mf.ID));
                switch (mf.Type)
                {
                    case MetaType.Byte:
                        w.Write(mf.Byte);
                        break;
                    case MetaType.Short:
                        w.Write(mf.Short);
                        break;
                    case MetaType.Int:
                        w.Write(mf.Int);
                        break;
                    case MetaType.Float:
                        w.Write(mf.Float);
                        break;
                    case MetaType.String:
                        Packet.WriteString8(w, mf.String);
                        break;
                    case MetaType.Item:
                        //when this is true, we can remove that parameter
                        SlotItem.Write(w, mf.Item); 
                        break;
                    case MetaType.Vector:
                        w.Write(mf.Vector.X);
                        w.Write(mf.Vector.Y);
                        w.Write(mf.Vector.Z);
                        break;
                    case MetaType.Bytes12:
                        w.Write(mf.Data);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            w.Write((byte)0x7F);
        }
    }
}

