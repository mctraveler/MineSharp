using System;
using System.Collections.Generic;
using System.Text;
using MiscUtil.IO;

namespace MineProxy.Packets.FromServer
{
    public class EntityProperties : PacketFromServer, IEntity
    {
        public const byte ID = 0x20;

        public override byte PacketID { get { return ID; } }

        public int EID { get; set; }

        public class Property
        {
            public string Key { get; set; }
            public double Val { get; set; }
            public List<PropData> List { get; set; }
            public Property()
            {
            }

            public Property(string key, double val)
            {
                this.Key = key;
                this.Val = val;
                this.List = new List<PropData>();
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("[{0} = {1}:\n", Key, Val.ToString("0.00"));
                foreach (var s in List)
                    sb.AppendLine(s.ToString());
                return sb.ToString();
            }
        }

        public class PropData
        {
            public long UUIDpart1 { get; set; }
            public long UUIDpart2 { get; set; }
            public double Amount { get; set; }
            public byte Operation { get; set; }

            public override string ToString()
            {
                return string.Format("[PropData: U1={0}, U2={1}, U3={2}, U4={3}]", UUIDpart1, UUIDpart2, Amount, Operation);
            }
        }

        public readonly List<Property> Properties = new List<Property>();

        public void AddMaxHealth(double health)
        {
            Properties.Add(new Property("generic.maxHealth", health));
        }

        public void AddMovementSpeed(double speed)
        {
            Properties.Add(new Property("generic.movementSpeed", speed));
        }

        public override string ToString()
        {
            string p = "";
            foreach (var kv in Properties)
                p += ", " + kv;
            return string.Format("[EntityProperties: EID={0}{1}]", EID, p);
        }

        public EntityProperties()
        {
        }

        public EntityProperties(int eid, double speed, double maxHealth)
        {
            this.EID = eid;
            AddMovementSpeed(speed);
            AddMaxHealth(maxHealth);
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EID = ReadVarInt(r);
            int count = r.ReadInt32();
            //Console.Write("DEBx2C: " + EID + " ");
            for(int n = 0; n < count; n++)
            {
                var p = new Property();
                p.Key = ReadString8(r);
                p.Val = r.ReadDouble();
                int sub = ReadVarInt(r);
                p.List = new List<PropData>();
                for (int s = 0; s < sub; s++)
                {
                    var sd = new PropData();
                    sd.UUIDpart1 = r.ReadInt64();
                    sd.UUIDpart2 = r.ReadInt64();
                    sd.Amount = r.ReadDouble();
                    sd.Operation = r.ReadByte();
                    p.List.Add(sd);
                }
                Properties.Add(p);
            }
            //Debug.WriteLine(this);
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, EID);
            w.Write((int)Properties.Count);
            foreach (var p in Properties)
            {
                WriteString8(w, p.Key);
                w.Write((double)p.Val);
                WriteVarInt(w, p.List.Count);
                foreach (var s in p.List)
                {
                    w.Write((long)s.UUIDpart1);
                    w.Write((long)s.UUIDpart2);
                    w.Write((double)s.Amount);
                    w.Write((byte)s.Operation);
                }
            }
        }

    }
}

