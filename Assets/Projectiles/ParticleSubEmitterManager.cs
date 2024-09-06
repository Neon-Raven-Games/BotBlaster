using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSubEmitterManager : MonoBehaviour
{
    public ParticleSystem[] particleSystems;

    // Reused array to store particles
    private ParticleSystem.Particle[] particles;

    // Dictionary to store particles by mesh and material combination
    private Dictionary<Mesh, Dictionary<Material, List<Matrix4x4>>> meshMaterialBatches =
        new Dictionary<Mesh, Dictionary<Material, List<Matrix4x4>>>();

    // Reuse material property block to avoid GC overhead
    private MaterialPropertyBlock materialPropertyBlock;

    void Start()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>(false);
        DisableRenderers(particleSystems);  // Disable Unity's default particle rendering
        materialPropertyBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        StoreMeshesMaterialsAndParticles(particleSystems);
        DrawParticles();
        ClearBatches();  // Clear after each frame
    }

    // Disable the built-in particle system rendering
    void DisableRenderers(ParticleSystem[] systems)
    {
        foreach (var system in systems)
        {
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;  // Disable Unity's default rendering
            }
        }
    }

    // Ensure we have a particle array with the proper size
    void EnsureParticleArrayCapacity(int capacity)
    {
        if (particles == null || particles.Length != capacity)
        {
            particles = new ParticleSystem.Particle[capacity];
        }
    }

    // Store all particle data (mesh-material pairs and transformations)
    void StoreMeshesMaterialsAndParticles(ParticleSystem[] systems)
    {
        foreach (var system in systems)
        {
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                Mesh mesh = renderer.mesh;  // Get the mesh used by this particle system

                if (mesh == null)
                {
                    mesh = GetDefaultMesh();  // Fallback in case no mesh is assigned (typically a quad)
                }

                foreach (var material in renderer.sharedMaterials)
                {
                    // Enable instancing on the material if it's not already enabled
                    if (!material.enableInstancing)
                    {
                        material.enableInstancing = true;
                    }

                    // Initialize dictionary for this mesh if needed
                    if (!meshMaterialBatches.ContainsKey(mesh))
                    {
                        meshMaterialBatches[mesh] = new Dictionary<Material, List<Matrix4x4>>();
                    }

                    // Initialize list for this mesh-material pair
                    if (!meshMaterialBatches[mesh].ContainsKey(material))
                    {
                        meshMaterialBatches[mesh][material] = new List<Matrix4x4>();
                    }
                }

                // Ensure we have enough capacity for the particles
                EnsureParticleArrayCapacity(system.main.maxParticles);

                // Get particle data
                int particleCount = system.GetParticles(particles);

                // Store transformation matrices and per-particle properties
                for (int i = 0; i < particleCount; i++)
                {
                    var particle = particles[i];

                    // Apply position, rotation, and scale based on the particle's properties
                    Vector3 finalScale = system.transform.lossyScale * particle.GetCurrentSize(system);
                    Matrix4x4 matrix = Matrix4x4.TRS(
                        particle.position,
                        Quaternion.Euler(0, 0, particle.rotation),
                        finalScale
                    );

                    // Add matrix to the corresponding mesh-material list
                    foreach (var material in renderer.sharedMaterials)
                    {
                        meshMaterialBatches[mesh][material].Add(matrix);
                    }

                    // Apply per-particle color
                    materialPropertyBlock.SetColor("_Color", particle.GetCurrentColor(system));
                }
            }
        }
    }

    // Render all particles by mesh-material pairs
    void DrawParticles()
    {
        foreach (var mesh in meshMaterialBatches.Keys)
        {
            foreach (var material in meshMaterialBatches[mesh].Keys)
            {
                var matrices = meshMaterialBatches[mesh][material];
                if (matrices.Count > 0)
                {
                    // Clear the material property block before each draw
                    materialPropertyBlock.Clear();

                    // Draw all particles for this mesh-material pair in one call
                    Graphics.DrawMeshInstanced(mesh, 0, material, matrices.ToArray(), matrices.Count, materialPropertyBlock);
                }
            }
        }
    }

    // Clear the batched data after rendering each frame
    void ClearBatches()
    {
        foreach (var mesh in meshMaterialBatches.Keys)
        {
            foreach (var material in meshMaterialBatches[mesh].Keys)
            {
                meshMaterialBatches[mesh][material].Clear();
            }
        }
    }

    // Fallback to a quad mesh if no mesh is assigned to the particle system
    Mesh GetDefaultMesh()
    {
        return Resources.GetBuiltinResource<Mesh>("Quad.fbx");  // Unity's built-in quad
    }
}