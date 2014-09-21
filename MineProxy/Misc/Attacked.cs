using System;

namespace MineProxy
{
    public class Attacked
    {
        public Client By { get; private set; }

        public DateTime Timestamp { get; private set; }

        public BlockID Item { get; set; }

        public Attacked(Client attacker)
        {
            By = attacker;
            Timestamp = DateTime.Now;
            if (attacker.Session.ActiveItem == null)
                Item = BlockID.BareHands;
            else
                Item = attacker.Session.ActiveItem.ItemID;
        }
    }
}

