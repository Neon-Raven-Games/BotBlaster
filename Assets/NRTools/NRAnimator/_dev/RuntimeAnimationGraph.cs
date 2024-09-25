using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NRTools.Animator.NRNodes;
using NRTools.GpuSkinning;
using UnityEngine;

namespace GraphProcessor
{
    [Serializable]
    public class AnimationNodeData
    {
        public string animationName;  // Animation name (e.g., "Idle", "Walk")
        public AnimationData data;    // Holds actual animation data

        public Dictionary<string, AnimationTransitionData> transitionsTo = new();

        public void AddTransition(string fromAnim, AnimationTransitionData transitionData)
        {
            if (!transitionsTo.ContainsKey(fromAnim))
            {
                transitionsTo[fromAnim] = transitionData;
            }
            else
            {
                Debug.LogWarning($"Transition from {fromAnim} already exists.");
            }
        }
    }

    [Serializable]
    public class RuntimeAnimatorNode
    {
        public string animatorName;  // e.g., "EnemyType.Tank"
        public Dictionary<string, AnimationNodeData> animations = new();  // Maps animation names to AnimationNodeData

        // Add an animation to the animator node
        public void AddAnimationNode(AnimationNodeData animationNode)
        {
            if (!animations.ContainsKey(animationNode.animationName))
            {
                animations[animationNode.animationName] = animationNode;
            }
            else
            {
                Debug.LogWarning($"Animation {animationNode.animationName} already exists for {animatorName}.");
            }
        }

        // Get the transition data for a specific animation within this animator
        public AnimationNodeData GetNextNode(string currentAnim, string nextAnim)
        {
            if (animations.ContainsKey(currentAnim) && animations[currentAnim].transitionsTo.ContainsKey(nextAnim))
            {
                var nextTransition = animations[currentAnim].transitionsTo[nextAnim];
                return animations.ContainsKey(nextTransition.toAnimation) ? animations[nextTransition.toAnimation] : null;
            }

            return null;
        }
    }

    [Serializable]
    public class RuntimeAnimationGraph
    {
        // Top-level animator nodes (e.g., EnemyType.Tank -> AnimatorNode)
        private Dictionary<string, RuntimeAnimatorNode> animators = new();

        // Add an animator node (e.g., EnemyType.Tank)
        public void AddAnimatorNode(RuntimeAnimatorNode runtimeAnimatorNode)
        {
            if (!animators.ContainsKey(runtimeAnimatorNode.animatorName))
            {
                animators[runtimeAnimatorNode.animatorName] = runtimeAnimatorNode;
            }
            else
            {
                Debug.LogWarning($"Animator {runtimeAnimatorNode.animatorName} already exists.");
            }
        }

        public void AddAnimationNode(string animatorName, AnimationNodeData animationNode)
        {
            if (animators.ContainsKey(animatorName))
            {
                animators[animatorName].AddAnimationNode(animationNode);
            }
            else
            {
                animators.Add(animatorName, new RuntimeAnimatorNode());
                Debug.LogError($"Animator {animatorName} does not exist.");
            }
        }
        // Add a transition between animations within a specific animator
        public void AddTransition(string animatorName, string fromAnim, string toAnim, AnimationTransitionData transitionData)
        {
            if (animators.ContainsKey(animatorName))
            {
                var animator = animators[animatorName];
                if (animator.animations.ContainsKey(fromAnim))
                {
                    animator.animations[fromAnim].AddTransition(toAnim, transitionData);
                }
                else
                {
                    Debug.LogError($"Animation {fromAnim} not found in {animatorName}.");
                }
            }
        }

        // O(1) lookup of the next animation within the same animator
        public AnimationNodeData Traverse(
            string animatorName, string currentAnim, string nextAnim)
        {
            if (animators.ContainsKey(animatorName))
            {
                var animatorNode = animators[animatorName];
                return animatorNode.GetNextNode(currentAnim, nextAnim);
            }

            Debug.LogWarning($"Animator {animatorName} or animation {currentAnim} not found.");
            return null;
        }

        // Optional override functionality to find an animation node by name across all animators
        public AnimationNodeData FindNode(string animationName)
        {
            foreach (var animator in animators.Values)
            {
                if (animator.animations.ContainsKey(animationName))
                {
                    return animator.animations[animationName];
                }
            }

            Debug.LogWarning($"Animation {animationName} not found in any animator.");
            return null;
        }

        // Serialize the graph to JSON, only saving necessary data
        public string SerializeGraph()
        {
            var jsonOptions = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented  // Make the JSON more readable
            };

            return JsonConvert.SerializeObject(animators, jsonOptions);
        }

        // Deserialize the graph from JSON
        public void DeserializeGraph(string json)
        {
            animators = JsonConvert.DeserializeObject<Dictionary<string, RuntimeAnimatorNode>>(json);
        }
    }
}
