using System;

namespace MineProxy
{
    public enum Vehicles
    {
        Unknown = 0,
        Boats = 1,
        Item = 2, //Replace old SpawnItem packet

        Minecart = 10,
        StorageCart = 11,
        PoweredCart = 12,
        ActivatedTNT = 50,
        Arrow = 60,
        ThrownSnowball = 61,
        ThrownEgg = 62,
        /// <summary>
        /// Fell on glass, probably not that
        /// </summary>
        Unknown63 = 63,
        BlazeFire = 64,
        Unknown66 = 66,
        //ExperienceOrb = 63, //No longer uses, own package now
        FallingBlock = 70,
        Frame = 71,
        SplashPotion = 73,

        FireworkRocket = 76,
        UnknownLeash = 77,

        FishingFloat = 90,
    }
}

