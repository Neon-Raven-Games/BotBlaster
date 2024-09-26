using System.Collections.Generic;
using GraphProcessor;
using NRTools.GpuSkinning;
using Unity.VisualScripting;
using UnityEngine;

namespace NRTools.Animator.NRNodes
{
    [System.Serializable, NodeMenuItem("Animations/Animation")]
    public class AnimatorNode : BaseNode
    {
        [Output(name = "Transition", allowMultiple = true), SerializeField]
        public string animationName;
        public override string name => animationName;

        [Input(name = "Input", allowMultiple = true)]
        public string inputAnimation;

        [SerializeField] public string animator;

        public AnimationData data;
        public Dictionary<string, AnimationTransitionData> transitionsTo = new();
        public bool isActive;

        public void AddTransition(string fromAnim, AnimationTransitionData transitionData)
        {
            if (transitionsTo.ContainsKey(fromAnim))
            {
                transitionsTo[fromAnim] = transitionData;
            }
            else
            {
                transitionsTo.Add(fromAnim, transitionData);
            }
        }

    }
}