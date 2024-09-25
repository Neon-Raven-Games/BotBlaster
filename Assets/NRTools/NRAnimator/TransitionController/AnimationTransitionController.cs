using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphProcessor;
using NRTools.Animator.NRNodes;
using NRTools.GpuSkinning;
using UnityEngine;
using AnimatorNode = NRTools.Animator.NRNodes.AnimatorNode;

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
            if (_transitionData != null)
            {
                // Trigger the next animation via graph traversal
                string currentAnim = _currentAnimationData.animationName; // Assuming animation data has a name
                TransitionToNext(AnimationController.currentAnimator, currentAnim, graph);
            }
            else
            {
                ChangeState(TransitionState.Ended);
            }
        }


        // todo, we need an animator property for this controller
        internal void Transition(AnimationTransitionData data, AnimationData animData)
        {
            _transitionData = animData;
            var blendingState = (BlendingTransition) _transitionStates[TransitionState.Blending];
            var nextAnimation =
                AnimationManager.GetAnimationData(AnimationController.currentAnimator, data.toAnimation);

            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt(_SNextAnimationOffset, nextAnimation.vertexOffset);
            renderer.SetPropertyBlock(_propertyBlock);

            if (data.blendDuration > 0) blendingState.SetNextAnimation(nextAnimation, data);
            ChangeState(data.blendDuration > 0 ? TransitionState.Blending : TransitionState.Fallback);
        }


        public void StartAtNode(AnimatorNode node)
        {
            var animNode = graph.Traverse(AnimationController.currentAnimator, node.animationName, node.transitionsTo.Keys.First());
            PlayAnimation(animNode.data);
        }

        public void TransitionToNext(string currentNode, string currentAnim, RuntimeAnimationGraph graph)
        {
            var nextNode = graph.Traverse(AnimationController.currentAnimator, currentNode, currentAnim);

            if (nextNode != null)
            {
                var nextAnimationData =
                    AnimationManager.GetAnimationData(AnimationController.currentAnimator, nextNode.animationName);
                Transition(nextNode.transitionsTo[currentAnim], nextAnimationData);
            }
            else
            {
                Debug.LogWarning("No valid transition found.");
            }
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

        public void SetNextAnimation(AnimationData currentAnimation, AnimationData nextAnimation,
            AnimationTransitionData data)
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