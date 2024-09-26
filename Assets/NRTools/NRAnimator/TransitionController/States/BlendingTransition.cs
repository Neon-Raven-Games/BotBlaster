using System.Linq.Expressions;
using NRTools.GpuSkinning;
using UnityEngine;

namespace NRTools.CustomAnimator
{
    public class BlendingTransition : AnimationTransitionState
    {
        private AnimationData _nextAnimation;
        private float _blendDuration;
        private bool _looping;
        private bool active;

        public override TransitionState state => TransitionState.Blending;

        private float _blendTime;
        public BlendingTransition(AnimationTransitionController transitionController) : base(transitionController)
        {
        }

        public override void OnEnter(AnimationData animation)
        {
            base.OnEnter(animation);
            _blendTime = 0;
            blendProgress = 0f;
            active = true;
        }

        public override void OnExit()
        {
            base.OnExit();
            _blendTime = 0;
        }

        public void SetNextAnimation(AnimationData nextAnimation, AnimationTransitionData data)
        {
            _nextAnimation = nextAnimation;
            _blendDuration = data.blendDuration;
            _looping = _nextAnimation.loop;
        }

        public override void UpdateState(Renderer renderer, MaterialPropertyBlock propertyBlock)
        {
            base.UpdateState(renderer, propertyBlock);

            blendProgress += Time.deltaTime / _blendDuration;
            blendProgress = Mathf.Clamp01(blendProgress);

            if (blendProgress >= 0.9)
            {
                propertyBlock.SetFloat(_SBlendFactor, 0f);
                controller.PlayAnimation(_nextAnimation,
                    _looping ? TransitionState.Loop : TransitionState.Transitionless);
            }
            else
            {
                propertyBlock.SetInt(AnimationTransitionController._SNextAnimationOffset,
                    _nextAnimation.vertexOffset + _nextAnimation.vertexCount);
                propertyBlock.SetFloat(_SBlendFactor, blendProgress);
            }
        }

        public override void UpdateState(Renderer renderer, MaterialPropertyBlock propertyBlock, float seconds)
        {
            if (!active) return;
            _blendTime += seconds * 24;           
            
            var timeBasedBlend = _blendTime / _blendDuration;
            
            var framesRemaining = numFrames - currentFrame;
            var framesNeeded = _blendDuration * 24;
            var blend = framesRemaining < framesNeeded ? 
                1.0f - (framesRemaining / framesNeeded) : timeBasedBlend;

            blendProgress = Mathf.Clamp01(blend);

            if (blendProgress >= 0.9 || currentFrame >= numFrames - 2)
            {
                currentFrame = 0;
                controller.AnimationEnd();
                
                propertyBlock.SetFloat(_SBlendFactor, 0f);
                renderer.SetPropertyBlock(propertyBlock);
                active = false;
                Debug.Log("Calling end in blend");
                return;
            }
            
            propertyBlock.SetFloat(_SBlendFactor, blendProgress);
            
            currentFrame += seconds * 24f;
            var frame0 = Mathf.FloorToInt(currentFrame);
            var t = currentFrame - frame0;
            OverwriteFrameOffset(propertyBlock, frame0, t);
        }
    }
}