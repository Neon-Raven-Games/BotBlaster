using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphProcessor;
using NRTools.Animator.GraphView;
using NRTools.Animator.NRNodes;
using UnityEngine;
using AnimatorNode = NRTools.Animator.NRNodes.AnimatorNode;

public class AnimationGraph : BaseGraph
{
    protected override void OnEnable()
    {
        base.OnEnable();

    }

    // this is where we need the state machine data to serialize
    public void SaveGraphToFile(string path)
    {
        string json = GraphSerializer.SerializeGraph(this);
        File.WriteAllText(path, json);
    }

    public void LoadGraphFromFile(string path)
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            GraphSerializer.DeserializeGraph(this, json);
        }
    }
}


public static class GraphSerializer
{
    public static void GenerateTransitionsFromGraph(BaseGraph graph)
    {
    }

    public static string SerializeGraph(BaseGraph graph)
    {
        var graphData = new GraphData();
        foreach (var node in graph.nodes)
        {
            var nodeData = new NodeData
            {
                nodeId = node.GUID,
                nodeType = node.GetType().AssemblyQualifiedName,
                position = node.position.position,
                ports = new List<AnimPortData>(),
                nodeCustomIdentifier = node is AnimatorNode animNode ? animNode.animationName : ""
            };

            foreach (var port in node.inputPorts)
            {
                nodeData.ports.Add(new AnimPortData
                {
                    fieldName = port.fieldName,
                    portData = port.portData,
                    isInput = true
                });
            }

            foreach (var port in node.outputPorts)
            {
                nodeData.ports.Add(new AnimPortData
                {
                    fieldName = port.fieldName,
                    portData = port.portData,
                    isInput = false
                });
            }

            graphData.nodes.Add(nodeData);
        }

        // Serialize edges
        foreach (var edge in graph.edges)
        {
            graphData.edges.Add(new EdgeData
            {
                inputNodeId = edge.inputNode.GUID,
                outputNodeId = edge.outputNode.GUID,
                inputFieldName = edge.inputFieldName,
                inputPortIdentifier = edge.inputPortIdentifier,
                outputFieldName = edge.outputFieldName,
                outputPortIdentifier = edge.outputPortIdentifier
            });
        }

        return JsonUtility.ToJson(graphData, true);
    }


    public static void DeserializeGraph(BaseGraph graph, string json)
    {
        var graphData = JsonUtility.FromJson<GraphData>(json);

        foreach (var nodeData in graphData.nodes)
        {
            var nodeType = Type.GetType(nodeData.nodeType);
            var node = BaseNode.CreateFromType(nodeType, nodeData.position);
            node.GUID = nodeData.nodeId;

            if (node is AnimatorNode animNode)
            {
                animNode.animationName = nodeData.nodeCustomIdentifier;
            }

            graph.AddNode(node);
            node.position = new Rect(nodeData.position, node.position.size);
            foreach (var portData in nodeData.ports)
            {
                node.AddPort(portData.isInput, portData.fieldName, portData.portData);
            }
        }

        foreach (var edgeData in graphData.edges)
        {
            var inputNode = graph.nodes.FirstOrDefault(n => n.GUID == edgeData.inputNodeId);
            var outputNode = graph.nodes.FirstOrDefault(n => n.GUID == edgeData.outputNodeId);

            if (inputNode != null && outputNode != null)
            {
                var inputPort = inputNode.GetPort(edgeData.inputFieldName, edgeData.inputPortIdentifier);
                var outputPort = outputNode.GetPort(edgeData.outputFieldName, edgeData.outputPortIdentifier);

                if (inputPort != null && outputPort != null)
                {
                    graph.Connect(inputPort, outputPort);
                }
            }
        }
    }
}


[Serializable]
public class GraphData
{
    public List<NodeData> nodes = new();
    public List<EdgeData> edges = new();
}

[Serializable]
public class NodeData
{
    public string nodeId;
    public string nodeType;
    public Vector2 position;
    public List<AnimPortData> ports = new();
    public string nodeCustomIdentifier;
}

[Serializable]
public class EdgeData
{
    public string inputNodeId; // GUID of the input node
    public string outputNodeId; // GUID of the output node
    public string inputFieldName; // Field name of the input port
    public string inputPortIdentifier; // Identifier of the input port
    public string outputFieldName; // Field name of the output port
    public string outputPortIdentifier; // Identifier of the output port
}

[Serializable]
public class AnimPortData
{
    public string fieldName;
    public PortData portData;
    public bool isInput;
}