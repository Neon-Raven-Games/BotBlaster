using UnityEngine;
using UnityEngine.InputSystem;

public class DualCannonShooter : MonoBehaviour
{
    [SerializeField] private float handSpreadDistance;
    [SerializeField] private HandCannon leftCannon;
    [SerializeField] private Transform leftHandTracker;
    
    [SerializeField] private HandCannon rightCannon;
    [SerializeField] private Transform rightHandTracker;

    [SerializeField] private HandCannon bigCannon;
    [SerializeField] private InputActionAsset actionAsset;
    
    private VRHand _leftHand;
    private VRHand _rightHand;
    public bool _merged;
    private InputAction _leftTrigger;
    private InputAction _rightTrigger;
    
    private void Awake()
    {
        _leftTrigger = actionAsset.FindAction("XRI Left Interaction/UI Press", true);
        _rightTrigger = actionAsset.FindAction("XRI Right Interaction/UI Press", true);

        _leftTrigger.Disable();
        _rightTrigger.Disable();

        _leftHand = leftHandTracker.GetComponent<VRHand>();
        _rightHand = rightHandTracker.GetComponent<VRHand>();
    }

    private void Update()
    {
        var handDistance = Vector3.Distance(_leftHand.transform.position, _rightHand.transform.position);
        if (handDistance < handSpreadDistance) CombinedCannon();
        else SeparateCannon();
    }

    private void SeparateCannon()
    {
        if (!_merged) return;
        _merged = false;
        
        _rightTrigger.performed -= bigCannon.TriggerPerformedAction;
        _leftTrigger.performed -= bigCannon.TriggerPerformedAction;
        _rightTrigger.canceled -= bigCannon.TriggerReleasedAction;
        _leftTrigger.canceled -= bigCannon.TriggerReleasedAction;
        
        _rightTrigger.Disable();
        _leftTrigger.Disable();
        
        bigCannon.TriggerReleasedAction(default);
        SetCannonsActive(true);
    }

    private void CombinedCannon()
    {
        if (!_merged)
        {
            _merged = true;
            SetCannonsActive(false);
            
            bigCannon.blasterElement = leftCannon.blasterElement | rightCannon.blasterElement;
            _rightTrigger.Enable();
            _leftTrigger.Enable();
            _rightTrigger.performed += bigCannon.TriggerPerformedAction;
            _leftTrigger.performed += bigCannon.TriggerPerformedAction;
            
            _rightTrigger.canceled += bigCannon.TriggerReleasedAction;
            _leftTrigger.canceled += bigCannon.TriggerReleasedAction;
        }
        
        var combinedPosition = (_leftHand.transform.position + _rightHand.transform.position) / 2;
        bigCannon.transform.position = combinedPosition;
        
        var averageDirection = (_leftHand.transform.forward + _rightHand.transform.forward).normalized;
        bigCannon.transform.rotation = Quaternion.LookRotation(averageDirection);
    }

    
    private void SetCannonsActive(bool active)
    {
        _leftHand.enabled = active;
        _rightHand.enabled = active;
        leftCannon.gameObject.SetActive(active);
        rightCannon.gameObject.SetActive(active);
        bigCannon.gameObject.SetActive(!active);
    }
}
