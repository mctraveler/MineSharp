using System;
using MiscUtil.IO;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.IO.Compression;
using MiscUtil.Conversion;

namespace MineProxy.NBT
{
    public abstract class Tag
    {
        public abstract byte TagID { get; }

        public virtual byte Byte
        {
            get{ throw new InvalidOperationException();}
            set{ throw new InvalidOperationException();}
        }

        public virtual short Short
        {
            get{ throw new InvalidOperationException();}
            set{ throw new InvalidOperationException();}
        }

        public virtual int Int
        {
            get{ throw new InvalidOperationException();}
            set{ throw new InvalidOperationException();}
        }

        public virtual long Long
        {
            get{ throw new InvalidOperationException();}
            set{ throw new InvalidOperationException();}
        }

        public virtual float Float
        {
            get{ throw new InvalidOperationException();}
            set{ throw new InvalidOperationException();}
        }

        public virtual double Double
        {
            get{ throw new InvalidOperationException();}
            set{ throw new InvalidOperationException();}
        }

        public virtual byte[] ByteArray
        {
            get{ throw new InvalidOperationException();}
            set{ throw new InvalidOperationException();}
        }
        
        public virtual int[] IntArray
        {
            get{ throw new InvalidOperationException();}
            set{ throw new InvalidOperationException();}
        }
        
        public virtual string String
        {
            get{ throw new InvalidOperationException();}
            set{ throw new InvalidOperationException();}
        }

        internal virtual List<TagString> ToListString()
        {
            throw new InvalidOperationException();
        }

        public virtual Tag this [string key]
        {
            get{ throw new InvalidOperationException();}
            set{ throw new InvalidOperationException();}
        }
		
        public static Tag ReadTag(Stream s)
        {
            using (GZipStream gzip = new GZipStream (s, CompressionMode.Decompress))
            {
                EndianBinaryReader r = new EndianBinaryReader(EndianBitConverter.Big, gzip);
                return Tag.ReadTag(r);
            }
        }

        public static Tag ReadTag(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (GZipStream gzip = new GZipStream (ms, CompressionMode.Decompress))
            {
                EndianBinaryReader r = new EndianBinaryReader(EndianBitConverter.Big, gzip);
                return Tag.ReadTag(r);
            }
        }

        public static Tag ReadTag(EndianBinaryReader r)
        {
            //Console.WriteLine ("Reading tag...");
            byte type = r.ReadByte();
            //Console.WriteLine ("Read tag " + type);
            if (type == 0)
                return new TagEnd();
            //string name = 
            TagString.Read(r);//.String;
            //Only for .dat files
            //if (name != "")
            //	throw new InvalidDataException ();

            Tag tag = ReadTagType(r, type);
            return tag;
        }

        public static Tag ReadNamedTag(EndianBinaryReader r, out string name)
        {
            byte type = r.ReadByte();
            if (type == 0)
            {
                name = null;
                return new TagEnd();
            }
            name = TagString.Read(r).String;

            Tag tag = ReadTagType(r, type);
            return tag;
        }

        public static Tag ReadTagType(EndianBinaryReader r, byte type)
        {
            switch (type)
            {
                case 0:
                    return TagEnd.Read(r);
                case 1:
                    return TagByte.Read(r);
                case 2:
                    return TagShort.Read(r);
                case 3:
                    return TagInt.Read(r);
                case 4:
                    return TagLong.Read(r);
                case 5:
                    return TagFloat.Read(r);
                case 6:
                    return TagDouble.Read(r);
                case 7:
                    return TagByteArray.Read(r);
                case 8:
                    return TagString.Read(r);
                case 9:
                    return TagListReader.Read(r);
                case 10:
                    return TagCompound.Read(r);
                case 11:
                    return TagIntArray.Read(r);
                default:
                    throw new NotImplementedException("NBT Tag type: " + type);
            }
        }

        public byte[] WriteToBuffer()
        {
            using (var ms = new MemoryStream())
            using (var writer = new EndianBinaryWriter(EndianBitConverter.Big, ms))
            {
                Write(writer);
                return ms.ToArray();
            }
        }
        
        public void Write(Stream s)
        {
            using (var writer = new EndianBinaryWriter(EndianBitConverter.Big, s))
                Write(writer);
        }
        
        public void Write(EndianBinaryWriter writer)
        {
            writer.Write(this.TagID);
            //name
            writer.Write((short)0);
            WriteType(writer);
        }

        public void Write(EndianBinaryWriter writer, string name)
        {
            //type
            writer.Write(this.TagID);

            //name
            TagString s = new TagString(name);
            s.WriteType(writer);

            //value
            WriteType(writer);
        }

        public abstract void WriteType(EndianBinaryWriter writer);

