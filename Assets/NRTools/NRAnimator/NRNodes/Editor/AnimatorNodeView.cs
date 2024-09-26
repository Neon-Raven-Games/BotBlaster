using System.Collections.Generic;
using GraphProcessor;
using NRTools.CustomAnimator;
using NRTools.GpuSkinning;
using UnityEngine;
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
            AnimationController.OnAnimationChanged += DrawOutline;
            AnimationController.OnEditorAnimationChanged += UpdateSelection;
        }

        private void UpdateSelection(string guid)
        {
            var animatorNode = nodeTarget as AnimatorNode; 
            if (guid == animatorNode.GUID)
            {
                animatorNode.isActive = true;
                DrawOutline(animatorNode);
            }
            else
            {
                animatorNode.isActive = false;
                DrawOutline(animatorNode);
            }
        }


        public override void Disable()
        {
            AnimationController.OnAnimatorChanged -= UpdateAnimator;
            AnimationController.OnAnimationChanged -= DrawOutline;
            AnimationController.OnEditorAnimationChanged -= UpdateSelection;
        }

        private void InitializeData()
        {
            Initialize();
        }

        private void DrawOutline(AnimatorNode node)
        {
            var animatorNode = nodeTarget as AnimatorNode;
            if (originalColor == Color.magenta) originalColor = animatorNode.color;
            if (animatorNode.isActive)
            {
                SetNodeColor(selectedColor);
            }
            else
            {
                SetNodeColor(originalColor);
            }
        }

        private Color originalColor = Color.magenta;
        private Color selectedColor = Color.green;
        
        private void Initialize()
        {
            var animatorNode = nodeTarget as AnimatorNode;
            

            
            if (animatorNode.animator == AnimationController.currentAnimator) style.display = DisplayStyle.Flex;
            else style.display = DisplayStyle.None;

            if (_loopToggle == null)
            {
                _animationData =
                    AnimationManager.GetAnimationData(AnimationController.currentAnimator, animatorNode.animationName);
                if (_animationData == null) return;

                _loopToggle = new Toggle("Looping")
                {
                    value = AnimationManager
                        .GetAnimationData(AnimationController.currentAnimator, _animationData.animationName).loop
                };

                _loopToggle.RegisterValueChangedCallback(evt =>
                    AnimationManager.EDITOR_SetLooping(AnimationController.currentAnimator, animatorNode.animationName,
                        evt.newValue));
                controlsContainer.Add(_loopToggle);
            }

            if (_guidLabel == null)
            {
                _guidLabel = new Label(animatorNode.GUID);
                controlsContainer.Add(_guidLabel);
            }

            animatorNode.data = _animationData;
        }

        private Label _guidLabel;

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