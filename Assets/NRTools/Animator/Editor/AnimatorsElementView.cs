using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NRTools.CustomAnimator
{
    public class AnimatorsElementView : PinnedElementView
    {
        BaseGraphView graphView;
        public AnimatorsElementView() => title = "Animators";
        public List<string> animators = new();
        private ScrollView scrollView;
        protected override void Initialize(BaseGraphView graphView)
        {
            animators.Add("Tank");
            animators.Add("Grunt");
            animators.Add("GlassCannon");
            animators.Add("Swarm");
            
            var searchField = new TextField();
            content.Add(searchField);
            
            this.graphView = graphView;
            scrollView = new ScrollView();
            
            foreach (var animator in animators)
            {
                var button = new Button(() =>
                {
                    AnimationController.RaiseAnimations(animator);
                }) { text = animator };
                scrollView.Add(button);
            }
            searchField.RegisterValueChangedCallback(evt => Search(evt.newValue));
            content.Add(scrollView);
            SetPosition(new Rect(24, 46, 140, 200));
        }

        private void Search(string search)
        {
            var lowerSearch = search.ToLower();
            foreach (var child in scrollView.Children())
            {
                if (child is Button button)
                {
                    if (button.text.ToLower().Contains(lowerSearch) || string.IsNullOrEmpty(search))
                        button.style.display = DisplayStyle.Flex;
                    else
                        button.style.display = DisplayStyle.None;
                }
            }
        }
    }
}