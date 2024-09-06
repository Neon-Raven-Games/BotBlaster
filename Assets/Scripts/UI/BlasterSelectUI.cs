using System;
using System.Collections.Generic;
using Gameplay.Enemies;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BlasterUIElement
{
    public ElementFlag element;
    public ElementHoverUI elementHoverUI;
}
public class BlasterSelectUI : MonoBehaviour
{
    [SerializeField] private List<BlasterUIElement> elements;
    [SerializeField] private GameObject player;
    private void Update()
    {
        var forward = player.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-forward, Vector3.up);
    }

    private void OnEnable()
    {
        foreach (var elements in elements)
        {
            var color = elements.elementHoverUI.GetComponent<Image>().color;
            if (elements.elementHoverUI.handCannon.blasterElement != elements.element)
            {
                color.a = 1f;
                elements.elementHoverUI.GetComponent<Image>().color = color;
            }
            else
            {
                color.a = 0.5f;
                elements.elementHoverUI.GetComponent<Image>().color = color;
            }
        }   
    }
}
