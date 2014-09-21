using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    /// <summary>
    /// Update minecraft time of day.
    /// </summary>
    public class TimeUpdate : PacketFromServer
    {
        public const byte ID = 0x03;

        public override byte PacketID { get { return ID; } }

        public long Time { get; set; }

        public long TimeOfDay { get; set; }

        public override string ToString()
        {
            return string.Format("[TimeUpdate: {0}]", Time);
        }

        /// <param name='time'>
        /// Game ticks, 20 ticks/s, 24000 ticks/minecraft day+night
        /// </param>
        public TimeUpdate(long time)
        {
            this.Time = time;
            this.TimeOfDay = time % 24000;
        }

        public TimeUpdate(TimeSpan time)
        {
            this.Time = time.Ticks / (TimeSpan.TicksPerSecond / 20);
            this.TimeOfDay = this.Time % 24000;
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Time = r.ReadInt64();
            TimeOfDay = r.ReadInt64();
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((long)Time);
            w.Write((long)TimeOfDay);
        }
    }
}

