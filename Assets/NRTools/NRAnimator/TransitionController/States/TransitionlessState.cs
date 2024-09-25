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
            if (currentFrame >= numFrames - 2) controller.ChangeState(TransitionState.Ended);
        }

        public override void UpdateState(Renderer renderer, MaterialPropertyBlock propertyBlock, float seconds)
        {
            base.UpdateState(renderer, propertyBlock, seconds);
            if (currentFrame >= numFrames - 2) controller.ChangeState(TransitionState.Ended);
        }
    }
}