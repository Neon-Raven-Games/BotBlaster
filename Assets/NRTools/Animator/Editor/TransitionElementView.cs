using GraphProcessor;
using NRTools.Animator.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NRTools.CustomAnimator
{
    public class TransitionElementView : PinnedElementView
    {
        public static TransitionElementView Instance { get; private set; }
        private BaseGraphView graphView;

        private Toggle boolField;
        private Toggle loopingField;
        private Toggle holdingField;
        private FloatField floatField;
        private Label animationTransitionField;
        
        public static AnimationTransition transition;

        public TransitionElementView()
        {
            Instance = this;
            title = "Transition Properties";
        }

        protected override void Initialize(BaseGraphView graphView)
        {
            Instance = this;
            this.graphView = graphView;
            var scrollView = new ScrollView();

            boolField = new Toggle("Blending")
            {
                value = transition?.shouldBlend ?? false
            };
            
            boolField.RegisterValueChangedCallback(evt => 
                transition.shouldBlend = evt.newValue);
            scrollView.Add(boolField);

            loopingField = new Toggle("Looping")
            {
                value = transition?.looping ?? false
            };
            loopingField.RegisterValueChangedCallback(evt => 
                transition.looping = evt.newValue); 
            scrollView.Add(loopingField);

            holdingField = new Toggle("Holding")
            {
                value = transition?.holding ?? false // Set initial value
            };
            holdingField.RegisterValueChangedCallback(evt => 
                transition.holding = evt.newValue); 
            scrollView.Add(holdingField);

            floatField = new FloatField("Blend Duration")
            {
                value = transition?.blendDuration ?? 0.5f // Set initial value
            };
            floatField.RegisterValueChangedCallback(evt => 
                transition.blendDuration = evt.newValue);
            scrollView.Add(floatField);

            animationTransitionField = new Label($"{transition?.fromAnimation} -> {transition?.toAnimation}");
            scrollView.Add(animationTransitionField);

            var previewButton = new Button(() => Debug.Log("Previewing transition")) { text = "Preview" };
            scrollView.Add(previewButton);

            content.Add(scrollView);
            SetPosition(new Rect(24, 46, 240, 200));
        }

        public void SetTransitionData(AnimationTransition newTransition)
        {
            transition = newTransition;
            boolField.value = transition.shouldBlend;
            loopingField.value = transition.looping;
            holdingField.value = transition.holding;
            floatField.value = transition.blendDuration;
            animationTransitionField.text = $"{transition.fromAnimation} -> {transition.toAnimation}";
            content.MarkDirtyRepaint();
        }
    }
}
