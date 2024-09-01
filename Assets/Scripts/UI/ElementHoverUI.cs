using Gameplay.Enemies;
using UnityEngine;
using UnityEngine.EventSystems;

public class ElementHoverUI : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] internal ElementFlag elementFlag;
    [SerializeField] private HandCannon handCannon;

    public void OnPointerEnter(PointerEventData eventData)
    {
        handCannon.blasterElement = elementFlag;
    }
}
