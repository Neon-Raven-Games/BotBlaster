using NRTools.GpuSkinning;
using UnityEngine;

namespace NRTools.CustomAnimator
{
    public class LoopState : AnimationTransitionState
    {
        public override TransitionState state => TransitionState.Loop;

        private float stopInterpolationThreshold = 2f;

        public LoopState(AnimationTransitionController transitionController) : base(transitionController)
        {
        }

        public override void UpdateState(Renderer renderer, MaterialPropertyBlock propertyBlock)
        {
            base.UpdateState(renderer, propertyBlock);
            if (currentFrame < numFrames - stopInterpolationThreshold) return;

            currentFrame %= numFrames;
            var frame0 = Mathf.FloorToInt(currentFrame);
            OverwriteFrameOffset(propertyBlock, frame0, 0);
        }

        public override void UpdateState(Renderer renderer, MaterialPropertyBlock propertyBlock, float seconds)
        {
            currentFrame += seconds * 24f;
            var frame0 = Mathf.FloorToInt(currentFrame);
            var t = currentFrame - frame0;
            OverwriteFrameOffset(propertyBlock, frame0, t);
            if (currentFrame >= numFrames - 1)
            {
                currentFrame = 0;
                controller.AnimationEnd();
            }
        }
    }
}