using Gameplay.Enemies;
using TMPro;
using UI.DamageNumbers;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    private Transform _mainCamera;
    private TextMeshPro _text;

    public void SetElementText(ElementFlag elementFlag, StatusEffectiveness statusEffectiveness, int number)
    {
        ElementTextTween.TweenElementText(_text, elementFlag, statusEffectiveness, number);
    }

    public void ClearText()
    {
        _text.text = "";
    }

    private void Awake()
    {
        if (Camera.main != null) 
            _mainCamera = Camera.main.transform;
        _text = GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        var forward = _mainCamera.position - transform.position;
        if (forward != Vector3.zero) transform.rotation = Quaternion.LookRotation(-forward, Vector3.up);
    }
}