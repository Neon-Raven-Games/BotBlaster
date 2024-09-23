using System.Collections.Generic;
using GraphProcessor;
using NRTools.Animator.NRNodes;
using UnityEngine;
using UnityEngine.UIElements;

namespace NRTools.CustomAnimator
{
    public class AnimationsElementView : PinnedElementView
    {
        public string currentAnimator;
        
        BaseGraphView graphView;
        public AnimationsElementView() => title = "Animations";
        public List<string> animations = new();
        private ScrollView scrollView;

        private void ChangeAnimations(List<string> anims)
        {
            animations = anims;
            scrollView.Clear();
            PopulateScrollView();
        }
        protected override void Initialize(BaseGraphView graphView)
        {
            AnimationController.OnAnimatorChanged += ChangeAnimations;
            animations = AnimationController.GetAnimations("Tank");
            this.graphView = graphView;
            scrollView = new ScrollView();
            var searchField = new TextField();
            content.Add(searchField);
            
            PopulateScrollView();
            
            searchField.RegisterValueChangedCallback(evt => Search(evt.newValue));
            content.Add(scrollView);
            SetPosition(new Rect(24, 46, 140, 200));
        }

        private void PopulateScrollView()
        {
            foreach (var animation in animations)
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

                scrollView.Add(button);
            }
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