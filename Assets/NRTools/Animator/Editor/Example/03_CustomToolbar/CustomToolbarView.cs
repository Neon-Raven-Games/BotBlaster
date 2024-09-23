using UnityEngine;
using GraphProcessor;
using NRTools.CustomAnimator;
using UnityEditor;
using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

// todo, handling the path better would be nice. 
// the graph behavior will serve as our base animator graph
public class CustomToolbarView : ToolbarView
{
    private readonly AnimationGraphView _animationGraphView;
    private AnimationGraph _animationGraph;
    public CustomToolbarView(BaseGraphView graphView) : base(graphView)
    {
        _animationGraphView = graphView as AnimationGraphView;
        _animationGraph = graphView.graph as AnimationGraph;
    }

    protected override void AddButtons()
    {
        base.AddButtons();
        AddButton("Refresh", () => Debug.Log("todo: Reload Animation File"), left: false);
        
        bool conditionalProcessorVisible = graphView.GetPinnedElementStatus<AnimatorsElementView>() != Status.Hidden;
        AddToggle("Open Animators", conditionalProcessorVisible,
            (v) => graphView.ToggleView<AnimatorsElementView>());
        
        bool animationsVisible = graphView.GetPinnedElementStatus<AnimationsElementView>() != Status.Hidden;
        AddToggle("Open Animations", animationsVisible,
            (v) => graphView.ToggleView<AnimationsElementView>());
        
        // todo, these buttons do nothing
        AddButton("Save graph", () =>
        {
            EditorUtility.SetDirty(_animationGraph);
            AssetDatabase.SaveAssets();
        }, left: false);
        
        AddButton("Load graph", () =>
        {
            _animationGraph = AssetDatabase.LoadAssetAtPath<AnimationGraph>("Assets/animations.asset");
            _animationGraphView.graph = _animationGraph;
            _animationGraphView.MarkDirtyRepaint();
        }, left: false);
        
        bool transitions = graphView.GetPinnedElementStatus<TransitionElementView>() != Status.Hidden;
        AddToggle("View TransitionData", transitions,
            (v) => graphView.ToggleView<TransitionElementView>());

    }
}