using GraphProcessor;
using NRTools.Animator.GraphView;
using NRTools.GpuSkinning;
using UnityEngine;
using UnityEngine.UIElements;

namespace NRTools.CustomAnimator
{
    public class TransitionElementView : PinnedElementView
    {
        public static TransitionElementView Instance { get; private set; }
        private BaseGraphView graphView;
        private FloatField _blendStart;
        private Toggle _looping;
        private Toggle _shouldBlend;
        private FloatField _blendDuration;
        private FloatField _blendWeight;
        private Label _animationTransitionField;
        
        public static AnimationTransition transition;

        public TransitionElementView()
        {
            Instance = this;
            title = "Transition Properties";
        }

        public void InitiateTransition()
        {
            var data = new AnimationTransitionData();
            
            if (!transition.shouldBlend) transition.blendWeight = 0;
            else
            {
                data.blendStartTime = transition.blendStartTime;
                data.blendDuration = transition.blendDuration;
                data.blendWeight = transition.blendWeight;
                data.fromAnimation = transition.fromAnimation;
                data.toAnimation = transition.toAnimation;
                data.loop = transition.looping;
            }
            
            AnimationController.RaiseTransition(data, transition.fromAnimation, transition.toAnimation);
        }
        protected override void Initialize(BaseGraphView graphView)
        {
            Instance = this;
            this.graphView = graphView;
            var scrollView = new ScrollView();

            _shouldBlend = new Toggle("Blending")
            {
                value = transition?.shouldBlend ?? false
            };
            
            _shouldBlend.RegisterValueChangedCallback(evt => 
                transition.shouldBlend = evt.newValue);
            scrollView.Add(_shouldBlend);

            _looping = new Toggle("Looping")
            {
                value = transition?.looping ?? false
            };
            _looping.RegisterValueChangedCallback(evt => 
                transition.looping = evt.newValue); 
            scrollView.Add(_looping);

            _blendDuration = new FloatField("Blend Duration")
            {
                value = transition?.blendDuration ?? 0.5f // Set initial value
            };
            _blendDuration.RegisterValueChangedCallback(evt =>
            {
                transition.blendDuration = evt.newValue;
                SetTransitionData(transition);
            });
            scrollView.Add(_blendDuration);
           
            _blendStart = new FloatField("Blend Start")
            {
                value = transition?.blendStartTime ?? 0.5f // Set initial value
            };
            _blendStart.RegisterValueChangedCallback(evt =>
            {
                transition.blendStartTime = evt.newValue;
                SetTransitionData(transition);
            });
            scrollView.Add(_blendStart);
            
            _blendWeight = new FloatField("Blend Weight")
            {
                value = transition?.blendWeight ?? 0.5f // Set initial value
            };
            _blendWeight.RegisterValueChangedCallback(evt => 
                transition.blendWeight = evt.newValue);
            scrollView.Add(_blendWeight); 
            
            
            _animationTransitionField = new Label($"{transition?.fromAnimation} -> {transition?.toAnimation}");
            scrollView.Add(_animationTransitionField);

            var previewButton = new Button(InitiateTransition) { text = "Preview Transition" };
            scrollView.Add(previewButton);

            content.Add(scrollView);
            SetPosition(new Rect(24, 46, 240, 200));
        }

        public void SetTransitionData(AnimationTransition newTransition)
        {
            transition = newTransition;
            _shouldBlend.value = transition.shouldBlend;
            _looping.value = transition.looping;
            _blendDuration.value = transition.blendDuration;
            _blendStart.value = transition.blendStartTime;
            _blendWeight.value = transition.blendWeight;
            _animationTransitionField.text = $"{transition.fromAnimation} -> {transition.toAnimation}";
            
            var data = new AnimationTransitionData();
            
            if (!transition.shouldBlend) transition.blendWeight = 0;
            else
            {
                data.blendStartTime = transition.blendStartTime;
                data.blendDuration = transition.blendDuration;
                data.blendWeight = transition.blendWeight;
                data.fromAnimation = transition.fromAnimation;
                data.toAnimation = transition.toAnimation;
                data.loop = transition.looping;
            }
            
            AnimationController.RaiseTransitionSelected(data);
            
            content.MarkDirtyRepaint();
        }
    }
}
