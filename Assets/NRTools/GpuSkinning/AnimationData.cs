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

    public class AnimationData : ScriptableObject
    {
        public List<int> vertexIndices; // List of vertex indices (keys of dictionary)
        public List<FrameDelta> frameDeltas;
        public List<Matrix4x4> boneMatricesPerFrame;
    }

    [Serializable]
    public class FrameDelta
    {
        public List<VertexSkinData> deltaSkinData;
        public List<Vector3> deltaVertices; // This will be serialized properly by Unity

        public FrameDelta(int count)
        {
            deltaVertices = new List<Vector3>(count);
            deltaSkinData = new List<VertexSkinData>(count);
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
        public Vector4[] boneDirections;
        public List<VertexInfo> verticesInfo;
        public Dictionary<int, List<MorphDelta>> frameDeltas;
        public Dictionary<int, List<float[]>> boneMatricesPerFrame;
        public List<DualQuaternion> dualQuaternions;
    }
}