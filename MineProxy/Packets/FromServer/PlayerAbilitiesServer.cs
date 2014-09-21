using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class PlayerAbilitiesServer : PacketFromServer
    {
        public const byte ID = 0x39;

        public override byte PacketID { get { return ID; } }

        [Flags]
        public enum Abilities
        {
            Creative = 1,
            Flying = 2,
            CanFly = 4,
            /// <summary>
            /// Invulnerable
            /// </summary>
            God = 8,
        }

        public Abilities Flags { get; set; }

        public float FlySpeed { get; set; }

        public float WalkSpeed { get; set; }

        protected override void Parse(EndianBinaryReader r)
        {
            Flags = (Abilities)r.ReadByte();
            FlySpeed = r.ReadSingle();
            WalkSpeed = r.ReadSingle();
        }
		
        public PlayerAbilitiesServer()
        {
            Flags = 0;
            FlySpeed = 0.05f;//12
            WalkSpeed = 0.1f;//25
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((byte)Flags);
            w.Write((float)FlySpeed);
            w.Write((float)WalkSpeed);
        }
		
        public override string ToString()
        {
            return string.Format("[PlayerAbilities: {1}, Fly={2}, Walk={3}]", PacketID, Flags, FlySpeed, WalkSpeed);
        }
    }
}

