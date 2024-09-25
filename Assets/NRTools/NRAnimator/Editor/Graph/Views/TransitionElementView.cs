using GraphProcessor;
using NRTools.Animator.GraphView;
using NRTools.GpuSkinning;
using UnityEngine;
using UnityEngine.UIElements;

namespace NRTools.CustomAnimator
{
    public class TransitionElementView : PinnedElementView
    {
        private BaseGraphView graphView;
        private FloatField _blendStart;
        private Toggle _looping;
        private Toggle _shouldBlend;
        private FloatField _blendDuration;
        private FloatField _blendWeight;
        private Label _animationTransitionField;

        public AnimationTransitionData transition = new();

        public TransitionElementView()
        {
            title = "Transition Properties";
        }

        public void InitiateTransition()
        {
            // AnimationController.RaiseTransition();
        }


        protected override void Initialize(BaseGraphView graphView)
        {
            this.graphView = graphView;
            var scrollView = new ScrollView();

            _shouldBlend = new Toggle("Blending")
            {
                value = transition?.shouldBlend ?? false
            };

            _shouldBlend.RegisterValueChangedCallback(evt =>
                transition.shouldBlend = evt.newValue);
            scrollView.Add(_shouldBlend);


            _blendDuration = new FloatField("Blend Duration")
            {
                value = transition?.blendDuration ?? 0.5f
            };
            _blendDuration.RegisterValueChangedCallback(evt =>
            {
                transition.blendDuration = evt.newValue;
                if (_transitionData != null) UpdateUIAndRaiseSelect(_transitionData);
            });
            scrollView.Add(_blendDuration);

            _animationTransitionField = new Label($"{transition?.fromAnimation} -> {transition?.toAnimation}");
            scrollView.Add(_animationTransitionField);

            // var previewButton = new Button(InitiateTransition) {text = "Preview Transition"};
            // scrollView.Add(previewButton);

            content.Add(scrollView);
            SetPosition(new Rect(24, 46, 240, 200));
            
            AnimationController.OnTransitionSelected += UpdateUIAndRaiseSelect;
        }

        private AnimationTransitionData _transitionData;
        private void UpdateUIAndRaiseSelect(AnimationTransitionData obj)
        {
            _transitionData = obj;
            _shouldBlend.value = obj.shouldBlend;
            _blendDuration.value = obj.blendDuration;
            _animationTransitionField.text = $"{obj.fromAnimation} -> {obj.toAnimation}";
            content.MarkDirtyRepaint();
        }
    }
}