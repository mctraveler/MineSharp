using System;

namespace MineProxy.Packets
{
    public enum GameState
    {
        InvalidBed = 0,
        EndRaining = 1,
        BeginRaining = 2,
        ChangeGameMode = 3,
        /// <summary>
        /// Enter credits
        /// </summary>
        EndText = 4,
        /// <summary>
        /// Demo messages
        /// </summary>
        EndText2 = 5,
        BowHitSound = 6,
        FadeValue = 7,
        FadeTime = 8,
    }
}

