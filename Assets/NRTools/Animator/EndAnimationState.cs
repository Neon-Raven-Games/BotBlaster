using NRTools.GpuSkinning;
using UnityEngine;

namespace NRTools.CustomAnimator
{
    public class EndAnimationState : AnimationTransitionState
    {
        public override TransitionState state => TransitionState.Ended;

        public EndAnimationState(AnimationTransitionController transitionController) : base(transitionController)
        {
        }

        public override void OnEnter(AnimationData animation)
        {
        }

        public override void OnExit()
        {
        }

        public override void UpdateState(Renderer renderer, MaterialPropertyBlock propertyBlock, float seconds)
        {
        }

        public override void UpdateState(Renderer renderer, MaterialPropertyBlock propertyBlock)
        {
        }
    }
}