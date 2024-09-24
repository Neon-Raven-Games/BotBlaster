using NRTools.GpuSkinning;
using UnityEngine;

namespace NRTools.CustomAnimator
{
    public class TransitionlessState : AnimationTransitionState
    {
        private bool _initialized;
        private float startTime;
        private float currentTime;
        public override TransitionState state => TransitionState.Transitionless;

        public TransitionlessState(AnimationTransitionController transitionController) : base(transitionController)
        {
        }

        public override void OnEnter(AnimationData animation)
        {
            base.OnEnter(animation);
            _initialized = true;
        }

        public override void OnExit()
        {
            _initialized = false;
            base.OnExit();
        }

        public override void UpdateState(Renderer renderer, MaterialPropertyBlock propertyBlock)
        {
            base.UpdateState(renderer, propertyBlock);
            if (currentFrame >= numFrames - 1)
            {
                controller.ChangeState(TransitionState.Ended);
            }
        }

        public override void UpdateState(Renderer renderer, MaterialPropertyBlock propertyBlock, float seconds)
        {
            currentFrame += seconds * 24f;
            var frame0 = Mathf.FloorToInt(currentFrame);
            var t = currentFrame - frame0;
            OverwriteFrameOffset(propertyBlock, frame0, t);
            
            if (currentFrame >= numFrames - 1) controller.ChangeState(TransitionState.Ended);
        }
    }
}