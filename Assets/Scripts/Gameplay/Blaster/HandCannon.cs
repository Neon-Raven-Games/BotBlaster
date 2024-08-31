using System;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CannonState
{
    Idle,
    Sucking,
    Shooting,
}

[Flags]
public enum ElementFlag
{
    None = 0,
    Fire = 1 << 1,
    Water = 1 << 2,
    Rock = 1 << 3,
    Wind = 1 << 4,
    Electricity = 1 << 5,
}

public class HandCannon : MonoBehaviour
{
    public ElementFlag blasterElement;
    private ElementFlag _previousElement;
    public AudioSource audioSource;
    internal readonly List<Projectile> dodgeBallAmmo = new();
    [SerializeField] private InputActionAsset actionAsset;
    public Transform barrelTransform;
    public Animator animator;
    public bool trajectoryAssist;

    [Header("Shooting Settings")] public GameObject muzzleFlash;
    public float launchForce = 20f;
    public int trajectoryPoints = 8;

    [Header("Sucking Settings")] public float suctionForce = 10f;
    public float swirlRadius = 1f;
    public float swirlSpeed = 2f;
    public float ballEndScale = 0.4f;

    private Dictionary<CannonState, BaseHandCanonState> _states;
    private BaseHandCanonState _currentState;
    private InputAction _triggerAction;
    public DevController actor;

    private void Start()
    {
        _states = new Dictionary<CannonState, BaseHandCanonState>
        {
            {CannonState.Shooting, new ShootingState(this)},
            {CannonState.Idle, new IdleState(this)}
        };
        _currentState = _states[CannonState.Idle];
        _currentState.EnterState();
    }

    private void PopulateInput()
    {
        var hand = GetComponentInParent<VRHand>().handSide;
        var handSideString = "Right";
        if (hand == HandSide.LEFT) handSideString = "Left";
        _triggerAction = actionAsset.FindAction($"XRI {handSideString} Interaction/UI Press");
        _triggerAction.performed += TriggerPerformedAction;
        _triggerAction.canceled += TriggerReleasedAction;
    }

    public void AddDodgeBall(Projectile ball)
    {
        dodgeBallAmmo.Add(ball);
    }

    public void ChangeState(CannonState state)
    {
        _currentState?.ExitState();
        _currentState = _states[state];
        _currentState.EnterState();
    }

    public void TriggerPerformedAction(InputAction.CallbackContext obj)
    {
        if (_currentState == _states[CannonState.Idle])
            _currentState?.FireAction();
    }

    private void TriggerReleasedAction(InputAction.CallbackContext obj)
    {
        _currentState?.FireReleaseAction();
    }


    private void Update()
    {
        _currentState?.Update();
    }

    private void FixedUpdate()
    {
        _currentState?.FixedUpdate();
    }

    private void OnEnable()
    {
        PopulateInput();
        _triggerAction.Enable();
    }

    public void Shoot()
    {
        ChangeState(CannonState.Shooting);
    }

    private void OnDisable()
    {
        _triggerAction.performed -= TriggerPerformedAction;
        _triggerAction.canceled -= TriggerReleasedAction;
        _triggerAction.Disable();
    }


    private void OnTriggerEnter(Collider other)
    {
        _currentState?.OnTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _currentState?.OnTriggerExit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        _currentState?.OnTriggerStay(other);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        _currentState?.OnDrawGizmos();
    }

#endif
    public void FinalizeElementChange()
    {
        if ((_previousElement & blasterElement) == 0)
        {
            // this is where we update visuals and pool flags
            Debug.Log("Changed Element from " + _previousElement + " to " + blasterElement);
        }
    }

    public void InitializeElementChange()
    {
        _previousElement = blasterElement;

    }
}