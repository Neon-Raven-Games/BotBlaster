using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    [Serializable]
    public class VertexSkinData
    {
        public int4 boneIndices; // Indices of bones affecting this vertex
        public float4 boneWeights; // Weights for each bone
    }

    [Serializable]
    public class AnimationLookupTable
    {
        public Dictionary<string, Dictionary<string, AnimationData>> lookupTable;

        public AnimationLookupTable()
        {
            lookupTable = new Dictionary<string, Dictionary<string, AnimationData>>();
        }

        public int Count => lookupTable.Count;

        public void AddAnimation(string enemyType, string animationName, AnimationData animData)
        {
            if (!lookupTable.ContainsKey(enemyType))
            {
                lookupTable[enemyType] = new Dictionary<string, AnimationData>();
            }
            lookupTable[enemyType][animationName] = animData;
        }

        public AnimationData GetAnimationData(string enemyType, string animationName)
        {
            if (lookupTable.ContainsKey(enemyType) && lookupTable[enemyType].ContainsKey(animationName))
            {
                return lookupTable[enemyType][animationName];
            }
            return null;
        }
    }

    public class AnimationTransitionData
    {
        public bool loop;
        // what frame to start on in the from animation
        public float blendStartTime;
        
        // in seconds
        public float blendDuration;
        public float blendWeight;
        
        public string fromAnimation;
        public string toAnimation;
    }

    [Serializable]
    public class AnimationData
    {
        public string animationName;
        public int vertexOffset;
        public int frameCount;
        public int vertexCount;
        public bool loop;

        public AnimationData(string animationName, int vertexOffset, int frameCount, int vertexCount, bool loop)
        {
            this.animationName = animationName;
            this.vertexOffset = vertexOffset;
            this.frameCount = frameCount;
            this.vertexCount = vertexCount;
            this.loop = loop;
        }
    }


    // v4 over v3: float4 caveat in engine channel of discord
    // https://developer.nvidia.com/content/understanding-structured-buffer-performance
    // we need ot test the performance to the test with xyz, instead of v4
    [Serializable]
    public struct VertexInfo
    {
        public int vertexID;
        public Vector4 position;
        public Vector4 normal;
        public Vector4 tangent;

        public int boneIndex0;
        public int boneIndex1;
        public int boneIndex2;
        public int boneIndex3;

        public float weight0;
        public float weight1;
        public float weight2;
        public float weight3;

        public float compensation_coef;
    }
    
    [Serializable]
    public struct MorphDelta
    {
        // float4 caveat in engine channel of discord
        // https://developer.nvidia.com/content/understanding-structured-buffer-performance
        public Vector4 position;
        public Vector4 normal;
        public Vector4 tangent;
    }
    [Serializable]
    public struct DualQuaternion
    {
        public Quaternion rotationQuaternion;
        public Vector4 position;
    }
    
    [Serializable]
    public class DualQuaternionAnimationData
    {
        public int numFrames;
        public List<MorphDelta> frameDeltas;
    }
}