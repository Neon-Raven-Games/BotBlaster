using System;

namespace NRTools.Animator.GraphView
{
    [Serializable]
    public class AnimationTransition
    {
        public string fromAnimation;
        public string toAnimation;
        public float blendDuration;
        public bool shouldBlend;
        public bool looping;
        public bool holding;
        public string fallbackAnimation;

        public AnimationTransition()
        {
        }

        public AnimationTransition(string fromAnim, string toAnim, float duration, bool blend, bool loop, bool hold,
            string fallback)
        {
            fromAnimation = fromAnim;
            toAnimation = toAnim;
            blendDuration = duration;
            shouldBlend = blend;
            looping = loop;
            holding = hold;
            fallbackAnimation = fallback;
        }
    }
}