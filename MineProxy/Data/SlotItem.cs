using System;
using System.IO.Compression;
using System.Collections.Generic;
using MineProxy;
using System.IO;
using System.Text;
using System.Threading;
using MineProxy.NBT;
using MiscUtil.IO;

namespace MineProxy
{
    public partial class SlotItem
    {        
        public BlockID ItemID { get; set; }
        
        public int Count { get; set; }
        
        public int Uses { get; set; }
        
        public Tag Data { get; set; }
        
        private SlotItem()
        {
        }
        
        public SlotItem(BlockID type, int count, int uses)
        {
            this.ItemID = type;
            this.Count = (byte)count;
            this.Uses = uses;
        }
        
        public SlotItem(BlockID type, int count, int uses, string name)
        {
            this.ItemID = type;
            this.Count = (byte)count;
            this.Uses = uses;

            var t = new TagCompound();
            var td = new TagCompound();
            t ["display"] = td;
            td ["Name"] = new TagString(name);

            this.Data = t;
        }
        
        public override string ToString()
        {
            return "[SlotItem: " + ItemID + ", " + Count + " pcs, " + Uses + " usage, " + Data + "]";
        }
        
        public static SlotItem Read(EndianBinaryReader r)
        {
            SlotItem i = new SlotItem();
            
            i.ItemID = (BlockID)r.ReadInt16();
            if (i.ItemID < 0)
                return null;
            #if DEBUGPACKET
            if (((int)i.ItemID).ToString() == i.ItemID.ToString())
                throw new NotImplementedException("BlockID: " + (int)i.ItemID);
            #endif

            i.Count = r.ReadByte();
            i.Uses = r.ReadInt16();

            //Metadata   
            i.Data = Tag.ReadTag(r);
            //Debug.WriteLine(i + ": " + i.Data);

            return i;

        }
        
        public static void Write(EndianBinaryWriter w, SlotItem i)
        {
            if (i == null || i.ItemID < 0)
            {
                w.Write((short)-1);
                return;
            }
            w.Write((short)i.ItemID);
            w.Write((byte)i.Count);
            w.Write((short)i.Uses);

            if (i.ItemID == BlockID.BareHands)
                return;

            if (i.Data == null)
            {
                w.Write((byte)0);
                return;
            }
            if (i.Data is TagEnd)
                w.Write((byte)0);
            else
                i.Data.Write(w);
        }
    }
}

