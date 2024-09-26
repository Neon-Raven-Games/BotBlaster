using System.Collections.Generic;
using System.IO;
using GraphProcessor;
using NRTools.GpuSkinning;
using UnityEngine;

// system needs an instanced observer, editor code is via actions for decoupling
// we need to separate the graphs based on the animator, but support editor reload on the fly
//

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
            graph = new RuntimeAnimationGraph();
            var path = Path.Combine(Application.streamingAssetsPath, "path_to_save.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                graph.DeserializeGraph(json);
            }


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

        private AnimationData _transitionData;
        private RuntimeAnimationGraph graph;

        protected internal void AnimationEnd()
        {
            _currentState.OnExit();
            TransitionToNext(graph);
        }

        private void Transition(AnimationTransitionData data, AnimationData animData)
        {
            _transitionData = animData;
            var nextAnimation = AnimationManager.GetAnimationData(
                AnimationController.currentAnimator, data.toAnimation);

            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt(_SNextAnimationOffset, nextAnimation.vertexOffset);
            renderer.SetPropertyBlock(_propertyBlock);

            ChangeState(animData.loop ? TransitionState.Loop : TransitionState.Fallback);
            
            // todo, this needs to happen on interrupt, needs some timing work again
            // if (data.blendDuration > 0) blendingState.SetNextAnimation(nextAnimation, data);
            // else ChangeState(data.blendDuration > 0 ? TransitionState.Blending : TransitionState.Fallback);
        }

        public void TransitionToNext(RuntimeAnimationGraph graph)
        {
            var nextNode = graph.Traverse(out var transitionData);
            if (nextNode != null)
            {
                var currentAnim = _currentAnimationData.animationName;
                _currentAnimationData = AnimationManager.GetAnimationData(AnimationController.currentAnimator,
                    nextNode.toNode.animationName);

                if (nextNode.toNode.transitions.Count == 0)
                {
                    Debug.Log("Animation Sequence finished. what do?" + nextNode.toNode.animationName);
                    ChangeState(_currentAnimationData.loop ? TransitionState.Loop : TransitionState.Fallback);
                    return;
                }

                if (transitionData != null)
                {
                    Transition(transitionData, _currentAnimationData);
                    AnimationController.RaiseTransitionSelected(transitionData,currentAnim);
                    AnimationController.RaiseEditorAnimationChanged(nextNode.toNode.GUID);
                }
                else
                {
                    Debug.LogError("Transition data was null. Loop back to root or somthin");
                }
            }
            else
            {
                ChangeState(TransitionState.Ended);
            }
        }
        
        public void PreviewSequence(string animationName)
        {
            graph.SetNode(animationName);
            _currentAnimationData = AnimationManager.GetAnimationData(AnimationController.currentAnimator,
                graph.GetCurrentNode().animationName);
            PlayAnimation(_currentAnimationData, TransitionState.Fallback);
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
    }
}