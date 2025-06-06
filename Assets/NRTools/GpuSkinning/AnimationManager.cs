﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace NRTools.GpuSkinning
{
    public class AnimationManager : MonoBehaviour
    {
        [SerializeField] private string vertexDataPath = "path_to_vertex_data.bin";
        [SerializeField] private string lookupTablePath = "path_to_lookup_table.json";
        [SerializeField] public Material atlasMaterial;
        private static ComputeBuffer _vertexBuffer;
        private static Vector3[] _allVertices;
        private static AnimationLookupTable _lookupTable;
        private static readonly int _SVertices = Shader.PropertyToID("vertices");
        public static bool IsLoaded => _lookupTable != null && _allVertices != null;
        public static Action OnLoaded;
        
        private void Awake()
        {
            StartCoroutine(DeserializeLookupTable(lookupTablePath, AssignLookupCallback));
            StartCoroutine(DeserializeVertexData(vertexDataPath, AssignVertexData));
        }

        public static void EDITOR_SetLooping(string animator, string animation, bool loop)
        {
            _lookupTable.SetLoop(animator, animation, loop);
            EditorSerialize();
        }
        
        public static void EditorSerialize()
        {
            var path = Path.Combine(Application.streamingAssetsPath,  "lookup_table.json");
            var json = JsonConvert.SerializeObject(_lookupTable);
            File.WriteAllText(path, json);
            Debug.Log($"Serialized lookup, {_editorInstance}");
        }
        
        public static AnimationManager _editorInstance;
        public void EditorDeserialize()
        {
            _editorInstance = this;
            _lookupTable = null;
            _vertexBuffer?.Dispose();
            _vertexBuffer = null;
            
            DeserializeLocalLookupTable(lookupTablePath, AssignLookupCallback);
            DeserializeLocalVertexData(vertexDataPath, AssignVertexData);
        }

        private static void DeserializeLocalLookupTable(string filename, Action<AnimationLookupTable> callback)
        {
            var path = Path.Combine(Application.streamingAssetsPath, "lookup_table.json");
            var json = File.ReadAllText(path);
            var lookupTable = JsonConvert.DeserializeObject<AnimationLookupTable>(json);
            callback(lookupTable);
        }

        private static void DeserializeLocalVertexData(string filename, Action<Vector3[]> callback)
        {
            var path = Path.Combine(Application.streamingAssetsPath, "vertex_data.bin");
            var data = File.ReadAllBytes(path);

            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);
            var length = reader.ReadInt32();
            var vertices = new Vector3[length];

            for (var i = 0; i < length; i++)
            {
                var x = reader.ReadSingle();
                var y = reader.ReadSingle();
                var z = reader.ReadSingle();

                vertices[i] = new Vector3(x, y, z);
            }

            callback(vertices);
        }

        public static AnimationData GetAnimationData(string enemyType, string animationName)
        {
            if (_lookupTable == null) return null;
            return _lookupTable.GetAnimationData(enemyType, animationName);
        }
        
        public static List<string> GetAnimations(string animator)
        {
            if (_lookupTable == null) return null;
            return _lookupTable.GetAnimationFiles(animator);
        }
        
        public static List<string> GetAnimators()
        {
            if (_lookupTable == null) return null;
            return _lookupTable.GetAnimators();
        }
        
        
        private static void AssignLookupCallback(AnimationLookupTable lookupTable)
        {
            _lookupTable = lookupTable;
            if (_allVertices != null)
            {
                OnLoaded?.Invoke();
                OnLoaded = null;
            }
        }

        private void AssignVertexData(Vector3[] vertices)
        {
            _allVertices = vertices;
            _vertexBuffer = new ComputeBuffer(_allVertices.Length, sizeof(float) * 3);
            _vertexBuffer.SetData(_allVertices);
            
            atlasMaterial.SetBuffer(_SVertices, _vertexBuffer);
            
            if (_lookupTable != null)
            {
                OnLoaded?.Invoke();
                OnLoaded = null;
            }
        }

        public void OnDestroy()
        {
            ReleaseBuffer();
            OnLoaded = null;
        }

        public void ReleaseBuffer()
        {
            if (_vertexBuffer == null) return;
            _vertexBuffer.Release();
            _vertexBuffer = null;
        }
        
        private static IEnumerator DeserializeLookupTable(string fileName, Action<AnimationLookupTable> callback)
        {
            var path = Path.Combine(Application.streamingAssetsPath, fileName);
            var request = UnityWebRequest.Get(path);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load lookup table: {request.error}");
                callback(null);
            }
            else
            {
                var json = request.downloadHandler.text;
                var lookupTable = JsonConvert.DeserializeObject<AnimationLookupTable>(json);
                callback(lookupTable);
            }
        }

        private static IEnumerator DeserializeVertexData(string filename, Action<Vector3[]> callback)
        {
            var path = Path.Combine(Application.streamingAssetsPath, filename);
            var request = UnityWebRequest.Get(path);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load vertex data: {request.error}");
                callback(null);
            }
            else
            {
                var data = request.downloadHandler.data;

                using var ms = new MemoryStream(data);
                using var reader = new BinaryReader(ms);
                var length = reader.ReadInt32();
                var vertices = new Vector3[length];

                for (var i = 0; i < length; i++)
                {
                    var x = reader.ReadSingle();
                    var y = reader.ReadSingle();
                    var z = reader.ReadSingle();

                    vertices[i] = new Vector3(x, y, z);
                }

                callback(vertices);
            }
        }

    }
}