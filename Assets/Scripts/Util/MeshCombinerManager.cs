using UnityEngine;
using System.Collections.Generic;

public class MeshCombinerManager : MonoBehaviour
{
    private Dictionary<string, List<MeshFilter>> meshFiltersDict = new Dictionary<string, List<MeshFilter>>();
    private Dictionary<string, Mesh> combinedMeshesDict = new Dictionary<string, Mesh>();
    private Dictionary<string, MeshFilter> combinedMeshFiltersDict = new Dictionary<string, MeshFilter>();
    private static MeshCombinerManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        foreach (var unitName in new[]
                     {"Grunt(Clone)", "GlassCannon(Clone)", "Tank(Clone)", "SwarmUnit(Clone)", "Swarm(Clone)"})
        {
            var combinedMeshFilter = new GameObject(unitName + "_Combined").AddComponent<MeshFilter>();
            combinedMeshFilter.gameObject.AddComponent<MeshRenderer>();
            combinedMeshFilter.mesh = new Mesh();

            combinedMeshFiltersDict[unitName] = combinedMeshFilter;
            combinedMeshesDict[unitName] = combinedMeshFilter.mesh;
            meshFiltersDict[unitName] = new List<MeshFilter>();
        }
    }

    public static void AddMesh(MeshFilter meshFilter, string unitType)
    {
        if (!instance.meshFiltersDict[unitType].Contains(meshFilter))
        {
            instance.meshFiltersDict[unitType].Add(meshFilter);
            instance.RebuildCombinedMesh(unitType);
        }
    }

    public static void RemoveMesh(MeshFilter meshFilter, string unitType)
    {
        if (instance.meshFiltersDict[unitType].Contains(meshFilter))
        {
            instance.meshFiltersDict[unitType].Remove(meshFilter);
            instance.RebuildCombinedMesh(unitType);
        }
    }

    private void RebuildCombinedMesh(string unitType)
    {
        CombineInstance[] combine = new CombineInstance[meshFiltersDict[unitType].Count];

        for (int i = 0; i < meshFiltersDict[unitType].Count; i++)
        {
            combine[i].mesh = meshFiltersDict[unitType][i].sharedMesh;
            combine[i].transform = meshFiltersDict[unitType][i].transform.localToWorldMatrix;
            meshFiltersDict[unitType][i].GetComponent<MeshRenderer>().enabled = false; // Disable the original objects
        }

        var combinedMesh = combinedMeshesDict[unitType];
        combinedMesh.Clear();
        combinedMesh.CombineMeshes(combine, true, true);

        var combinedMeshFilter = combinedMeshFiltersDict[unitType];
        combinedMeshFilter.mesh = combinedMesh; // Assign the combined mesh to the MeshFilter
        combinedMeshFilter.gameObject.SetActive(true);
    }

}