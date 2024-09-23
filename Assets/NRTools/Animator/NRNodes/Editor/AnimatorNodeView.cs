using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using NRTools.CustomAnimator;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UIElements;

namespace NRTools.Animator.NRNodes
{
    [NodeCustomEditor(typeof(AnimatorNode))]
    public class AnimatorNodeView : BaseNodeView
    {
        
        public override void Enable()
        {
            var animatorNode = nodeTarget as AnimatorNode;
            AnimationController.OnAnimatorChanged += UpdateAnimator;
            if (!string.IsNullOrEmpty(animatorNode.inputAnimation))
            {
                Label textElement = new Label
                {
                    text = animatorNode.inputAnimation + " -> " + animatorNode.animationName,
                };
                controlsContainer.Add(textElement);
            }
        }

        private void UpdateAnimator(List<string> obj)
        {
            var animatorNode = nodeTarget as AnimatorNode;
            
            if (animatorNode.animator == AnimationController.currentAnimator) style.display = DisplayStyle.Flex;
            else style.display = DisplayStyle.None;
        }

        public override void Select(VisualElement selectionContainer, bool additive)
        {
            base.Select(selectionContainer, additive);
            var animatorNode = nodeTarget as AnimatorNode;
            AnimationController.RaiseAnimationChanged(animatorNode.animationName);
        }
    }
}