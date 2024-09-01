using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.TextCore.LowLevel;

public class ExportFontAtlas : MonoBehaviour
{
    [MenuItem("Assets/Export Font Atlas")]
    public static void ExportFont()
    {
        // Select the font asset
        TMP_FontAsset fontAsset = Selection.activeObject as TMP_FontAsset;
        
        if (fontAsset != null && fontAsset.atlasTexture != null)
        {
            // Create a path to save the texture
            string path = EditorUtility.SaveFilePanel("Save Font Atlas", "", fontAsset.name + "_Atlas.png", "png");
            
            if (!string.IsNullOrEmpty(path))
            {
                // Get the texture
                Texture2D texture = fontAsset.atlasTexture;
                
                // Encode texture into PNG
                byte[] bytes = texture.EncodeToPNG();
                
                // Save the PNG file
                System.IO.File.WriteAllBytes(path, bytes);
                Debug.Log("Saved texture to: " + path);
                
                // Refresh the asset database to show the new file
                AssetDatabase.Refresh();
            }
        }
        else
        {
            Debug.LogError("No font asset selected or atlas texture not found!");
        }
    }
    [MenuItem("Assets/Replace Font Atlas")]
    public static void ReplaceFontAtlas()
    {
        // Select the font asset
        TMP_FontAsset fontAsset = Selection.activeObject as TMP_FontAsset;

        if (fontAsset != null)
        {
            // Select the new texture
            string path = EditorUtility.OpenFilePanel("Select Font Atlas Texture", "", "png");

            if (!string.IsNullOrEmpty(path))
            {
                
                byte[] fileData = System.IO.File.ReadAllBytes(path);
                Texture2D newTexture = new Texture2D(fontAsset.atlasWidth, fontAsset.atlasHeight, TextureFormat.Alpha8, false);
                newTexture.LoadImage(fileData);
                newTexture.Apply();

                fontAsset.atlas = newTexture;
       

                EditorUtility.SetDirty(fontAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("Assigned new font atlas texture.");
            }
        }
        else
        {
            Debug.LogError("No font asset selected!");
        }
    }
}