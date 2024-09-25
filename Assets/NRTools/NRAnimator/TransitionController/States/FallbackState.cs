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
            currentFrame += seconds * 24f;
            var frame0 = Mathf.FloorToInt(currentFrame);
            var t = currentFrame - frame0;
            OverwriteFrameOffset(propertyBlock, frame0, t);
            if (currentFrame >= numFrames - 1) controller.AnimationEnd();
        }
    }
}