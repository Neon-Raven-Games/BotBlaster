using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class VRHand : MonoBehaviour
{
    public HandSide handSide;
    [SerializeField] private InputActionAsset actionAsset;
    [SerializeField] private VRUILaserSetup uiLaserSetup;
    [SerializeField] private GameObject selectUI;
    [SerializeField] private float uiSpawnDistance = 1f;   
    private HapticImpulsePlayer _impulsePlayer;
    private InputAction _blasterSelect;

    public void PlayHapticImpulse(float amplitude, float duration) =>
        _impulsePlayer.SendHapticImpulse(amplitude, duration);

    public void Awake()
    {
        if (Application.isPlaying) Application.focusChanged += OnApplicationFocusChanged;
        _impulsePlayer = GetComponent<HapticImpulsePlayer>();
        
        var handString = handSide == HandSide.LEFT ? "Left" : "Right";
        _blasterSelect = actionAsset.FindAction($"XRI {handString} Interaction/Scale Toggle", true);
        
        _blasterSelect.Enable();
        
        _blasterSelect.performed += _ => InitializeChangeBlaster();
        _blasterSelect.canceled += _ => FinalizeChangeBlaster();
    }

    public void OnDestroy()
    {
        if (Application.isPlaying) Application.focusChanged -= OnApplicationFocusChanged;
    }

    private void OnApplicationFocusChanged(bool hasFocus)
    {
        gameObject.SetActive(hasFocus);
    }

    private void FinalizeChangeBlaster()
    {
        // change the handside blaster if valid 
    }

    private void InitializeChangeBlaster()
    {
        uiLaserSetup.gameObject.SetActive(true);
        selectUI.transform.position = transform.position + transform.forward * uiSpawnDistance;
        selectUI.SetActive(true);
    }
}