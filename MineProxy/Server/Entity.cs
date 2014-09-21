using System;
using System.Drawing;
using MineProxy.Packets;

namespace MineProxy
{
    public abstract class Entity
    {
        public readonly int EID;
		
        public Entity(int eid)
        {
            this.EID = eid;
        }
		
        public bool Onfire { get; set; }
		
        public bool Crouched { get; set; }

        public bool Riding { get; set; }

        public bool Sprinting { get; set; }
        /// <summary>
        /// eating, blocking, any right click action?
        /// </summary>
        public bool Eating { get; set; }
		
        /// <summary>
        /// 300 = normal, reduced 3/tick, 60/second, when belo -19 entity gets hurt and it is reset to 0
        /// </summary>
        public int Oxygen { get; set; }
		
        //Index 8
        public Color Potion { get; set; }
		
        /// <summary>
        /// 0 for grown animals,
        /// babies start at -23999
        /// </summary>
        public int Maturity { get; set; }
		
        protected virtual bool ImportMeta(int id, int val)
        {
            return false;
        }

        protected virtual bool ImportMeta(int id, short val)
        {
            return false;
        }

        protected virtual bool ImportMeta(int id, sbyte val)
        {
            return false;
        }
        
        protected virtual bool ImportMeta(int id, float val)
        {
            return false;
        }
        
        protected virtual bool ImportMeta(int id, string val)
        {
            return false;
        }
        
        protected virtual bool ImportMeta(int id, SlotItem val)
        {
            return false;
        }
        
        public void Update(EntityMetadata meta)
        {
            UpdateMeta(meta.Metadata);
        }
		
        protected void UpdateMeta(Metadata meta)
        {
            foreach (var kvp in meta.Fields)
            {
                switch (kvp.Key)
                {
                    case 0: //Index 0, flags
				
                        sbyte flag = kvp.Value.Byte;
                        Onfire = (flag & 0x01) != 0;
                        Crouched = (flag & 0x01) != 0;
                        Riding = (flag & 0x01) != 0;
                        Sprinting = (flag & 0x01) != 0;
                        Eating = (flag & 0x01) != 0;
                        Debug.Assert((flag & 0xC0) == 0);
                        break;
			
                    case 1://Index 1, drowning
                        Oxygen = kvp.Value.Short;
                        Debug.Assert(Oxygen <= 300 & Oxygen > -20);
                        break;
					
                    case 5: //Horses
                        break;
                    case 6: //Horses
                        break;

                    case 8://Index 8, potion color
                        int c = kvp.Value.Int;
                        Potion = Color.FromArgb(0, (c >> 16) & 0xFF, (c >> 8) & 0xFF, c & 0xFF);
                        Debug.Assert((c & 0xFF000000) == 0);
                        break;

                    case 18: //Horses
                        break;
                    case 19: //Boat
                        break;

                    default:
                        switch (kvp.Value.Type)
                        {
                            case MetaType.Byte:
                                if (ImportMeta(kvp.Key, kvp.Value.Byte))
                                    continue;
                                break;
                            case MetaType.Short:
                                if (ImportMeta(kvp.Key, kvp.Value.Short))
                                    continue;
                                break;
                            case MetaType.Int:
                                if (ImportMeta(kvp.Key, kvp.Value.Int))
                                    continue;
                                break;
                            case MetaType.String:
                                if (ImportMeta(kvp.Key, kvp.Value.String))
                                    continue;
                                break;
                            case MetaType.Item:
                                if (ImportMeta(kvp.Key, kvp.Value.Item))
                                    continue;
                                break;
                            case MetaType.Float:
                                if (ImportMeta(kvp.Key, kvp.Value.Float))
                                    continue;
                                break;
#if DEBUG
                            default:
                                throw new NotImplementedException("Unhandled metadata type " + kvp.Value);
#endif
                        }
                        break;
                }
            }
        }
		
        protected virtual void ExportMeta(Metadata meta)
        {
            meta.SetByte(0, (sbyte)(
			         (Onfire ? 0x01 : 0) |
                (Crouched ? 0x02 : 0) |
                (Riding ? 0x04 : 0) |
                (Sprinting ? 0x08 : 0) |
                (Eating ? 0x10 : 0)
			         ));
            meta.SetInt(1, Oxygen);
            meta.SetInt(8, (Potion.R << 16) | (Potion.G << 8) | Potion.B);
            meta.SetInt(12, Maturity);
        }
    }
}

