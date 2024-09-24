using System;
using NRTools.Animator.GraphView;
using NRTools.Animator.NRNodes;
using NRTools.CustomAnimator;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.Serialization;


namespace GraphProcessor
{
    public class EdgeView : Edge
    {
        public bool isConnected = false;

        public AnimationTransition transition;

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
                var fromAnim = "";
                if ((output as PortView).owner.nodeTarget is AnimatorNode animNode)
                {
                    fromAnim = animNode.animationName;
                }

                var toAnim = "";

                if ((input as PortView).owner.nodeTarget is AnimatorNode outNode)
                {
                    toAnim = outNode.animationName;
                }


                if (transition == null)
                {
                    transition = new AnimationTransition(
                        fromAnim,
                        toAnim,
                        duration: 0.5f,
                        blend: true,
                        loop: false
                    );
                }
                transition.fromAnimation = fromAnim;
                transition.toAnimation = toAnim;
                if (TransitionElementView.Instance != null) 
                    TransitionElementView.Instance.SetTransitionData(transition);
                
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
                    var fromAnim = "";
                    if ((output as PortView).owner.nodeTarget is AnimatorNode animNode)
                    {
                        fromAnim = animNode.animationName;
                    }

                    var toAnim = "";

                    if ((input as PortView).owner.nodeTarget is AnimatorNode outNode)
                    {
                        toAnim = outNode.animationName;
                    }


                    if (transition == null)
                    {
                        transition = new AnimationTransition(
                            fromAnim,
                            toAnim,
                            duration: 0.5f,
                            blend: true,
                            loop: false
                        );
                    }
                    transition.fromAnimation = fromAnim;
                    transition.toAnimation = toAnim;
                    if (TransitionElementView.Instance != null)
                        TransitionElementView.Instance.SetTransitionData(transition);
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