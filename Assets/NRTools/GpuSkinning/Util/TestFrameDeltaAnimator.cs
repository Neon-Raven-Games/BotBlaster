using System.Collections;
using NRTools.GpuSkinning;
using UnityEngine;

public class TestFrameDeltaAnimator : MonoBehaviour
{
    // Assign this manually in the editor or dynamically at runtime
    public GpuMeshAnimator gpuMeshAnimator;  // Reference to the GpuMeshAnimator script
    private SkinnedMeshRenderer skinnedMeshRenderer;  // SkinnedMeshRenderer component on the GameObject
    private Mesh mesh;  // The mesh to modify
    private int currentFrame = 0;
    private float animationSpeed = 1.0f;  // Speed to step through frames
    private int numFrames;

    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (gpuMeshAnimator == null || skinnedMeshRenderer == null)
        {
            Debug.LogError("GpuMeshAnimator or SkinnedMeshRenderer not assigned!");
            return;
        }

        // Get the mesh from the SkinnedMeshRenderer
        mesh = skinnedMeshRenderer.sharedMesh;
        numFrames = gpuMeshAnimator.animationData.frameDeltas.Count;

        // Start the frame stepping process
        StartCoroutine(StepThroughFrames());
    }

    IEnumerator StepThroughFrames()
    {
        while (true)
        {
            // Apply the deltas for the current frame
            ApplyFrameDeltas(currentFrame);

            // Increment the frame, wrap around when reaching the end
            currentFrame = (currentFrame + 1) % numFrames;

            // Wait for the next frame based on animation speed
            yield return new WaitForSeconds(1.0f / animationSpeed);
        }
    }

    private void ApplyFrameDeltas(int frameIndex)
    {
        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var tangents = mesh.tangents;

        // Get the morph deltas for this frame
        var frameDeltas = gpuMeshAnimator.animationData.frameDeltas[frameIndex];

        // Apply the deltas to the vertices, normals, and tangents
        for (int i = 0; i < frameDeltas.Count; i++)
        {
            vertices[i] = frameDeltas[i].position;
            normals[i] += (Vector3)frameDeltas[i].normal;
            tangents[i] += frameDeltas[i].tangent;
        }

        // Update the mesh with the modified vertices, normals, and tangents
        mesh.vertices = vertices;
        mesh.normals = normals;
        // mesh.tangents = tangents;

        // Apply the modified mesh back to the SkinnedMeshRenderer
        skinnedMeshRenderer.sharedMesh = mesh;

        // Log the frame number for debugging
        Debug.Log($"Applied frame {frameIndex}");
    }
}
