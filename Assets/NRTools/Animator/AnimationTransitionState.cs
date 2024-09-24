using NRTools.GpuSkinning;
using UnityEngine;

namespace NRTools.CustomAnimator
{
    public abstract class AnimationTransitionState
    {
        protected AnimationData _currentAnimation;
        protected static readonly int _SInterpolationFactor = Shader.PropertyToID("_InterpolationFactor");
        private static readonly int _SFrameOffset = Shader.PropertyToID("_FrameOffset");
        protected static readonly int _SBlendFactor = Shader.PropertyToID("_BlendFactor");

        protected AnimationTransitionController controller;
        protected internal float currentFrame;
        protected float blendProgress;
        protected int numFrames;

        public virtual TransitionState state => TransitionState.Transitionless;

        protected AnimationTransitionState(AnimationTransitionController transitionController)
        {
            controller = transitionController;
        }

        public virtual void OnEnter(AnimationData animation)
        {
            _currentAnimation = animation;
            numFrames = animation.frameCount;
            currentFrame = 0;
        }

        public virtual void UpdateState(Renderer renderer, MaterialPropertyBlock propertyBlock)
        {
            currentFrame += Time.deltaTime * 24f;
            SetFrameOffset(propertyBlock);
        }

        public virtual void UpdateState(Renderer renderer, MaterialPropertyBlock propertyBlock, float seconds)
        {
            currentFrame += seconds * 24f;
            SetFrameOffset(propertyBlock);
        }

        protected void SetFrameOffset(MaterialPropertyBlock propertyBlock)
        {
            var frame0 = Mathf.FloorToInt(currentFrame);
            var t = currentFrame - frame0;


            propertyBlock.SetInt(_SFrameOffset, _currentAnimation.vertexOffset + _currentAnimation.vertexCount * frame0);
            propertyBlock.SetFloat(_SInterpolationFactor, t);
        }

        protected void OverwriteFrameOffset(MaterialPropertyBlock propertyBlock, int frame, float t)
        {
            propertyBlock.SetInt(_SFrameOffset,
                _currentAnimation.vertexOffset + _currentAnimation.vertexCount * frame);
            propertyBlock.SetFloat(_SInterpolationFactor, t);
        }

        public virtual void OnExit()
        {
            currentFrame = 0;
        }
    }
}