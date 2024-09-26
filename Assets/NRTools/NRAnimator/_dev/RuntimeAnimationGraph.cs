using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NRTools.Animator.NRNodes;
using NRTools.GpuSkinning;
using Unity.VisualScripting;
using UnityEngine;

namespace GraphProcessor
{
    // runtime edge holds the edge transition data to invoke from an observer to update the runtime animator instance
    public class RuntimeEdge
    {
        public RuntimeAnimatorNode toNode;
    }

    // runtime animator node, should have animation data and transitional data to each of it's edges
    [Serializable]
    public class RuntimeAnimatorNode
    {
        public string animationName;
        public string GUID;
        public Dictionary<string, AnimationTransitionData> transitions = new();
        public List<RuntimeEdge> edges = new();

        public void AddAnimationEdge(string animation, RuntimeEdge edge)
        {
            edges.Add(edge);
        }

        public bool CanTransition(out AnimationTransitionData transition)
        {
            transition = transitions.Values.FirstOrDefault();
            return true;
        }
    }

    public class RuntimeAnimationGraph
    {
        private RuntimeAnimatorNode _currentNode;
        private Dictionary<string, RuntimeAnimatorNode> Animations = new();
        private Dictionary<string, RuntimeAnimatorNode> NodesByGUID = new();

        public void AddParentLevelNode(RuntimeAnimatorNode runtimeAnimatorNode)
        {
            if (Animations.ContainsKey(runtimeAnimatorNode.GUID))
            {
                Debug.LogError(
                    $"Animator {runtimeAnimatorNode.GUID}{runtimeAnimatorNode.animationName} already exists.");
            }
            else
            {
                Animations.Add(runtimeAnimatorNode.GUID, runtimeAnimatorNode);
                NodesByGUID.Add(runtimeAnimatorNode.GUID, runtimeAnimatorNode); // Track node by GUID
            }
        }

        public void AddEdge(string parentGuid, RuntimeEdge edge)
        {
            NodesByGUID[parentGuid].AddAnimationEdge(edge.toNode.animationName, edge);
        }

        public RuntimeEdge Traverse(out AnimationTransitionData transitionData)
        {
            if (_currentNode.edges.Count == 0 && Animations.ContainsKey(_currentNode.GUID))
                _currentNode = Animations[_currentNode.GUID];
            
            foreach (var edge in _currentNode.edges)
            {
                if (!edge.toNode.CanTransition(out transitionData)) continue;
                _currentNode = edge.toNode;
                return edge;
            }
            
            transitionData = null;
            return null;
        }

        // todo, we need to search for guid
        public RuntimeAnimatorNode FindNode(string animationName)
        {
            // Perform BFS/DFS across the graph to find the node by its animation name
            foreach (var node in Animations.Values)
            {
                if (node.animationName == animationName)
                {
                    _currentNode = node;
                    return node;
                }
            }

            Debug.LogWarning($"Animation node {animationName} not found.");
            return null;
        }

        public RuntimeAnimatorNode GetCurrentNode() => _currentNode;

        public AnimationTransitionData GetTransitionData(string animationName)
        {
            _currentNode.transitions.TryGetValue(animationName, out var transitionData);
            if (transitionData == null)
            {
                var animEdge = _currentNode.edges.Find(x => x.toNode.animationName == animationName);
                if (animEdge != null) return animEdge.toNode.transitions[animationName];
                if (Animations.ContainsKey(_currentNode.GUID)) _currentNode = Animations[_currentNode.GUID];
                _currentNode.transitions.TryGetValue(animationName, out transitionData);
            }

            return transitionData;
        }
        
        public void SetNode(string guid)
        {
            if (Animations.ContainsKey(guid))
            {
                _currentNode = Animations[guid];
                return;
            }
            
            foreach (var node in Animations.Values)
            {
                if (node.GUID == guid)
                {
                    _currentNode = node;
                    return;
                }
            }
            Debug.LogWarning($"Animation node {guid} not found.");
        }
        // Find a node by GUID
        public RuntimeAnimatorNode FindNodeByGUID(string guid)
        {
            if (NodesByGUID.ContainsKey(guid))
            {
                return NodesByGUID[guid];
            }

            Debug.LogWarning($"Node with GUID {guid} not found.");
            return null;
        }


        public void RecurseEdges(RuntimeAnimatorNode node, Dictionary<string, RuntimeAnimatorNode> dups,
            List<RuntimeAnimatorNode> toAdd)
        {
            foreach (var e in node.edges)
            {
                if (!dups.ContainsKey(e.toNode.GUID))
                {
                    dups.Add(e.toNode.GUID, e.toNode);
                }
                else
                {
                    dups[e.toNode.GUID].edges.Clear();
                    dups[e.toNode.GUID] = e.toNode;
                    toAdd.Add(e.toNode);
                }
                RecurseEdges(e.toNode, dups, toAdd);
            }
        }

        
        public string SerializeGraph()
        {
            var dupDict = new Dictionary<string, RuntimeAnimatorNode>();
            var toAdd = new List<RuntimeAnimatorNode>();
            foreach (var node in Animations)
            {
                RecurseEdges(node.Value, dupDict, toAdd);
            }

            foreach (var node in toAdd)
            {
                var clone = JsonConvert.SerializeObject(dupDict[node.GUID]);
                node.edges.Clear();
                dupDict[node.GUID].edges.Clear();
                var deserialized = JsonConvert.DeserializeObject<RuntimeAnimatorNode>(clone);
                if (!Animations.ContainsKey(node.GUID))
                {
                    Animations.Add(node.GUID, deserialized);
                }
            }

            var jsonOptions = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented // Make the JSON more readable
            };

            return JsonConvert.SerializeObject(Animations, jsonOptions);
        }

        public void DeserializeGraph(string json)
        {
            Animations = JsonConvert.DeserializeObject<Dictionary<string, RuntimeAnimatorNode>>(json);
            _currentNode = Animations.Values.FirstOrDefault();
            // NodesByGUID = Animations.Values.ToDictionary(node => node.GUID);
        }

        public bool AddNode(RuntimeAnimatorNode node)
        {
            if (NodesByGUID.ContainsKey(node.GUID))
            {
                return false;
            }

            NodesByGUID[node.GUID] = node;
            return true;
        }

        public void AddAnimationEdge(RuntimeAnimatorNode runtimeNode, string targetNodeAnimationName,
            RuntimeEdge runtimeEdge)
        {
            runtimeNode.AddAnimationEdge(targetNodeAnimationName, runtimeEdge);
        }
    }
}