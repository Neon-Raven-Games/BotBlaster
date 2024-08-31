using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

// extend this for your hand controllers
public class VRHand : MonoBehaviour
{
    public HandSide handSide;
    private HapticImpulsePlayer _impulsePlayer;

    public void PlayHapticImpulse(float amplitude, float duration) =>
        _impulsePlayer.SendHapticImpulse(amplitude, duration);
    
    public void Awake()
    {
        Application.focusChanged += OnApplicationFocusChanged;
        _impulsePlayer = GetComponent<HapticImpulsePlayer>();
    }

    public void OnDestroy()
    {
        Application.focusChanged -= OnApplicationFocusChanged;
    }

    private void OnApplicationFocusChanged(bool hasFocus)
    {
        gameObject.SetActive(hasFocus);
    }
}