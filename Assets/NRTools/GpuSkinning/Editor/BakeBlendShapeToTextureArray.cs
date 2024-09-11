using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class BakeBlendShapeToTextureArray : EditorWindow
    {
        private SkinnedMeshRenderer _skinnedMeshRenderer;
        private AnimationClip _animationClip;
        private List<Vector3> _originalVerts;
        private readonly List<FrameDelta> _deltas = new();


        [MenuItem("Neon Raven/GPU Skinning/BlendShape")]
        public static void ShowWindow()
        {
            GetWindow<BakeBlendShapeToTextureArray>("Bake Blend Shape");
        }

        private void OnGUI()
        {
            _skinnedMeshRenderer = (SkinnedMeshRenderer) EditorGUILayout.ObjectField("Skinned Mesh Renderer",
                _skinnedMeshRenderer, typeof(SkinnedMeshRenderer), true);
            _animationClip =
                (AnimationClip) EditorGUILayout.ObjectField("Animation Clip", _animationClip, typeof(AnimationClip),
                    true);

            if (_skinnedMeshRenderer != null && _animationClip != null)
            {
                if (GUILayout.Button("Bake GPU Animation Data"))
                {
                    BakeAnimationVertexData(_skinnedMeshRenderer, _animationClip);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Assign a Skinned Mesh Renderer and an Animation Clip to bake.",
                    MessageType.Info);
            }
        }

        private void BakeAnimationVertexData(SkinnedMeshRenderer skinnedMeshRenderer, AnimationClip animationClip)
        {
            var bakedMesh = new Mesh();

            var vertIndices = new List<int>();
            skinnedMeshRenderer.BakeMesh(bakedMesh);

            _originalVerts = new List<Vector3>();

            for (var i = 0; i < bakedMesh.vertices.Length; i++)
            {
                vertIndices.Add(i);
                _originalVerts.Add(bakedMesh.vertices[i]);
            }

            HashSet<float> keyframeTimes = GetKeyframeTimes(animationClip);
            _deltas.Clear();

            var boneMatricesPerFrame = new List<Matrix4x4>();
            var frameIndex = 0;
            foreach (var time in keyframeTimes)
            {
                var progress = (float) frameIndex / keyframeTimes.Count;
                EditorUtility.DisplayProgressBar("Baking Animation",
                    $"Processing frame {frameIndex + 1} of {keyframeTimes.Count}", progress);

                _deltas.Add(new FrameDelta(bakedMesh.vertexCount));

                animationClip.SampleAnimation(skinnedMeshRenderer.transform.parent.gameObject, time);
                skinnedMeshRenderer.BakeMesh(bakedMesh);

                for (var boneIndex = 0; boneIndex < skinnedMeshRenderer.bones.Length; boneIndex++)
                {
                    boneMatricesPerFrame.Add(
                        skinnedMeshRenderer.bones[boneIndex].localToWorldMatrix *
                        skinnedMeshRenderer.sharedMesh.bindposes[boneIndex]);
                }
                Debug.Log($"Vertex count: {bakedMesh.vertexCount}, Bone weights length: {skinnedMeshRenderer.sharedMesh.boneWeights.Length}");

                AssignDeltas(frameIndex, bakedMesh);

                for (var i = 0; i < bakedMesh.vertexCount; i++)
                {
                    if (i >= skinnedMeshRenderer.sharedMesh.boneWeights.Length)
                    {
                        Debug.LogError($"Bone weights array is smaller than the vertex count at vertex {i}");
                        break;
                    }
                    var boneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;
                    var skinData = new VertexSkinData
                    {
                        boneIndices = new int4(
                            boneWeights[i].boneIndex0,
                            boneWeights[i].boneIndex1,
                            boneWeights[i].boneIndex2,
                            boneWeights[i].boneIndex3),
                        boneWeights = new float4(
                            boneWeights[i].weight0,
                            boneWeights[i].weight1,
                            boneWeights[i].weight2,
                            boneWeights[i].weight3)
                    };
                    _deltas[frameIndex].deltaSkinData.Add(skinData);
                }


                frameIndex++;
            }

            EditorUtility.ClearProgressBar();
            var data = CreateInstance<AnimationData>();
            data.frameDeltas = _deltas;
            data.vertexIndices = vertIndices;
            data.boneMatricesPerFrame = boneMatricesPerFrame;
            var path = EditorUtility.SaveFilePanelInProject("Save GPU Animation Data", "GPUAnimation", "asset",
                "Please enter a file name to save the GPU animation data.");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(data, path);
            }
        }

        private void AssignDeltas(int frameIndex, Mesh bakedMesh)
        {
            var vertices = bakedMesh.vertices;
            var vertexCount = bakedMesh.vertexCount;

            if (bakedMesh.vertexCount != _originalVerts.Count)
            {
                Debug.LogError(
                    $"Vertex count mismatch at frame {frameIndex}. Original: {_originalVerts.Count}, Current: {bakedMesh.vertexCount}");
                return;
            }


            for (var i = 0; i < vertexCount; i++)
            {
                _deltas[frameIndex].deltaVertices.Add(vertices[i] - _originalVerts[i]);
            }
        }

        private HashSet<float> GetKeyframeTimes(AnimationClip clip)
        {
            var keyframeTimes = new HashSet<float>();

            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                foreach (var keyframe in curve.keys)
                {
                    keyframeTimes.Add(keyframe.time);
                }
            }

            Debug.Log(keyframeTimes.Count + " keyframes found");
            return keyframeTimes;
        }
    }
}