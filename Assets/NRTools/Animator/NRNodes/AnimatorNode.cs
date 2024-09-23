using GraphProcessor;
using UnityEngine;

namespace NRTools.Animator.NRNodes
{
    [System.Serializable, NodeMenuItem("Animations/Animation")]
    public class AnimatorNode : BaseNode
    {
        [Output(name = "Transition", allowMultiple = true), SerializeField]
        public string animationName;
        

        [Input(name = "Input", allowMultiple = true)] 
        public string inputAnimation;

        [SerializeField]
        public string animator;

        public override string name => animationName;
    }
}