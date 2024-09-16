using System;
using System.Collections.Generic;
using System.IO;
using Gameplay.Enemies;
using NRTools.AtlasHelper;
using UnityEditor;
using UnityEngine;


public class AtlasFolder : EditorWindow
{
    public AtlasData[] atlasDataArray;
    public SerializedObject[] serializedAtlasDataArray;

    private Rect[] assetsRects;
    public string finalPath = "Assets/Art/Atlas";
    Vector2 _scrollPos;

    [MenuItem("Neon Raven/Atlas")]
    public static void OpenWindow()
    {
        GetWindow<AtlasFolder>("Atlas Helper").Show();
    }

    private string[] textureGUIDs;

    private AtlasData ConvertToSO(AtlasRuntimeData data)
    {
        var so = CreateInstance<AtlasData>();
        so.textureType = data.textureType;
        so.enemyType = data.enemyType;
        so.elementFlag = data.elementFlag;
        so.sceneName = data.sceneName;
        so.UVRect = data.UVRect;
        so.AtlasPage = data.AtlasPage;
        return so;
    }

    private void OnGUI()
    {
        if (textureGUIDs == null) textureGUIDs = AssetDatabase.FindAssets("t:Texture2D", new[] {"Assets/Art/Textures"});

        if (atlasDataArray == null || atlasDataArray.Length != textureGUIDs.Length ||
            serializedAtlasDataArray == null ||
            serializedAtlasDataArray.Length != textureGUIDs.Length)
        {
            atlasDataArray = new AtlasData[textureGUIDs.Length];
            serializedAtlasDataArray = new SerializedObject[textureGUIDs.Length];

            var deserializedData = DeserializeAtlasData();

            for (var i = 0; i < textureGUIDs.Length; i++)
            {
                var guid = textureGUIDs[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (asset == null) continue;

                // Use deserialized data if available
                if (deserializedData != null && deserializedData.Count > i && deserializedData[i] != null)
                {
                    atlasDataArray[i] = ConvertToSO(deserializedData[i]);
                }

                if (path.Contains("Bots"))
                {
                    atlasDataArray[i].textureType = TextureType.Bots;

                    if (path.Contains("Tank")) atlasDataArray[i].enemyType = EnemyType.Tank;
                    else if (path.Contains("GlassCannon")) atlasDataArray[i].enemyType = EnemyType.GlassCannon;
                    else if (path.Contains("Swarm")) atlasDataArray[i].enemyType = EnemyType.Swarm;
                    else if (path.Contains("Grunt")) atlasDataArray[i].enemyType = EnemyType.Grunt;
                }
                else if (path.Contains("Blaster"))
                {
                    atlasDataArray[i].textureType = TextureType.Blaster;
                }
                else if (path.Contains("Environment"))
                {
                    atlasDataArray[i].textureType = TextureType.Environment;
                }

                if (path.Contains("Fire")) atlasDataArray[i].elementFlag = ElementFlag.Fire;
                else if (path.Contains("Elect")) atlasDataArray[i].elementFlag = ElementFlag.Electricity;
                else if (path.Contains("Rock")) atlasDataArray[i].elementFlag = ElementFlag.Rock;
                else if (path.Contains("Wind")) atlasDataArray[i].elementFlag = ElementFlag.Wind;
                else if (path.Contains("Water")) atlasDataArray[i].elementFlag = ElementFlag.Water;

                // Create SerializedObject and update
                serializedAtlasDataArray[i] = new SerializedObject(atlasDataArray[i]);
                serializedAtlasDataArray[i].Update();

                // Ensure UVRect and textureType are properly applied
                if (atlasDataArray[i] != null)
                {
                    var uvRectProperty = serializedAtlasDataArray[i].FindProperty("UVRect");
                    uvRectProperty.rectValue = atlasDataArray[i].UVRect; // Assuming UVRect is a Rect field in AtlasData

                    var textureTypeProperty = serializedAtlasDataArray[i].FindProperty("textureType");
                    textureTypeProperty.enumValueIndex =
                        (int) atlasDataArray[i].textureType; // Assuming textureType is an enum
                }

                // Apply changes
                serializedAtlasDataArray[i].ApplyModifiedProperties();
            }
        }

        // Layout for rendering
        var itemWidth = 260;
        var itemHeight = 220;
        var minPadding = 10;
        var windowWidth = (int) position.width - minPadding * 2 - 15;
        var availableWidth = windowWidth - (2 * minPadding);

        var itemsPerRow = Mathf.FloorToInt(availableWidth / (itemWidth + minPadding));
        if (itemsPerRow < 1) itemsPerRow = 1;

        var totalItemWidth = itemsPerRow * itemWidth + (itemsPerRow - 1) * minPadding;
        var remainingWidth = availableWidth - totalItemWidth;
        var padding = minPadding + (remainingWidth / (itemsPerRow + 1));

        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(position.height - 50),
            GUILayout.Width(position.width));
        GUILayout.BeginVertical();

        for (var i = 0; i < textureGUIDs.Length; i += itemsPerRow)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(padding);

            for (var j = i; j < i + itemsPerRow && j < textureGUIDs.Length; j++)
            {
                GUILayout.BeginVertical("box", GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));

                var guid = textureGUIDs[j];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (asset == null) continue;

                GUILayout.Label(AssetPreview.GetAssetPreview(asset), GUILayout.Width(128), GUILayout.Height(128));

                serializedAtlasDataArray[j].Update();
                var iterator = serializedAtlasDataArray[j].GetIterator();
                iterator.NextVisible(true);
                while (iterator.NextVisible(false))
                    EditorGUILayout.PropertyField(iterator, true);

                serializedAtlasDataArray[j].ApplyModifiedProperties();

                GUILayout.EndVertical();
                GUILayout.Space(padding);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();

        GUILayout.BeginVertical();

        if (GUILayout.Button("Create Atlas"))
        {
            GenerateAtlas();
            Debug.Log("Atlas created!");
            SerializeAtlasData();
            atlasDataArray = null;
        }

        if (GUILayout.Button("Serialize Atlas Data"))
        {
            SerializeAtlasData();
        }

        GUILayout.EndVertical();
    }


