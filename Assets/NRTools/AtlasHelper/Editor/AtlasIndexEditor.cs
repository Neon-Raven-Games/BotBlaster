using System;
using System.Linq;
using Gameplay.Enemies;
using NRTools.GpuSkinning;

namespace NRTools.AtlasHelper.Editor
{
    using UnityEditor;
    using UnityEngine;

    namespace NRTools.AtlasHelper
    {
        [CustomEditor(typeof(AtlasIndex))]
        public class AtlasIndexEditor : Editor
        {
            private AtlasIndex atlasIndex;

            private void OnEnable()
            {
                atlasIndex = (AtlasIndex) target;
                var anim = atlasIndex.gameObject.GetComponent<GpuMeshAnimator>();
                if (anim) atlasIndex.enemyType = anim.enemyType;
                else atlasIndex.AtlasData.Clear();

                var AtlasMaster = FindObjectOfType<AtlasMaster>();
                if (!AtlasMaster)
                {
                    Debug.LogError("No AtlasMaster found in scene.");
                    return;
                }

                AtlasMaster.AssignInstance(AtlasMaster);
                atlasIndex.AtlasData.Clear();

                if (atlasIndex.textureType == TextureType.Bots)
                {
                    var data = AtlasMaster.atlasData.Where(
                        x =>
                            x.textureType == TextureType.Bots &&
                            x.enemyType == atlasIndex.enemyType).ToList();
                    atlasIndex.textureType = TextureType.Bots;
                    atlasIndex.AtlasData = data;
                    var rect = AtlasMaster.GetRect(atlasIndex.enemyType, ElementFlag.Electricity, out var page);
                    var rend = atlasIndex.GetComponent<Renderer>();
                    if (!rend) rend = atlasIndex.GetComponentInChildren<Renderer>();
                    if (rend) SetUVAndAtlasPage(rect, page, rend);
                }
                else
                {
                    var data = AtlasMaster.atlasData.Where(
                        x =>
                            x.textureType == atlasIndex.textureType).ToList();

                    atlasIndex.AtlasData = data;
                    var rect = AtlasMaster.GetUVRect(atlasIndex.textureType, ElementFlag.Electricity, out var page);
                    var rend = atlasIndex.GetComponent<Renderer>();
                    if (!rend) rend = atlasIndex.GetComponentInChildren<Renderer>();
                    if (rend) SetUVAndAtlasPage(rect, page, rend);
                }
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                // can we make enemy type appear in the inspector?
                atlasIndex.textureType = (TextureType) 
                    EditorGUILayout.EnumPopup("Texture Type", atlasIndex.textureType);
                if (atlasIndex.textureType == TextureType.Bots)
                    atlasIndex.enemyType = (EnemyType) EditorGUILayout.EnumPopup("Enemy Type", atlasIndex.enemyType);

                // Display basic information about the AtlasIndex
                EditorGUILayout.LabelField("Atlas Index", EditorStyles.boldLabel);

                // Display each AtlasData entry in a compact form
                if (atlasIndex.AtlasData != null && atlasIndex.AtlasData.Count > 0)
                {
                    foreach (var atlasData in atlasIndex.AtlasData)
                    {
                        // Draw a box around each item to make it clearer in the inspector
                        EditorGUILayout.BeginVertical("box");

                        if (atlasData == null)
                        {
                            Debug.Log("Atlas Data is null");
                            continue;
                        }

                        // Display Texture Type
                        EditorGUILayout.LabelField("Texture Type", atlasData.textureType.ToString(),
                            EditorStyles.label);
                        EditorGUILayout.LabelField("Element Flag", atlasData.elementFlag.ToString(),
                            EditorStyles.label);
                        EditorGUILayout.LabelField("UV Rect",
                            $"X: {atlasData.UVRect.x}, Y: {atlasData.UVRect.y}, Width: {atlasData.UVRect.width}, Height: {atlasData.UVRect.height}");

                        EditorGUILayout.Space(5);

                        if (GUILayout.Button("Apply Element"))
                        {
                            var rect = atlasIndex.GetRect(atlasData.elementFlag, out var page);
                            var rend = atlasIndex.GetComponent<Renderer>();
                            if (!rend) rend = atlasIndex.GetComponentInChildren<Renderer>();
                            if (rend) SetUVAndAtlasPage(rect, page, rend);
                        }

                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No Atlas Data assigned.", MessageType.Info);
                }

                serializedObject.ApplyModifiedProperties();
            }

            private static MaterialPropertyBlock _materialPropertyBlock;
            private static readonly int _SUVOffset = Shader.PropertyToID("_UVOffset");

            public static void SetUVAndAtlasPage(Rect uvRect, int atlasPage, Renderer renderer)
            {
                if (_materialPropertyBlock == null)
                    _materialPropertyBlock = new MaterialPropertyBlock();

                _materialPropertyBlock.Clear();

                _materialPropertyBlock.SetVector(_SUVOffset, new Vector4(
                    uvRect.x, uvRect.y, uvRect.width, uvRect.height));

                renderer.SetPropertyBlock(_materialPropertyBlock);
            }
        }
    }
}