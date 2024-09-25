using System.Collections.Generic;
using GraphProcessor;
using NRTools.CustomAnimator;
using NRTools.GpuSkinning;
using UnityEngine.UIElements;

namespace NRTools.Animator.NRNodes
{
    [NodeCustomEditor(typeof(AnimatorNode))]
    public class AnimatorNodeView : BaseNodeView
    {
        private AnimationData _animationData;
        private Toggle _loopToggle;


        public override void Enable()
        {
            if (AnimationController.IsLoaded && _loopToggle == null) InitializeData();
            else AnimationController.OnLoaded += InitializeData;
            AnimationController.OnAnimatorChanged += UpdateAnimator;
        }

        private void InitializeData()
        {
            Initialize();
        }

        private void Initialize()
        {
            var animatorNode = nodeTarget as AnimatorNode;

            if (animatorNode.animator == AnimationController.currentAnimator) style.display = DisplayStyle.Flex;
            else style.display = DisplayStyle.None;

            if (_loopToggle == null)
            {
                _animationData = AnimationManager.GetAnimationData(AnimationController.currentAnimator, animatorNode.animationName);
                if (_animationData == null) return;
                
                _loopToggle = new Toggle("Looping")
                {
                    value = AnimationManager.GetAnimationData(AnimationController.currentAnimator, _animationData.animationName).loop
                };

                _loopToggle.RegisterValueChangedCallback(evt =>
                    AnimationManager.EDITOR_SetLooping(AnimationController.currentAnimator, animatorNode.animationName,
                        evt.newValue));
                controlsContainer.Add(_loopToggle);
            }
            
            animatorNode.data = _animationData;
        }

        private void UpdateAnimator(List<string> obj)
        {
            var animatorNode = nodeTarget as AnimatorNode;

            if (animatorNode.animator == AnimationController.currentAnimator) style.display = DisplayStyle.Flex;
            else style.display = DisplayStyle.None;
            if (_loopToggle == null) Initialize();
        }

        public override void Select(VisualElement selectionContainer, bool additive)
        {
            base.Select(selectionContainer, additive);
            var animatorNode = nodeTarget as AnimatorNode;
            AnimationController.RaiseAnimationChanged(animatorNode);
        }
    }
}