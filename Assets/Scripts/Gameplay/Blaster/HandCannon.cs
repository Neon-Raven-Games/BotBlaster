using System.Collections.Generic;
using Gameplay;
using Gameplay.Enemies;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;



public class HandCannon : MonoBehaviour
{
    public ElementFlag blasterElement;
    public List<BlasterElementMaterial> blasterElementMaterials;
    [SerializeField] private SkinnedMeshRenderer blasterRenderer;
    private Dictionary<ElementFlag, Material> _elementMaterials;
    private ElementFlag _previousElement;
    public AudioSource audioSource;
    [SerializeField] private InputActionAsset actionAsset;
    public Transform barrelTransform;
    public Animator animator;

    [Header("Shooting Settings")] 
    public GameObject muzzleFlash;
    public float launchForce = 20f;
    
    private Dictionary<CannonState, BaseHandCanonState> _states;
    private BaseHandCanonState _currentState;
    private InputAction _triggerAction;
    public DevController actor;

    [SerializeField] internal bool soloCannon;
    private void Start()
    {
        _states = new Dictionary<CannonState, BaseHandCanonState>
        {
            {CannonState.Shooting, new ShootingState(this)},
            {CannonState.Idle, new IdleState(this)}
        };
        
        _elementMaterials = new Dictionary<ElementFlag, Material>();
        foreach (var elementMaterial in blasterElementMaterials)
            _elementMaterials.Add(elementMaterial.elementFlag, elementMaterial.material);
        
        SetBlasterMaterial();
        _currentState = _states[CannonState.Idle];
        _currentState.EnterState();
    }

    private void SetBlasterMaterial()
    {
        if (soloCannon) return;
        blasterRenderer.material = _elementMaterials[blasterElement];
    }

    private void PopulateInput()
    {
        var hand = GetComponentInParent<VRHand>();
        if (!hand || soloCannon) return;
        var side =hand.handSide;
        var handSideString = "Right";
        if (side == HandSide.LEFT) handSideString = "Left";
        _triggerAction = actionAsset.FindAction($"XRI {handSideString} Interaction/UI Press");
        _triggerAction.Enable();
        _triggerAction.performed += TriggerPerformedAction;
        _triggerAction.canceled += TriggerReleasedAction;
    }

    public void ChangeState(CannonState state)
    {
        _currentState?.ExitState();
        _currentState = _states[state];
        _currentState.EnterState();
    }

    public void TriggerPerformedAction(InputAction.CallbackContext obj)
    {
        animator.Play("Fire");
    }

    internal void TriggerReleasedAction(InputAction.CallbackContext obj)
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
        if (soloCannon) return;
        PopulateInput();
        if (_triggerAction == null) return;
        _triggerAction.Enable();
    }

    public void Shoot()
    {
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