    private void SerializeAtlasData()
    {
        var AtlasMaster = FindObjectOfType<AtlasMaster>();
        if (!AtlasMaster)
        {
            Debug.LogError("No AtlasMaster found in scene.");
            return;
        }

        AtlasMaster.AssignInstance(AtlasMaster);
        AtlasMaster.atlasData.Clear();
        for (int i = 0; i < atlasDataArray.Length; i++)
        {
            var idx = atlasDataArray[i];
            atlasDataArray[i].UVRect = atlasUVRects[idx.AtlasPage][i];
            var runtimeData = new AtlasRuntimeData
            {
                UVRect = idx.UVRect,
                AtlasPage = idx.AtlasPage, // Store atlas page
                textureType = idx.textureType,
                elementFlag = idx.elementFlag,
                enemyType = idx.enemyType,
                sceneName = idx.sceneName
            };
            AtlasMaster.AddData(runtimeData);
        }

        Debug.Log("Serialized Atlas Data to JSON.");
    }


    private List<AtlasRuntimeData> DeserializeAtlasData()
    {
        var AtlasMaster = FindObjectOfType<AtlasMaster>();
        if (!AtlasMaster)
        {
            Debug.LogError("No AtlasMaster found in scene.");
            return null;
        }

        return AtlasMaster.atlasData;
    }

    private const int MaxAtlasSize = 4096; // Example maximum size, check your hardware's max size

    private List<Texture2D> generatedAtlases = new List<Texture2D>();
    private List<Rect[]> atlasUVRects = new List<Rect[]>();

    [Serializable]
    public class AtlasDataWrapper
    {
        public AtlasData[] data;
    }

    private int NextPowerOfTwo(int value)
    {
        return Mathf.NextPowerOfTwo(value);
    }

    private void GenerateAtlas()
    {
        var textureGUIDs = AssetDatabase.FindAssets("t:Texture2D", new[] {"Assets/Art/Textures"});
        var textures = new List<Texture2D>();

        foreach (var guid in textureGUIDs)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture != null)
            {
                textures.Add(texture);
            }
        }

        int currentAtlasIndex = 0;
        int atlasWidth = MaxAtlasSize;
        int atlasHeight = MaxAtlasSize;

        while (textures.Count > 0)
        {
            var atlas = new Texture2D(atlasWidth, atlasHeight, TextureFormat.RGBA32, false);
            Rect[] uvRects = atlas.PackTextures(textures.ToArray(), 2, MaxAtlasSize);

            int packedCount = uvRects.Length;
            textures.RemoveRange(0, packedCount);

            generatedAtlases.Add(atlas);
            atlasUVRects.Add(uvRects);

            // Save the atlas as a PNG file
            string atlasPath = $"{finalPath}/Atlas_{currentAtlasIndex}.png";
            File.WriteAllBytes(atlasPath, atlas.EncodeToPNG());
            Debug.Log($"Saved atlas {currentAtlasIndex} with size: {atlasWidth}x{atlasHeight}");

            currentAtlasIndex++;
        }
    }

    private void EnableAlphaIsTransparency(Texture2D texture)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

        if (textureImporter != null && !textureImporter.alphaIsTransparency)
        {
            textureImporter.alphaIsTransparency = true;
            textureImporter.ignoreMipmapLimit = true; // Disable mipmaps to avoid extra computation
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }

    Rect GetTrimmedRect(Texture2D texture)
    {
        int minX = texture.width, minY = texture.height, maxX = 0, maxY = 0;
        Color[] pixels = texture.GetPixels();

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Color pixel = pixels[x + y * texture.width];
                if (pixel.a > 0) // Consider only non-transparent pixels
                {
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }
        }

        if (minX > maxX || minY > maxY)
        {
            // If no non-transparent pixels are found, return a zero-sized rectangle
            return new Rect(0, 0, 0, 0);
        }

        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    Texture2D GetTrimmedTexture(Texture2D original, Rect trimmedRect)
    {
        if (trimmedRect.width == 0 || trimmedRect.height == 0)
            return new Texture2D(1, 1);

        Texture2D trimmedTexture = new Texture2D((int) trimmedRect.width, (int) trimmedRect.height);
        Color[] trimmedPixels = original.GetPixels((int) trimmedRect.x, (int) trimmedRect.y, (int) trimmedRect.width,
            (int) trimmedRect.height);
        trimmedTexture.SetPixels(trimmedPixels);
        trimmedTexture.Apply();
        return trimmedTexture;
    }

    private Vector2Int CalculateAtlasGrid(int imageCount)
    {
        int columns = (int) Math.Ceiling(Math.Sqrt(imageCount));

        // Calculate the number of rows (height) based on the total image count and the number of columns
        int rows = (int) Math.Ceiling((float) imageCount / columns);

        return new Vector2Int(columns, rows);
    }

    private Vector2Int CalculateAtlasSize(int unitSize, int imageCount)
    {
        // Get the grid size as columns (width) and rows (height)
        Vector2Int gridSize = CalculateAtlasGrid(imageCount);
        Debug.Log(gridSize.ToString());
        int atlasWidth = gridSize.x * unitSize;
        int atlasHeight = gridSize.y * unitSize;

        // Return the maximum size required to fit all textures
        return new Vector2Int(atlasWidth, atlasHeight);
    }
}