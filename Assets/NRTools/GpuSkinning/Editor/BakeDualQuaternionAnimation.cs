using System.Collections.Generic;
using System.IO;
using System.Linq;
using NRTools.GpuSkinning.Util;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class BakeDualQuaternionAnimation : EditorWindow
    {
        private Animator _rootAnimationObject;
        private SkinnedMeshRenderer _skinnedMeshRenderer;
        private AnimationClip _animationClip;

        [MenuItem("Neon Raven/GPU Skinning/Dual Quaternion Animation")]
        public static void ShowWindow()
        {
            GetWindow<BakeDualQuaternionAnimation>("Dual Quaternion Animation");
        }

        private void OnGUI()
        {
            _skinnedMeshRenderer = (SkinnedMeshRenderer) EditorGUILayout.ObjectField("Skinned Mesh Renderer",
                _skinnedMeshRenderer, typeof(SkinnedMeshRenderer), true);

            _rootAnimationObject = (Animator) EditorGUILayout.ObjectField("Root Animation Object",
                _rootAnimationObject, typeof(Animator), true);

            _animationClip =
                (AnimationClip) EditorGUILayout.ObjectField("Animation Clip", _animationClip, typeof(AnimationClip),
                    true);

            if (_skinnedMeshRenderer != null && _animationClip != null && _rootAnimationObject != null)
            {
                if (GUILayout.Button("Bake GPU Animation Data"))
                {
                    BakeAnimationVertexData(_skinnedMeshRenderer, _animationClip, _rootAnimationObject);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Assign a Skinned Mesh Renderer and an Animation Clip to bake.",
                    MessageType.Info);
            }
        }

        private static void BakeAnimationVertexData(SkinnedMeshRenderer skinnedMeshRenderer,
            AnimationClip animationClip, Animator animator)
        {
            // init mesh at keyframe
            var bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);

            // bone matrices
            var boneMatricesPerFrame = new Dictionary<int, List<float[]>>();
            // vertices info
            var verticesInfoCollection = new List<VertexInfo>();
            // morph deltas
            var deltas = new List<List<MorphDelta>>();
            // dual quaternion translations
            var dualQuaternions = ExtractBindPoseTranslations(skinnedMeshRenderer);

            // get bone weights to populate vertex info
            var boneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;

            // process all verts in first frame
            for (var i = 0; i < bakedMesh.vertices.Length; i++)
            {
                if (i >= skinnedMeshRenderer.sharedMesh.boneWeights.Length)
                {
                    Debug.LogError($"Bone weights array is smaller than the vertex count at vertex {i}");
                    break;
                }

                var vertInfo = new VertexInfo
                {
                    vertexID = i,
                    position = bakedMesh.vertices[i],
                    normal = bakedMesh.normals[i],
                    tangent = bakedMesh.tangents[i],
                    boneIndex0 = boneWeights[i].boneIndex0,
                    boneIndex1 = boneWeights[i].boneIndex1,
                    boneIndex2 = boneWeights[i].boneIndex2,
                    boneIndex3 = boneWeights[i].boneIndex3,
                    weight0 = boneWeights[i].weight0,
                    weight1 = boneWeights[i].weight1,
                    weight2 = boneWeights[i].weight2,
                    weight3 = boneWeights[i].weight3,
                };
                verticesInfoCollection.Add(vertInfo);
            }

            var keyTime = GetKeyframeTimes(animationClip);
            var keyframeTimes = keyTime.ToList();
            keyframeTimes.Sort();

            var frameIndex = 0;
            foreach (var time in keyframeTimes)
            {
                var progress = (float) frameIndex / keyframeTimes.Count;
                EditorUtility.DisplayProgressBar("Baking Animation",
                    $"Processing frame {frameIndex + 1} of {keyframeTimes.Count}", progress);
                    deltas.Add(new List<MorphDelta>());

                    animationClip.SampleAnimation(animator.gameObject, time);
                    skinnedMeshRenderer.BakeMesh(bakedMesh);

                    boneMatricesPerFrame.Add(frameIndex, new List<float[]>());
                    for (var boneIndex = 0; boneIndex < skinnedMeshRenderer.bones.Length; boneIndex++)
                    {
                        var matrix =
                            skinnedMeshRenderer.bones[boneIndex].localToWorldMatrix *
                            skinnedMeshRenderer.sharedMesh.bindposes[boneIndex];

                        boneMatricesPerFrame[frameIndex].Add(matrix.ToFloatArray());
                    }

                    for (var i = 0; i < bakedMesh.vertexCount; i++)
                    {
                        var morphD = new MorphDelta
                        {
                            position = bakedMesh.vertices[i],
                            normal = bakedMesh.normals[i],
                            tangent = bakedMesh.tangents[i],
                        };

                        deltas[frameIndex].Add(morphD);
                    }

                frameIndex++;
            }

            EditorUtility.ClearProgressBar();
            // todo, switch to binary when valid
            var path = EditorUtility.SaveFilePanelInProject("Save GPU Animation Json",
                "GPUAnimation", "json",
                "Please enter a file name to save the GPU animation data.");
            if (!string.IsNullOrEmpty(path))
            {
                var data = new DualQuaternionAnimationData
                {
                    verticesInfo = verticesInfoCollection,
                    boneMatricesPerFrame = boneMatricesPerFrame,
                    dualQuaternions = dualQuaternions,
                    boneDirections = InitializeBoneDirectionBuffer(skinnedMeshRenderer)
                };

                data.frameDeltas = deltas;
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented
                };

                var json = JsonConvert.SerializeObject(data, settings);
                File.WriteAllText(path, json);
                Debug.Log("GPU Animation Data saved to " + path);
            }
        }

        private static Vector4[] InitializeBoneDirectionBuffer(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var boneCount = skinnedMeshRenderer.bones.Length;
            var boneDirections = new Vector4[boneCount];

            for (int i = 0; i < boneCount; i++)
            {
                Transform bone = skinnedMeshRenderer.bones[i];

                // If the bone has a child, calculate direction towards the child
                if (bone.childCount > 0)
                {
                    Transform childBone = bone.GetChild(0); // Assuming it's a simple chain
                    Vector3 direction = (childBone.position - bone.position).normalized;
                    boneDirections[i] = new Vector4(direction.x, direction.y, direction.z, 0);
                }
                else
                {
                    // If there is no child, assign a default direction (could be identity or any other vector)
                    boneDirections[i] = new Vector4(0, 1, 0, 0); // Default upward direction
                }
            }

            return boneDirections;
        }

        private static List<DualQuaternion> ExtractBindPoseTranslations(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var bindPoses = skinnedMeshRenderer.sharedMesh.bindposes;
            var bindDqs = new DualQuaternion[bindPoses.Length];

            for (var i = 0; i < bindPoses.Length; i++)
            {
                var rotation = bindPoses[i].ExtractRotation();
                rotation.Normalize();

                bindDqs[i].rotationQuaternion = rotation;
                bindDqs[i].position = bindPoses[i].ExtractPosition();
            }

            return bindDqs.ToList();
        }

        private static HashSet<float> GetKeyframeTimes(AnimationClip clip)
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