using System;
using UnityEngine;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using Gameplay.Elements;
using Gameplay.Enemies; // For Mesh Baker

[Serializable]
public class ElementUVOffset
{
    public ElementFlag elementFlag;
    public Vector4 uvOffset;
}

[Serializable]
public class EnemyUVOffsets
{
    public EnemyType enemyType;
    public ElementUVOffset[] elementUVOffsets;
}

public class TextureAtlasManager : MonoBehaviour
{
    [SerializeField] private Material atlasMaterial;
    public MB3_TextureBaker textureBaker;
    public Dictionary<EnemyType, Dictionary<ElementFlag, Vector4>> enemyUVOffsets = new();
    [SerializeField] private EnemyMaterialHandler enemyMaterialHandler;

    private static TextureAtlasManager _instance;

    public static Vector4 GetUVOffset(EnemyType enemyType, ElementFlag elementFlag) =>
        _instance.enemyUVOffsets[enemyType][elementFlag];

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    public static Material GetAtlasMaterial() => _instance.atlasMaterial;


    // Generate the texture atlas using Mesh Baker and store UV offsets
    // Generate a texture atlas for all element textures
    public static void GenerateTextureAtlas()
    {
        // Prepare list of GameObjects for Mesh Baker
        List<GameObject> objectsToBake = new List<GameObject>();

        // Create temporary GameObjects for each enemy's element texture
        foreach (var enemyTextures in _instance.enemyMaterialHandler.enemyElementTextures)
        {
            foreach (var elementTexture in enemyTextures.elementMaterials)
            {
                GameObject dummy = new GameObject($"{enemyTextures.enemyType}_{elementTexture.elementFlag}");
                Renderer renderer = dummy.AddComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.mainTexture = elementTexture.elementTexture;
                objectsToBake.Add(dummy);
            }
        }

        // Assign objects to Mesh Baker
        _instance.textureBaker.GetObjectsToCombine().Clear();
        _instance.textureBaker.GetObjectsToCombine().AddRange(objectsToBake);

        // Bake the textures into an atlas
        _instance.textureBaker.CreateAtlases();
        _instance.atlasMaterial = _instance.textureBaker.textureBakeResults.resultMaterials[0].combinedMaterial;
        _instance.StoreUVOffsets(_instance.textureBaker.textureBakeResults);

        // Cleanup the temporary objects
        foreach (var obj in objectsToBake)
        {
            Destroy(obj);
        }

        Debug.Log("Texture atlas generated and UV offsets stored.");
    }

    // Store UV offsets from Mesh Baker's texture bake results
    private void StoreUVOffsets(MB2_TextureBakeResults textureBakeResults)
    {
        foreach (var enemyTextures in enemyMaterialHandler.enemyElementTextures)
        {
            Dictionary<ElementFlag, Vector4> uvOffsets = new Dictionary<ElementFlag, Vector4>();

            foreach (var elementTexture in enemyTextures.elementMaterials)
            {
                Rect uvRect = new Rect();

                foreach (var matAndUV in textureBakeResults.materialsAndUVRects)
                {
                    if (matAndUV.material.mainTexture == elementTexture.elementTexture)
                    {
                        uvRect = matAndUV.atlasRect;
                        break;
                    }
                }

                if (uvRect != Rect.zero)
                {
                    // Convert UV rect to Vector4 (xMin, yMin, xMax, yMax)
                    Vector4 uvOffset = new Vector4(uvRect.xMin, uvRect.yMin, uvRect.xMax, uvRect.yMax);
                    uvOffsets[elementTexture.elementFlag] = uvOffset;
                    Debug.Log(
                        $"UV offset stored for {enemyTextures.enemyType} - {elementTexture.elementFlag}: {uvOffset}");
                }
                else
                {
                    Debug.LogWarning($"No UV rect found for {elementTexture.elementFlag}");
                }
            }

            // Store UV offsets for this enemy type
            enemyUVOffsets[enemyTextures.enemyType] = uvOffsets;
        }
    }

    // Apply the atlas and UV offset to the enemy's renderer
    public static void ApplyElement(Enemy enemy, ElementFlag elementFlag)
    {
        if (_instance == null)
        {
            Debug.LogError("EnemyMaterialHandler instance is not initialized.");
            return;
        }

        if (!_instance.enemyUVOffsets.TryGetValue(enemy.enemyType, out var uvOffsets) ||
            !uvOffsets.TryGetValue(elementFlag, out var uvOffset))
        {
            Debug.LogWarning($"No UV offset found for {enemy.enemyType} with element {elementFlag}");
            return;
        }

        // Apply the atlas material and UV offset
        var renderer = enemy.GetComponent<Renderer>() ?? enemy.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material = _instance.atlasMaterial;
            renderer.material.SetVector("_UVOffset", uvOffset); // Assuming the shader supports UV offsets
            Debug.Log($"Applied atlas and UV offset for {enemy.name} with {elementFlag}");
        }
    }

    // Apply atlas and UV offset to regular MeshRenderer
    public static void ApplyAtlasToRegularRenderer(Enemy enemy, ElementFlag elementFlag)
    {
        if (!_instance.enemyUVOffsets.ContainsKey(enemy.enemyType))
        {
            Debug.LogWarning($"UV offset for {enemy.enemyType} not found.");
            return;
        }

        Vector4 uvOffset = _instance.enemyUVOffsets[enemy.enemyType][elementFlag];

        // Check if the enemy has a regular MeshRenderer
        Renderer renderer = enemy.GetComponent<Renderer>();
        if (!renderer) renderer = enemy.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material = _instance.atlasMaterial;
            renderer.material.SetVector("_UVOffset", uvOffset);

            Debug.Log($"Applied atlas and UV offset for {elementFlag} to {enemy.name}");
        }
    }

    // Apply the UV offset and atlas to a SkinnedMeshRenderer
    public static void ApplyAtlasToSkinnedRenderer(Enemy enemy, ElementFlag elementFlag)
    {
        if (!_instance.enemyUVOffsets.ContainsKey(enemy.enemyType))
        {
            Debug.LogWarning($"UV offset for {enemy.enemyType} not found.");
            return;
        }

        Vector4 uvOffset = _instance.enemyUVOffsets[enemy.enemyType][elementFlag];

        // Check if the enemy has a SkinnedMeshRenderer
        SkinnedMeshRenderer skinnedRenderer = enemy.GetComponent<SkinnedMeshRenderer>();
        if (!skinnedRenderer) skinnedRenderer = enemy.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedRenderer != null)
        {
            skinnedRenderer.material = _instance.atlasMaterial;
            skinnedRenderer.material.SetVector("_UVOffset", uvOffset);

            Debug.Log($"Applied atlas and UV offset for {elementFlag} to {enemy.name} (Skinned)");
        }
    }
}