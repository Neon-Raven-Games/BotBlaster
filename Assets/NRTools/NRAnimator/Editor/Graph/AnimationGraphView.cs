using System.IO;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine;
using GraphProcessor;
using NRTools.Animator.NRNodes;
using UnityEditor;
using AnimatorNode = NRTools.Animator.NRNodes.AnimatorNode;

public class AnimationGraphView : BaseGraphView
{
    public AnimationGraphView(EditorWindow window) : base(window)
    {
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        BuildStackNodeContextualMenu(evt);
        base.BuildContextualMenu(evt);
    }

    public void AddNode(AnimatorNode node, RuntimeAnimationGraph runtimeGraph)
    {
        runtimeGraph.AddAnimationNode(node.GUID, new AnimationNodeData
        {
            animationName = node.animationName,
            data = node.data
        });
        
        foreach (var transition in node.transitionsTo)
        {
            runtimeGraph.AddTransition(node.animator, transition.Key, transition.Value.toAnimation, transition.Value);
        }
        foreach (var child in node.outputPorts.SelectMany(p => p.GetEdges().Select(e => e.inputNode as AnimatorNode)))
        {
            AddNode(child, runtimeGraph);
        }
    }
    
    protected override void InitializeView()
    {
        base.InitializeView();

        // todo, lets construct a real graph off of this data,
        // then we can pass it to the animator
        // var runtimeGraph = new RuntimeAnimationGraph();
        // foreach (AnimatorNode node in graph.nodes)
        // {
        //     // AddNode(node, runtimeGraph);
        // }
        //
        // Debug.Log(graph.nodes.Count);
        // var path = Path.Combine(Application.streamingAssetsPath, "path_to_save.json");
        // Debug.Log(runtimeGraph.SerializeGraph());
        // File.WriteAllText(path, runtimeGraph.SerializeGraph());
    }

    /// <summary>
    /// Add the New Stack entry to the context menu
    /// </summary>
    /// <param name="evt"></param>
    protected void BuildStackNodeContextualMenu(ContextualMenuPopulateEvent evt)
    {
        Vector2 position =
            (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);

        evt.menu.AppendAction("New Stack", (e) =>
            AddStackNode(new BaseStackNode(position)), DropdownMenuAction.AlwaysEnabled);
    }
}