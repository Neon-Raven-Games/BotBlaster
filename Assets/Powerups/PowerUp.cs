using System;
using Gameplay.Enemies;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public ElementFlag element;
    public event Action<ElementFlag, PowerUp> OnPowerUpCollected;

    public ElementFlag GetElement()
    {
        OnPowerUpCollected?.Invoke(element, this);
        gameObject.SetActive(false);
        return element;
    }
}