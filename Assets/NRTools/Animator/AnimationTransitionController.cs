using System.Collections.Generic;
using NRTools.GpuSkinning;
using UnityEngine;

namespace NRTools.CustomAnimator
{
    public enum TransitionState
    {
        Loop,
        Blending,
        Fallback,
        Transitionless,
        Ended
    }

    public class AnimationTransitionController : MonoBehaviour
    {
        public Renderer renderer;

        private MaterialPropertyBlock _propertyBlock;

        private AnimationTransitionState _currentState;
        private AnimationData _currentAnimationData;

        private Dictionary<TransitionState, AnimationTransitionState> _transitionStates;

        internal static readonly int _SNextAnimationOffset = Shader.PropertyToID("_NextAnimationOffset");

        public void Start()
        {
            _propertyBlock = new MaterialPropertyBlock();
            renderer = GetComponent<Renderer>();

            _transitionStates = new Dictionary<TransitionState, AnimationTransitionState>
            {
                {TransitionState.Loop, new LoopState(this)},
                {TransitionState.Blending, new BlendingTransition(this)},
                {TransitionState.Fallback, new FallbackState(this)},
                {TransitionState.Transitionless, new TransitionlessState(this)},
                {TransitionState.Ended, new EndAnimationState(this)}
            };

            _currentState = _transitionStates[TransitionState.Transitionless];
        }

        public void PlayAnimation(AnimationData animationData, TransitionState initialState = TransitionState.Loop)
        {
            _currentAnimationData = animationData;
            ChangeState(initialState);
        }

        public void ChangeState(TransitionState state)
        {
            _currentState.OnExit();
            _transitionStates[state].currentFrame = 0;
            _currentState = _transitionStates[state];
            _currentState.OnEnter(_currentAnimationData);
        }

        private void Update()
        {
            renderer.GetPropertyBlock(_propertyBlock);
            _currentState?.UpdateState(renderer, _propertyBlock);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        public void EditorUpdate(float deltaSeconds)
        {
            renderer.GetPropertyBlock(_propertyBlock);
            _currentState?.UpdateState(renderer, _propertyBlock, deltaSeconds);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        public void SetNextAnimation(AnimationData currentAnimation, AnimationData nextAnimation, AnimationTransitionData data)
        {
            if (_currentState.state == TransitionState.Blending) return;

            var blendingState = (BlendingTransition) _transitionStates[TransitionState.Blending];
            
            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt(_SNextAnimationOffset, nextAnimation.vertexOffset);
            renderer.SetPropertyBlock(_propertyBlock);
            _currentAnimationData = currentAnimation;
            blendingState.SetNextAnimation(nextAnimation, data);
            ChangeState(TransitionState.Blending);
        }
    }
}