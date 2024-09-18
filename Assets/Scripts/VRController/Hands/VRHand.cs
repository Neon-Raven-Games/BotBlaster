using Gameplay.Enemies;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class VRHand : MonoBehaviour
{
    public HandSide handSide;
    [SerializeField] private InputActionAsset actionAsset;
    private HapticImpulsePlayer _impulsePlayer;
    private InputAction _blasterSelect;
    private HandCannon _handCannon;

    public void PlayHapticImpulse(float amplitude, float duration) =>
        _impulsePlayer.SendHapticImpulse(amplitude, duration);

    public void Awake()
    {
        if (Application.platform != RuntimePlatform.WindowsEditor)
            Application.focusChanged += OnApplicationFocusChanged;
        _impulsePlayer = GetComponent<HapticImpulsePlayer>();
        
        // var handString = handSide == HandSide.LEFT ? "Left" : "Right";
        // _blasterSelect = actionAsset.FindAction($"XRI {handString} Interaction/Select", true);
        
        // _blasterSelect.Enable();
        
        // _blasterSelect.performed += _ => InitializeChangeBlaster();
        // _blasterSelect.canceled += _ => FinalizeChangeBlaster();
        
        _handCannon = GetComponentInChildren<HandCannon>();
    }

    public void OnDestroy()
    {
        if (Application.platform != RuntimePlatform.WindowsEditor)
            Application.focusChanged -= OnApplicationFocusChanged;
    }

    private void OnApplicationFocusChanged(bool hasFocus)
    {
        gameObject.SetActive(hasFocus);
    }

    private void FinalizeChangeBlaster()
    {
        _handCannon.FinalizeElementChange();
        // selectUI.gameObject.SetActive(false);
        // uiLaserSetup.gameObject.SetActive(false);
        // change the handside blaster if valid 
    }

    private void InitializeChangeBlaster()
    {
        _handCannon.InitializeElementChange();
        // uiLaserSetup.gameObject.SetActive(true);
        // selectUI.transform.position = transform.position + transform.forward * uiSpawnDistance;
        // selectUI.transform.rotation = Quaternion.Euler(0, selectUI.transform.rotation.eulerAngles.y, 0);
        // selectUI.gameObject.SetActive(true);
    }
}