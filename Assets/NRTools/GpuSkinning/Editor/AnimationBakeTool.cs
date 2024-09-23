using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.Plastic.Newtonsoft.Json;

namespace NRTools.GpuSkinning
{
    public class AnimationBakeTool : EditorWindow
    {
        [MenuItem("Neon Raven/GPU Skinning/Build AssetBundles")]
        private static void BuildAllAssetBundles()
        {
            // The path where Asset Bundles will be created
            string assetBundleDirectory = "Assets/AssetBundles";
            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }

            BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                BuildAssetBundleOptions.None,
                BuildTarget.Android);
        }

        private string _rootFolder = "Assets/Animations"; // Root folder where enemy prefabs and animations are stored
        private AnimationLookupTable _lookupTable = new();
        private readonly List<Vector3> _allVertices = new();

        [MenuItem("Neon Raven/GPU Skinning/Bake Animations")]
        public static void ShowWindow()
        {
            GetWindow<AnimationBakeTool>("Animation Bake Tool");
        }

        private void OnGUI()
        {
            _rootFolder = EditorGUILayout.TextField("Root Folder", _rootFolder);

            if (GUILayout.Button("Bake All Animations"))
            {
                BakeAnimations();
            }
        }

        private void BakeAnimations()
        {
            // Clear the lookup table and vertex list
            _lookupTable = new AnimationLookupTable();
            _allVertices.Clear();

            var enemyFolders = Directory.GetDirectories(_rootFolder);

            var currentOffset = 0;
            var totalAnimations = 0;

            foreach (var enemyFolder in enemyFolders)
            {
                var animationFiles = Directory.GetFiles(enemyFolder, "*.anim");
                totalAnimations += animationFiles.Length;
            }

            var animationCount = 0;

            foreach (var enemyFolder in enemyFolders)
            {
                var enemyName = new DirectoryInfo(enemyFolder).Name;

                var prefabFiles = Directory.GetFiles(enemyFolder, "*.prefab");
                if (prefabFiles.Length == 0)
                {
                    Debug.LogWarning($"No prefab found in {enemyFolder}");
                    continue;
                }

                var prefabPath = prefabFiles[0]; // Assume there's one prefab per folder
                var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (enemyPrefab == null)
                {
                    Debug.LogWarning($"Prefab could not be loaded from {prefabPath}");
                    continue;
                }

                var meshRenderer = enemyPrefab.GetComponentInChildren<MeshRenderer>();
                var skinnedMeshRenderer = enemyPrefab.GetComponentInChildren<SkinnedMeshRenderer>();

                if (skinnedMeshRenderer != null)
                {
                    var animationFiles = Directory.GetFiles(enemyFolder, "*.anim");
                    foreach (var animFile in animationFiles)
                    {
                        AnimationClip animClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animFile);

                        // Update progress bar for each animation
                        animationCount++;
                        var progress = (float) animationCount / totalAnimations;
                        EditorUtility.DisplayProgressBar("Baking Animations", $"Processing {animClip.name}", progress);

                        BakeAnimationForSkinnedMesh(enemyName, animClip, skinnedMeshRenderer, ref currentOffset);
                    }
                }
                else if (meshRenderer != null)
                {
                    var animationFiles = Directory.GetFiles(enemyFolder, "*.anim");
                    foreach (string animFile in animationFiles)
                    {
                        var animClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animFile);

                        animationCount++;
                        var progress = (float) animationCount / totalAnimations;
                        EditorUtility.DisplayProgressBar("Baking Animations", $"Processing {animClip.name}", progress);

                        BakeAnimationForMesh(enemyName, animClip, meshRenderer, ref currentOffset);
                    }
                }
                else
                {
                    Debug.LogError($"No Renderer found in prefab {enemyPrefab.name}");
                }
            }

            var vertexDataPath = EditorUtility.SaveFilePanel("Save Vertex Data", "Assets", "vertex_data", "bin");
            if (!string.IsNullOrEmpty(vertexDataPath)) SerializeVertexData(_allVertices.ToArray(), vertexDataPath);

            var lookupTablePath = EditorUtility.SaveFilePanel("Save Lookup Table", "Assets", "lookup_table", "json");
            if (!string.IsNullOrEmpty(lookupTablePath)) SerializeLookupTable(_lookupTable, lookupTablePath);

            EditorUtility.ClearProgressBar();

            Debug.Log("Animations baked and serialized successfully.");
        }

        private void BakeAnimationForSkinnedMesh(string enemyName, AnimationClip animClip, SkinnedMeshRenderer skinnedMeshRenderer, ref int currentOffset)
        {
            var bakedMesh = new Mesh();
            var animator = skinnedMeshRenderer.gameObject.transform.parent.GetComponent<UnityEngine.Animator>();
            if (!animator)
            {
                animator = skinnedMeshRenderer.transform.GetComponent<UnityEngine.Animator>();
                if (!animator)
                {
                    Debug.LogError($"Failed to find an animator on skinned mesh object: {skinnedMeshRenderer.gameObject.name}");
                    return;
                }
            }

            var frameCount = Mathf.CeilToInt(animClip.length * animClip.frameRate); // Use the length and frame rate to calculate frame count
            var vertexCount = skinnedMeshRenderer.sharedMesh.vertexCount;

            for (var frame = 0; frame < frameCount; frame++)
            {
                var time = (float)frame / frameCount * animClip.length;
                animClip.SampleAnimation(animator.gameObject, time);
                skinnedMeshRenderer.BakeMesh(bakedMesh);

                for (var i = 0; i < bakedMesh.vertexCount; i++)
                    _allVertices.Add(bakedMesh.vertices[i]);
            }

            _lookupTable.AddAnimation(enemyName, animClip.name,
                new AnimationData(animClip.name, currentOffset, frameCount, vertexCount, true));

            currentOffset += frameCount * vertexCount;
        }

        private void BakeAnimationForMesh(string enemyName, AnimationClip animClip, MeshRenderer meshRenderer,
            ref int currentOffset)
        {
            var meshFilter = meshRenderer.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                Debug.LogError($"MeshFilter not found for {meshRenderer.gameObject.name}");
                return;
            }

            var mesh = meshFilter.sharedMesh;
            var frameCount = Mathf.CeilToInt(animClip.length * animClip.frameRate);
            var vertexCount = mesh.vertexCount;

            for (var frame = 0; frame < frameCount; frame++)
            {
                var time = (float) frame / frameCount * animClip.length;
                animClip.SampleAnimation(meshRenderer.gameObject, time);

                var localPosition = meshRenderer.transform.localPosition;
                var localRotation = meshRenderer.transform.localRotation;
                var localScale = meshRenderer.transform.localScale;

                for (var i = 0; i < mesh.vertexCount; i++)
                {
                    var transformedVertex = localPosition + localRotation * Vector3.Scale(mesh.vertices[i], localScale);
                    _allVertices.Add(transformedVertex);
                }
            }

            _lookupTable.AddAnimation(enemyName, animClip.name,
                new AnimationData(animClip.name, currentOffset, frameCount, vertexCount, true));
            currentOffset += frameCount * vertexCount;
        }

        private static List<float> GetKeyframeTimes(AnimationClip clip)
        {
            var keyframeTimes = new List<float>();

            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                foreach (var keyframe in curve.keys)
                {
                    keyframeTimes.Add(keyframe.time);
                }
            }

            return keyframeTimes;
        }

        private static void SerializeVertexData(Vector3[] allVertices, string path)
        {
            using var fs = new FileStream(path, FileMode.Create);
            using var writer = new BinaryWriter(fs);

            writer.Write(allVertices.Length);
            foreach (var vertex in allVertices)
            {
                writer.Write(vertex.x);
                writer.Write(vertex.y);
                writer.Write(vertex.z);
            }
        }

        private static void SerializeLookupTable(AnimationLookupTable lookupTable, string path)
        {
            var json = JsonConvert.SerializeObject(lookupTable, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}