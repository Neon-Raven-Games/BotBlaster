using System.Collections;
using System.Collections.Generic;
using Gameplay.Elements;
using Gameplay.Enemies;
using Gameplay.Enemies.EnemyTypes;
using UnityEngine;

public class EnemyElementConverter : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private List<ElementMaterial> elementMaterials;
    private readonly Dictionary<ElementFlag, ElementMaterial> _elementMaterials = new();
    
    public void SwitchElement(ElementFlag elementFlag)
    {
        if (elementFlag == ElementFlag.None) return;
        meshRenderer.material = _elementMaterials[elementFlag].characterMaterial;
    }

    public void Initialize()
    {
        foreach (var mat in elementMaterials)
            _elementMaterials.Add(mat.elementFlag, mat);
    }
}
