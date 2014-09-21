using System;

namespace MineProxy.Worlds
{
    /// <summary>
    /// Used in speed guard.
    /// This is a post for a single time in history to record the current effect in action
    /// </summary>
    public class EffectPost
    {
        public readonly PlayerEffects Effect;
		
        public bool Active { get; set; }
		
        public int Amplifier { get; set; }
		
        public EffectPost(PlayerEffects eff)
        {
            this.Effect = eff;
        }
    }
}

