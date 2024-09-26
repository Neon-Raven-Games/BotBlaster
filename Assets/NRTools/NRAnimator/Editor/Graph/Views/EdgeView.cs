using NRTools.CustomAnimator;
using NRTools.GpuSkinning;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using AnimatorNode = NRTools.Animator.NRNodes.AnimatorNode;

namespace GraphProcessor
{
    public class EdgeView : Edge
    {
        public bool isConnected = false;

        public AnimationTransitionData transition;

        public SerializableEdge serializedEdge
        {
            get { return userData as SerializableEdge; }
        }

        readonly string edgeStyle = "GraphProcessorStyles/EdgeView";

        protected BaseGraphView owner => ((input ?? output) as PortView).owner.owner;

        public EdgeView() : base()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(edgeStyle));
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        public override void OnPortChanged(bool isInput)
        {
            base.OnPortChanged(isInput);
            UpdateEdgeSize();
            if (input != null && output != null)
            {
                var animNode = (output as PortView).owner.nodeTarget as AnimatorNode;
                var fromAnim = animNode?.animationName;
                var outNode = (input as PortView).owner.nodeTarget as AnimatorNode;
                var toAnim = outNode?.animationName;

                if (transition == null)
                {
                    transition = new AnimationTransitionData()
                    {
                        blendDuration = 0.5f,
                        shouldBlend = true,
                        toAnimation = toAnim,
                    };
                }

                if (animNode != null)
                {
                    animNode.AddTransition(toAnim, transition);
                }

                AnimationController.RaiseTransitionSelected(transition, fromAnim);
            }
        }

        public void UpdateEdgeSize()
        {
            if (input == null && output == null)
                return;

            PortData inputPortData = (input as PortView)?.portData;
            PortData outputPortData = (output as PortView)?.portData;

            for (int i = 1; i < 20; i++) RemoveFromClassList($"edge_{i}");
            int maxPortSize = Mathf.Max(inputPortData?.sizeInPixel ?? 0, outputPortData?.sizeInPixel ?? 0);
            if (maxPortSize > 0) AddToClassList($"edge_{Mathf.Max(1, maxPortSize - 6)}");
        }

        protected override void OnCustomStyleResolved(ICustomStyle styles)
        {
            base.OnCustomStyleResolved(styles);
            UpdateEdgeControl();
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            if (e.clickCount == 1)
            {
                if (input != null && output != null)
                {
                    var animNode = (output as PortView).owner.nodeTarget as AnimatorNode;
                    var fromAnim = animNode?.animationName;
                    var outNode = (input as PortView).owner.nodeTarget as AnimatorNode;
                    var toAnim = outNode?.animationName;

                    if (transition == null)
                    {
                        transition = new AnimationTransitionData()
                        {
                            blendDuration = 0.5f,
                            shouldBlend = true,
                            toAnimation = toAnim,
                        };
                    }

                    if (animNode != null)
                    {
                        animNode.AddTransition(toAnim, transition);
                    }

                    AnimationController.RaiseTransitionSelected(transition, fromAnim);
                }
            }

            if (e.clickCount == 2)
            {
                // Empirical offset:
                var position = e.mousePosition;
                position += new Vector2(-10f, -28);
                Vector2 mousePos = owner.ChangeCoordinatesTo(owner.contentViewContainer, position);

                owner.AddRelayNode(input as PortView, output as PortView, mousePos);
            }
        }
    }
}