        protected static string Indent(string s)
        {
            string o = "";
            foreach (string l in s.Split('\n'))
                o += "  " + l + "\n";
            if (o == "")
                return o;
            else
                return o.Substring(0, o.Length - 1);
        }

    }

    class TagByte : Tag
    {
        public override byte TagID { get { return 1; } }

        public override byte Byte { get; set ; }

        public TagByte()
        {

        }

        public TagByte(byte val)
        {
            Byte = val;
        }

        public static TagByte Read(EndianBinaryReader r)
        {
            TagByte t = new TagByte();
            t.Byte = r.ReadByte();
            return t;
        }

        public override void WriteType(EndianBinaryWriter writer)
        {
            writer.Write(this.Byte);
        }

        public override string ToString()
        {
            return Byte + " b";
        }
    }

    class TagShort : Tag
    {
        public override byte TagID { get { return 2; } }

        public override short Short { get; set ; }

        public TagShort(short value)
        {
            this.Short = value;
        }

        public TagShort()
        {

        }

        public static TagShort Read(EndianBinaryReader r)
        {
            return new TagShort(r.ReadInt16());
        }

        public override void WriteType(EndianBinaryWriter writer)
        {
            writer.Write(Short);
        }

        public override string ToString()
        {
            return Short + " s";
        }
    }

    class TagInt : Tag
    {
        public override byte TagID { get { return 3; } }

        public override int Int { get; set ; }

        public TagInt(int val)
        {
            this.Int = val;
        }

        public TagInt()
        {

        }

        public static TagInt Read(EndianBinaryReader r)
        {
            return new TagInt(r.ReadInt32());
        }

        public override void WriteType(EndianBinaryWriter writer)
        {
            writer.Write(Int);
        }

        public override string ToString()
        {
            return Int + " i";
        }
    }

    class TagLong : Tag
    {
        public override byte TagID { get { return 4; } }

        public override long Long { get; set ; }

        public static TagLong Read(EndianBinaryReader r)
        {
            TagLong t = new TagLong();
            t.Long = r.ReadInt64();
            return t;
        }

        public override void WriteType(EndianBinaryWriter writer)
        {
            writer.Write(Long);
        }

        public override string ToString()
        {
            return Long + " l";
        }
    }

    class TagFloat : Tag
    {
        public override byte TagID { get { return 5; } }

        public override float Float { get; set ; }

        public TagFloat(float val)
        {
            Float = val;
        }

        public TagFloat()
        {

        }

        public static TagFloat Read(EndianBinaryReader r)
        {
            return new TagFloat(r.ReadSingle());
        }

        public override void WriteType(EndianBinaryWriter writer)
        {
            writer.Write(Float);
        }

        public override string ToString()
        {
            return Float + " f";
        }
    }

    class TagDouble : Tag
    {
        public override byte TagID { get { return 6; } }

        public override double Double { get; set ; }

        public TagDouble(double val)
        {
            Double = val;
        }

        public TagDouble()
        {

        }

        public static TagDouble Read(EndianBinaryReader r)
        {
            return new TagDouble(r.ReadDouble());
        }

        public override void WriteType(EndianBinaryWriter writer)
        {
            writer.Write(Double);
        }

        public override string ToString()
        {
            return Double + " d";
        }
    }

    class TagByteArray : Tag
    {
        public override byte TagID { get { return 7; } }
        
        public override byte[] ByteArray { get; set ; }
        
        public static TagByteArray Read(EndianBinaryReader r)
        {
            int length = r.ReadInt32();
            TagByteArray t = new TagByteArray();
            t.ByteArray = r.ReadBytes(length);
            return t;
        }
        
        public override void WriteType(EndianBinaryWriter w)
        {
            w.Write((int)ByteArray.Length);
            w.Write(ByteArray);
        }
        
        public override string ToString()
        {
            return ByteArray.Length + " bytes";
        }
    }
    
    class TagIntArray : Tag
    {
        public override byte TagID { get { return 11; } }
        
        public override int[] IntArray { get; set ; }
        
        public static TagIntArray Read(EndianBinaryReader r)
        {
            int length = r.ReadInt32();
            //Not sure if length is in bytes or ints
            TagIntArray t = new TagIntArray();
            t.IntArray = new int[length];
            for (int n = 0; n < length; n++)
                t.IntArray [n] = r.ReadInt32();
            return t;
        }
        
        public override void WriteType(EndianBinaryWriter w)
        {
            w.Write((int)IntArray.Length);
            for (int n = 0; n < IntArray.Length; n++)
                w.Write(IntArray [n]);
        }
        
        public override string ToString()
        {
            return IntArray.Length + " integers";
        }
    }
    
    class TagString : Tag
    {
        public override byte TagID { get { return 8; } }

