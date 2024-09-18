using System;
using Gameplay.Enemies;
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
    [SerializeField] private float lerpSpeed = 0.5f;
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

    // todo, fixed merging cannons.
    private void Update()
    {
        var handDistance = Vector3.Distance(_leftHand.transform.position, _rightHand.transform.position);
        if (leftCannon.blasterElement != ElementFlag.None 
            && rightCannon.blasterElement != ElementFlag.None 
            && handDistance < handSpreadDistance) 
            CombinedCannon();
        else 
            SeparateCannon();
        
        if (_merged) return;
        
        var targetOffset = leftCannon.FireRate / 2;

        if (leftCannon.fireTime >= rightCannon.fireTime)
        {
            leftCannon.fireTime = Mathf.Lerp(leftCannon.fireTime, rightCannon.fireTime + targetOffset,
                Time.deltaTime * lerpSpeed);
        }
        else if (rightCannon.fireTime > leftCannon.fireTime)
        {
            rightCannon.fireTime = Mathf.Lerp(rightCannon.fireTime, leftCannon.fireTime + targetOffset,
                Time.deltaTime * lerpSpeed);
        }
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