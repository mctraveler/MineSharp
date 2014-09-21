using System;

namespace MineProxy.Worlds
{
    public class HellSession : ConstructSession
    {
        public override World World { get { return World.HellBanned; } }

        public override string ShortWorldName
        {
            get
            {
                BadPlayer b = Banned.CheckBanned(Player);
                if (b == null)
                    return "Hell(Visitor)";
                else
                    return "Hell(Banned)";
            }
        }

        protected override void SetPosition(CoordDouble pos, bool speedguard)
        {
            base.SetPosition(pos, false);
        }

        public HellSession(Client player) : base (player)
        {
        }
    }
}

