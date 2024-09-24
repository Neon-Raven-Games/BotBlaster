using UnityEngine;

namespace NRTools.CustomAnimator
{
    public class FallbackState : AnimationTransitionState
    {
        public override TransitionState state => TransitionState.Fallback;

        public FallbackState(AnimationTransitionController transitionController) : base(transitionController)
        {
        }

        public override void UpdateState(Renderer renderer, MaterialPropertyBlock propertyBlock)
        {
            base.UpdateState(renderer, propertyBlock);
            if (currentFrame >= numFrames - 1) controller.ChangeState(TransitionState.Loop);
        }

        public override void UpdateState(Renderer renderer, MaterialPropertyBlock propertyBlock, float seconds)
        {
            base.UpdateState(renderer, propertyBlock, seconds);
            if (currentFrame >= numFrames - 1) controller.ChangeState(TransitionState.Loop);
        }
    }
}