using System.Collections.Generic;
using GraphProcessor;
using NRTools.GpuSkinning;
using UnityEngine;
using UnityEngine.UIElements;

namespace NRTools.CustomAnimator
{
    public class AnimatorsElementView : PinnedElementView
    {
        BaseGraphView graphView;
        private bool subbed;
        public AnimatorsElementView() => title = "Animators";
        private ScrollView _scrollView;

        private void LoadAnimator(string animator)
        {
            AnimationController.RaiseAnimations(animator);
            
            if (_scrollView == null)
            {
                var searchField = new TextField();
                content.Add(searchField);
                _scrollView = new ScrollView();

                searchField.RegisterValueChangedCallback(evt => Search(evt.newValue));
                content.Add(_scrollView);
            }

            var anims = AnimationManager.GetAnimators();
            _scrollView.Clear();
            foreach (var dude in anims)
            {
                var button = new Button(() => { LoadAnimator(dude); }) {text = dude};
                _scrollView.Add(button);
            }
        }

        private void LoadInitialAnimators()
        {
            LoadAnimator(AnimationController.currentAnimator);
            MarkDirtyRepaint();
        }

        protected override void Destroy()
        {
            base.Destroy();
            AnimationController.OnLoaded -= LoadInitialAnimators;
            subbed = false;
        }
        
        protected override void Initialize(BaseGraphView graphView)
        {
            if (AnimationController.IsLoaded) LoadAnimator(AnimationController.currentAnimator);
            else if (!subbed)
            {
                subbed = true;
                AnimationController.OnLoaded += LoadInitialAnimators;
            }

            this.graphView = graphView;

            SetPosition(new Rect(24, 46, 140, 200));
            MarkDirtyRepaint();
        }

        private void Search(string search)
        {
            var lowerSearch = search.ToLower();
            foreach (var child in _scrollView.Children())
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