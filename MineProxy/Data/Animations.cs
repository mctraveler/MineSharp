using System;

namespace MineProxy.Data
{
    public enum Animations
    {
        SwingArm = 0,
        /// <summary>
        /// Perhaps not useful anymore, EntityStatus(eid, StatusTypes.EntityHurt) does the same and more, moves arms
        /// </summary>
        DamageAnimation = 1,
        LeaveBed = 2,
        Eating = 3,
        CriticalEffect = 4,
        MagicalEffect = 5,
        
        Unknown = 102,
        Crouch = 104,
        Uncrouch = 105,
    }
}

