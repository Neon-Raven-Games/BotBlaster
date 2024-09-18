using System.Collections.Generic;
using Gameplay.Enemies;
using NRTools.GpuSkinning;
using UnityEngine;
using UnityEngine.InputSystem;


public class HandCannon : MonoBehaviour
{
    public float FireRate => actor.baseAttackCoolDown;
    public ElementFlag blasterElement;
    private ElementFlag _previousElement;
    
    public AudioSource audioSource;
    private GpuMeshAnimator _gpuMeshAnimator;
    [SerializeField] private InputActionAsset actionAsset;
    public Transform barrelTransform;
    internal HandSide handSide;

    [Header("Muzzle Flash")] [SerializeField]
    private GameObject fireFlash;

    [SerializeField] private GameObject waterFlash;
    [SerializeField] private GameObject windFlash;
    [SerializeField] private GameObject rockFlash;
    [SerializeField] private GameObject electricityFlash;


    [Header("Shooting Settings")] public float launchForce = 20f;
    internal GameObject muzzleFlash;
    internal CannonState state;
    private Dictionary<CannonState, BaseHandCanonState> _states;
    private BaseHandCanonState _currentState;
    private InputAction _triggerAction;
    public DevController actor;

    [SerializeField] internal bool soloCannon;

    private void Start()
    {
        _gpuMeshAnimator = GetComponent<GpuMeshAnimator>();
        _states = new Dictionary<CannonState, BaseHandCanonState>
        {
            {CannonState.Shooting, new ShootingState(this)},
            {CannonState.Idle, new IdleState(this)}
        };

        SetBlasterMaterial();
        _currentState = _states[CannonState.Idle];
        _currentState.EnterState();
    }

    public void PlayOneShotAnimation()
    {
        _gpuMeshAnimator.PlayOneShotHitAnimation();
    }

    private void SetBlasterMaterial()
    {
        if (soloCannon) return;
        if (blasterElement == ElementFlag.Electricity) muzzleFlash = electricityFlash;
        else if (blasterElement == ElementFlag.Fire) muzzleFlash = fireFlash;
        else if (blasterElement == ElementFlag.Water) muzzleFlash = waterFlash;
        else if (blasterElement == ElementFlag.Wind) muzzleFlash = windFlash;
        else if (blasterElement == ElementFlag.Rock) muzzleFlash = rockFlash;
        else if (blasterElement == ElementFlag.None) muzzleFlash = electricityFlash;

        _gpuMeshAnimator.UpdateElement(blasterElement);
    }

    private void PopulateInput()
    {
        var hand = GetComponentInParent<VRHand>();
        if (!hand || soloCannon) return;
        handSide = hand.handSide;
        var side = hand.handSide;
        var handSideString = "Right";
        if (side == HandSide.LEFT) handSideString = "Left";
        _triggerAction = actionAsset.FindAction($"XRI {handSideString} Interaction/UI Press");
        _triggerAction.Enable();
        _triggerAction.performed += TriggerPerformedAction;
        _triggerAction.canceled += TriggerReleasedAction;
    }

    public void ChangeState(CannonState nextState)
    {
        _currentState?.ExitState();
        state = nextState;
        _currentState = _states[nextState];
        _currentState.EnterState();
    }

    public void TriggerPerformedAction(InputAction.CallbackContext obj)
    {
        ChangeState(CannonState.Shooting);
    }

    public void TriggerReleasedAction(InputAction.CallbackContext obj)
    {
        _currentState?.FireReleaseAction();
    }

    private void Update()
    {
        _states[CannonState.Shooting].Update();
    }

    private void FixedUpdate()
    {
        _currentState?.FixedUpdate();
    }

    private void OnEnable()
    {
        if (soloCannon) return;
        PopulateInput();
        if (_triggerAction == null) return;
        _triggerAction.Enable();
    }

    public void Shoot()
    {
        if (!soloCannon && handSide == HandSide.LEFT) actor.PlayLeftFeedback();
        else if (!soloCannon && handSide == HandSide.RIGHT) actor.PlayRightFeedback();
        ChangeState(CannonState.Shooting);
    }

    private void OnDisable()
    {
        if (soloCannon) return;
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

    public void FinalizeElementChange()
    {
        if ((_previousElement & blasterElement) == 0) SetBlasterMaterial();
    }

    public void InitializeElementChange()
    {
        _previousElement = blasterElement;
    }
}