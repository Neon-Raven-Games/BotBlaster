using System.Collections.Generic;
using GraphProcessor;
using NRTools.Animator.NRNodes;
using NRTools.GpuSkinning;
using UnityEngine;
using UnityEngine.UIElements;

namespace NRTools.CustomAnimator
{
    public class AnimationsElementView : PinnedElementView
    {
        BaseGraphView graphView;
        public AnimationsElementView() => title = "Animations";

        private List<string> _animations = new();
        private ScrollView _scrollView;
        private TextField _searchField;

        protected override void Destroy()
        {
            base.Destroy();
            AnimationController.OnAnimatorChanged -= ChangeAnimations;
            AnimationController.OnLoaded -= LoadInitialAnimations;
        }

        private void ChangeAnimations(List<string> anims)
        {
            _animations = anims;
            if (_scrollView == null)
            {
                _scrollView = new ScrollView();
                _searchField = new TextField();
                _searchField.RegisterValueChangedCallback(evt => Search(evt.newValue));
                
                content.Add(_searchField);
                content.Add(_scrollView);
            }

            _scrollView.Clear();
            PopulateScrollView();
        }

        private void LoadInitialAnimations()
        {
            ChangeAnimations(AnimationManager.GetAnimations(AnimationController.currentAnimator));
        }

        protected override void Initialize(BaseGraphView graphView)
        {
            AnimationController.OnAnimatorChanged += ChangeAnimations;
            if (AnimationController.IsLoaded) ChangeAnimations(AnimationManager.GetAnimations(AnimationController.currentAnimator));
            else AnimationController.OnLoaded += LoadInitialAnimations;
            this.graphView = graphView;

            SetPosition(new Rect(24, 46, 140, 200));
        }

        private void PopulateScrollView()
        {
            foreach (var animation in _animations)
            {
                var button = new Button(() =>
                    {
                        var nodeType = typeof(AnimatorNode);
                        var node = BaseNode.CreateFromType(nodeType, new Vector2(180, 46));
                        if (node is AnimatorNode animNode)
                        {
                            animNode.animationName = animation;
                            animNode.animator = AnimationController.currentAnimator;
                        }

                        graphView.graph.AddNode(node);
                        graphView.AddNodeView(node);
                        graphView.MarkDirtyRepaint();

                        Debug.Log("Spawned animation: " + animation);
                    })
                    {text = animation};

                _scrollView.Add(button);
            }
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