using System;

namespace MineProxy.Data
{
    public enum RailMeta
    {
        NS = 0,
        WE,
        RisingWE,
        RisingEW,
        RisingSN,
        RisingNS,
        ES,
        SW,
        WN,
        NE,
    }

    /// <summary>
    /// Powered rail and rail trigger
    /// </summary>
    public enum PoweredRailMeta
    {
        NS = 0,
        WE,
        RisingWE,
        RisingEW,
        RisingSN,
        RisingNS,

        PoweredNS = 8,
        PoweredWE,
        PoweredRisingWE,
        PoweredRisingEW,
        PoweredRisingSN,
        PoweredRisingNS,

    }
}

