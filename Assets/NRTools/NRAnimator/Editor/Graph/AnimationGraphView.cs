using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine;
using GraphProcessor;
using NRTools.CustomAnimator;
using UnityEditor;
using AnimatorNode = NRTools.Animator.NRNodes.AnimatorNode;

public class AnimationGraphView : BaseGraphView
{
    private AnimatorNode _selectedNode;
    public void ColorNode(AnimatorNode animatorNode)
    {
        if (_selectedNode != null)
        {
            _selectedNode.isActive = false;
        }
        _selectedNode = animatorNode;
        _selectedNode.isActive = true;
    }
    public AnimationGraphView(EditorWindow window) : base(window)
    {
        AnimationController.OnAnimationChanged += ColorNode;
    }
    

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        BuildStackNodeContextualMenu(evt);
        base.BuildContextualMenu(evt);
    }

    protected override void InitializeView()
    {
        base.InitializeView();
        var runtimeGraph = new RuntimeAnimationGraph();
        foreach (AnimatorNode node in graph.nodes)
        {
            var runtimeNode = new RuntimeAnimatorNode()
            {
                GUID = node.GUID,
                edges = new(),
                animationName = node.animationName
            };

            var inputEdges = node.inputPorts.FirstOrDefault()?.GetEdges();
            if (inputEdges == null || inputEdges.Count == 0)
            {
                runtimeGraph.AddParentLevelNode(runtimeNode);
            }
            else
            {
                runtimeGraph.AddNode(runtimeNode);
            }

            PopulateNode(node, runtimeNode, runtimeGraph);
        }

        var path = Path.Combine(Application.streamingAssetsPath, "path_to_save.json");
        File.WriteAllText(path, runtimeGraph.SerializeGraph());
    }

    private void PopulateNode(AnimatorNode node, RuntimeAnimatorNode runtimeNode, RuntimeAnimationGraph runtimeGraph)
    {
        if (node == null || runtimeNode == null) return;

        foreach (AnimatorNode outNode in node.GetOutputNodes())
        {
            var targetNode = graph.nodes.FirstOrDefault(n =>
                (n as AnimatorNode).animationName == outNode.animationName) as AnimatorNode;
            foreach (var input in outNode.outputPorts.FirstOrDefault().GetEdges())
            {
                if (outNode.transitionsTo.ContainsKey((input.inputNode as AnimatorNode).animationName))
                {
                    targetNode = input.inputNode as AnimatorNode;
                    break;
                }
            }

            if (targetNode != null)
            {
                var runtimeEdge = new RuntimeEdge()
                {
                    toNode = new RuntimeAnimatorNode()
                    {
                        GUID = outNode.GUID,
                        animationName = outNode.animationName,
                    },
                };

                if (outNode.transitionsTo.ContainsKey(targetNode.animationName))
                {
                    runtimeEdge.toNode.transitions.Add(targetNode.animationName,
                        outNode.transitionsTo[targetNode.animationName]);
                }

                runtimeGraph.AddAnimationEdge(runtimeNode, targetNode.animationName, runtimeEdge);
                PopulateNode(outNode, runtimeEdge.toNode, runtimeGraph);
            }
            else
            {
                Debug.LogError($"Target node for animation '{outNode.animationName}' not found.");
            }
        }
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