        public override string String { get; set ; }

        public TagString()
        {
        }

        public TagString(string s)
        {
            this.String = s;
        }

        public static TagString Read(EndianBinaryReader r)
        {
            ushort length = r.ReadUInt16();
            byte[] buffer = r.ReadBytes(length);
            if (buffer.Length != length)
                throw new InvalidDataException();

            TagString s = new TagString(Encoding.UTF8.GetString(buffer));
            return s;
        }

        public override void WriteType(EndianBinaryWriter writer)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(String);
            writer.Write((short)buffer.Length);
            writer.Write(buffer);
        }

        public override string ToString()
        {
            return "\"" + String + "\"";
        }
    }

    static class TagListReader
    {
        public static Tag Read(EndianBinaryReader r)
        {
            byte tagID = r.ReadByte();
            switch (tagID)
            {
                case 0:
                    return TagList<TagEnd>.ReadList(r);
                case 1:
                    return TagList<TagByte>.ReadList(r);
                case 2:
                    return TagList<TagShort>.ReadList(r);
                case 3:
                    return TagList<TagInt>.ReadList(r);
                case 4:
                    return TagList<TagLong>.ReadList(r);
                case 5:
                    return TagList<TagFloat>.ReadList(r);
                case 6:
                    return TagList<TagDouble>.ReadList(r);
                case 7:
                    return TagList<TagByteArray>.ReadList(r);
                case 8:
                    return TagList<TagString>.ReadList(r);
                case 9:
                    throw new NotImplementedException();
                case 10:
                    return TagList<TagCompound>.ReadList(r);
                case 11:
                    return TagIntArray.Read(r);
                default:
                    throw new NotImplementedException("NBT Type: " + tagID);
            }
        }
    }

    public class TagList<T> : Tag where T:Tag, new()
    {
        public override byte TagID { get { return 9; } }

        public List<T> List = new List<T>();

        public IEnumerator<T> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        public T this [int index]
        {
            get
            {
                return List [index];
            }
            set
            {
                this.List.Insert(index, value);
            }
        }

        internal override List<TagString> ToListString()
        {
            return List as List<TagString>;
        }

        public void Add(T item)
        {
            List.Add(item);
        }

        public void Remove(T item)
        {
            List.Remove(item);
        }
		
        public void Sort(Comparison<T> c)
        {
            List.Sort(c);
        }
		
        public static Tag ReadList(EndianBinaryReader r)
        {
            TagList<T> tl = new TagList<T>();

            int length = r.ReadInt32();
            T item = new T();

            for (int n = 0; n < length; n++)
            {
                T t = (T)ReadTagType(r, item.TagID);
                tl.List.Add(t);
            }
            return tl;
        }

        public override void WriteType(EndianBinaryWriter writer)
        {
            T item = new T();
            writer.Write(item.TagID);
            writer.Write((int)List.Count);
            foreach (T i in List)
            {
                ((Tag)i).WriteType(writer);
            }
        }

        public override string ToString()
        {
            string s = List.Count + " items (list)";
            foreach (Tag t in List)
                s += "\n" + Tag.Indent(t.ToString());
            return s;
        }
    }

    public class TagCompound : Tag
    {
        public override byte TagID { get { return 10; } }

        /// <summary>
        /// Only for internal use, use [] index directly instead
        /// </summary>
        public readonly Dictionary<string, Tag> CompoundList = new Dictionary<string, Tag>();

        public override Tag this [string key]
        {
            get
            {
                if (CompoundList.ContainsKey(key) == false)
                    return null;
                return CompoundList [key];
            }
            set
            {
                CompoundList [key] = value;
            }
        }

        public static TagCompound Read(EndianBinaryReader r)
        {
            TagCompound tc = new TagCompound();
            while (true)
            {
                string name;
                Tag t = Tag.ReadNamedTag(r, out name);
                if (t is TagEnd)
                    break;
                tc.CompoundList.Add(name, t);
            }
            return tc;
        }

        public override void WriteType(EndianBinaryWriter writer)
        {
            foreach (var kvp in CompoundList)
            {
                kvp.Value.Write(writer, kvp.Key);
            }
            //TagEnd
            writer.Write((byte)0);
        }

        public override string ToString()
        {
            string s = CompoundList.Count + " items (compound)";
            foreach (var kvp in CompoundList)
                s += "\n" + Indent(kvp.Key + ": " + kvp.Value.ToString());
            return s;
        }
    }

    class TagEnd : Tag
    {
        public override byte TagID { get { throw new NotImplementedException(); } }

        public static TagString Read(EndianBinaryReader r)
        {
            throw new NotImplementedException();
        }

        public override void WriteType(EndianBinaryWriter writer)
        {
            writer.Write((byte)0);
        }
    }

}

