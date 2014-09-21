using System;

namespace MineProxy
{
    public static class LawsOfMinecraft
    {
        //Mode      Crouch  Walk    Run     SprJump
        //Normal:   1.30    4.32    5.61    7.62
        //+0        1.55    5.18    6.73    7.56
        //+0-0		1.32    4.40    5.72    7.22
        //-0        1.10    3.67    4.77    7.05
        //+1-0				5.14    6.68
        //+1        1.81    6.04    7.86    7.86

#if DEBUG
        const double margin = 0.1; //Trigger ban
#else
        const double margin = 2.1;
#endif

        /// <summary>
        /// Real speed is (Effect - ZeroSpeedEffect)*Crounch
        /// </summary>
        public const double ZeroSpeedEffect = -5;
        public const double SpeedEffectMultiplier = 0.3;

        public const double Crounch = 1 * 1.4; //squared 1, if walking sideways
        public const int Walk = 3;
        public const int Sprint = 4; //2 == sprintjumping

        /// <summary>
        /// This is regardless of potion/crounch state
        /// </summary>
        public const double LadderSpeed = 2.47;

        //These two are not used in speed guard since they are server side
        public const double LimitBoat = 6.5; //Boat:	6.2
        public const double LimitCart = 8.5;
		
    }
}

