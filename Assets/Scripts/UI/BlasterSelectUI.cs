using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class BlasterUIElement
{
    public ElementHoverUI elementHoverUI;
    public TextMeshProUGUI text;

    public void SetElement(ElementFlag element)
    {
        elementHoverUI.elementFlag = element;
        text.text = element.ToString();
    }
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

    public void SetElements(ElementFlag mask)
    {
        var i = 0;
        foreach (var flag in Enum.GetValues(typeof(ElementFlag)))
        {
            var element = (ElementFlag) flag;
            if (mask.HasFlag(element)) continue;
            if (i >= elements.Count)
            {
                Debug.LogError("Flags exceed element texts.");
                break;
            }
            
            var elementText = elements[i++];
            elementText.SetElement(element);
        }
    }
}
