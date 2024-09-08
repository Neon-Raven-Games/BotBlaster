using Gameplay.Enemies;

namespace NRTools.AtlasHelper.Editor
{
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NRTools.AtlasHelper
{
    [CustomEditor(typeof(AtlasIndex))]
    public class AtlasIndexEditor : Editor
    {
        // Store a reference to the AtlasIndex script
        private AtlasIndex atlasIndex;

        // Called when the inspector is loaded
        private void OnEnable()
        {
            atlasIndex = (AtlasIndex)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

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
                    EditorGUILayout.LabelField("Texture Type", atlasData.textureType.ToString(), EditorStyles.label);
                    EditorGUILayout.LabelField("Element Flag", atlasData.elementFlag.ToString(), EditorStyles.label);
                    EditorGUILayout.LabelField("UV Rect", $"X: {atlasData.UVRect.x}, Y: {atlasData.UVRect.y}, Width: {atlasData.UVRect.width}, Height: {atlasData.UVRect.height}");

                    EditorGUILayout.Space(5);

                    if (GUILayout.Button("Apply Element"))
                    {
                        atlasIndex.Awake();
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
                uvRect.x , uvRect.y, uvRect.width, uvRect.height));

            renderer.SetPropertyBlock(_materialPropertyBlock);
        }
    }
}

}