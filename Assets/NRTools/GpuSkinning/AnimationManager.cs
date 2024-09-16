using System;
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
        [SerializeField] private Material atlasMaterial;
        private static ComputeBuffer _vertexBuffer;
        private static Vector3[] _allVertices; 
        private static AnimationLookupTable _lookupTable;
        private static readonly int _SVertices = Shader.PropertyToID("vertices");
        public static bool IsLoaded => _lookupTable != null && _allVertices != null;
        public static Action OnLoaded;

        
        // ================== Art ========================
        // fire/water
        // fire/electricity
        // water/electricity
        // blasters; fire, water, electricity, wind, rock
        // combined blasters (make abstract with a color scheme for element combinations?)
        // icons for upgrade menu
        // icons for blaster element selection
        // Boss
        // Environment
        
        // ================== Code ========================
        // beef up damage number oomph~
        // projectile collisions~
        // balance the enemies' health, damage speed~
        // enemy health bars~
        
        // fix upgrade station design, needs to be better (git gud bro)
        // generate waves on the fly instead of cached collections
        // boss wave sequencing
        // move the status defects to utilize the same as fire, remove status effects otherwise
        // test and validate updated ui functionality
        // animation polishing
        
        // ================= SoundDev ==================
        // Wave Audio Mixing
        // projectile launch
        // projectile impact
        // songs list, regular wave && boss wave
        
        // bot sounds;
        // Common: death
        // Tank: launch, tracks, hurt
        // GlassCannon: hover/zoom, charge, hurt
        // Grunt: moving, hurt, shoot
        // SwarmBot: Hover, DiveBomb queue, diving sound
        
        
        private void Awake()
        {
            StartCoroutine(DeserializeLookupTable(lookupTablePath, AssignLookupCallback));
            StartCoroutine(DeserializeVertexData(vertexDataPath, AssignVertexData));
        }

        public static AnimationData GetAnimationData(string enemyType, string animationName)
        {
            return _lookupTable.GetAnimationData(enemyType, animationName);
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
        private void OnDestroy()
        {
            _vertexBuffer?.Release();
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