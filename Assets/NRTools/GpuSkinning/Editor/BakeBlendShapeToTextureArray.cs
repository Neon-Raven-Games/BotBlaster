using UnityEditor;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class BakeBlendShapeToTextureArray : EditorWindow
    {
        // Fields to assign in the editor
        private SkinnedMeshRenderer skinnedMeshRenderer;
        private AnimationClip animationClip;

        [MenuItem("Neon Raven/GPU Skinning/BlendShape")]
        public static void ShowWindow()
        {
            // Show the editor window
            GetWindow<BakeBlendShapeToTextureArray>("Bake Blend Shape");
        }

        private void OnGUI()
        {
            // GUI for selecting the SkinnedMeshRenderer and AnimationClip
            skinnedMeshRenderer = (SkinnedMeshRenderer) EditorGUILayout.ObjectField("Skinned Mesh Renderer",
                skinnedMeshRenderer, typeof(SkinnedMeshRenderer), true);
            animationClip =
                (AnimationClip) EditorGUILayout.ObjectField("Animation Clip", animationClip, typeof(AnimationClip),
                    true);

            if (skinnedMeshRenderer != null && animationClip != null)
            {
                if (GUILayout.Button("Bake Blend Shape Animation"))
                {
                    // Calculate the texture resolution correctly to fit all vertex data (vertex count = pixels required)
                    int requiredPixels = skinnedMeshRenderer.sharedMesh.vertexCount;
                    int textureResolution = Mathf.CeilToInt(Mathf.Sqrt(requiredPixels));

                    Debug.Log("Calculated texture resolution: " + textureResolution);

                    BakeBlendShapeAnimation(skinnedMeshRenderer, animationClip, textureResolution);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Assign a Skinned Mesh Renderer and an Animation Clip to bake.",
                    MessageType.Info);
            }
        }

        private void BakeBlendShapeAnimation(SkinnedMeshRenderer skinnedMeshRenderer, AnimationClip animationClip,
            int textureResolution)
        {
            Mesh bakedMesh = new Mesh();
            int numFrames =
                Mathf.CeilToInt(animationClip.length * animationClip.frameRate); // Calculate the total number of frames

            Texture2DArray blendShapeTextureArray = new Texture2DArray(
                textureResolution, // Width of each texture
                textureResolution, // Height of each texture
                numFrames, // Number of frames (slices)
                TextureFormat.RGBAFloat, // Precision format for float-based deltas
                false);

            for (int frameIndex = 0; frameIndex < numFrames; frameIndex++)
            {
                // Calculate the sample time for the current frame
                float normalizedTime = (float) frameIndex / (numFrames - 1);
                float sampleTime = normalizedTime * animationClip.length;
                Debug.Log($"Frame {frameIndex}: SampleTime {sampleTime} (Normalized {normalizedTime})");

                animationClip.SampleAnimation(skinnedMeshRenderer.gameObject, sampleTime);
                skinnedMeshRenderer.BakeMesh(bakedMesh);
                BakeFrameToTexture(frameIndex, bakedMesh, blendShapeTextureArray, 2f);
            }

            blendShapeTextureArray.Apply();

            // Save the Texture2DArray asset (if needed for later use)
            AssetDatabase.CreateAsset(blendShapeTextureArray, "Assets/Art/BlendShapeAnimation.asset");
            Debug.Log("Blend shape animation baked to Assets/BlendShapeAnimation.asset");
        }

        void BakeFrameToTexture(int frameIndex, Mesh bakedMesh, Texture2DArray blendShapeTextureArray, float amplifyFactor)
        {
            Vector3[] vertices = bakedMesh.vertices;
            int vertexCount = vertices.Length;
            Color[] colors = new Color[blendShapeTextureArray.width * blendShapeTextureArray.height]; // Create color array for the texture slice

            // Pack vertex positions (XYZ) into Color[], applying an amplification factor
            for (int i = 0; i < vertexCount; i++)
            {
                    Debug.Log(vertices[i].ToString());
                int x = i % blendShapeTextureArray.width;
                int y = i / blendShapeTextureArray.width;

                if (x + y * blendShapeTextureArray.width >= colors.Length)
                {
                    Debug.LogWarning("Vertex data exceeds texture capacity.");
                    continue;
                }

                Vector3 vertex = vertices[i];
                // Map XYZ vertex positions to RGBA channels
                colors[x + y * blendShapeTextureArray.width] = new Color(
                    vertex.x * amplifyFactor, // R channel (X displacement)
                    vertex.y * amplifyFactor, // G channel (Y displacement)
                    vertex.z * amplifyFactor, // B channel (Z displacement)
                    1.0f); // A channel (unused)
            }

            Debug.Log("Setting pixel data for frame: " + frameIndex + ", texture pixel count: " + colors.Length);

            // Set the pixel data for the current frame in the texture array
            blendShapeTextureArray.SetPixels(colors, frameIndex);
        }
    }
}