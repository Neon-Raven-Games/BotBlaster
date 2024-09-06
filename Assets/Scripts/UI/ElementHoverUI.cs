using System;
using Gameplay.Enemies;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElementHoverUI : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] internal ElementFlag elementFlag;
    [SerializeField] internal HandCannon handCannon;
    private Image _iconImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        handCannon.blasterElement = elementFlag;
        var color = _iconImage.color;
        color.a = 0.5f;
        _iconImage.color = color;
    }
    
    private void Start()
    {
        _iconImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (handCannon.blasterElement != elementFlag && _iconImage.color.a != 1f)
        {
            var color = _iconImage.color;
            color.a = 1f;
            _iconImage.color = color;
        }
    }
}
