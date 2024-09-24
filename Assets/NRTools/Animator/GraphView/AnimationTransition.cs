using System;
using UnityEngine.Serialization;

namespace NRTools.Animator.GraphView
{
    [Serializable]
    public class AnimationTransition
    {
        public string fromAnimation;
        public string toAnimation;
        public float blendDuration;
        public float blendWeight;
        public bool shouldBlend;
        public bool looping;
        public float blendStartTime;

        public AnimationTransition()
        {
        }

        public AnimationTransition(string fromAnim, string toAnim, float duration, bool blend, bool loop)
        {
            fromAnimation = fromAnim;
            toAnimation = toAnim;
            blendDuration = duration;
            shouldBlend = blend;
            looping = loop;
        }
    }
}