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

        mesh = skinnedMeshRenderer.sharedMesh;

    }



}
