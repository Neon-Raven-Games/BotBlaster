using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Enemies;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum RotationMode
{
    Smooth,
    Snap
}

public enum LocomotionMode
{
    Smooth,
    Teleport
}

public enum HandSide
{
    LEFT,
    RIGHT
}

[RequireComponent(typeof(CharacterController))]
public class DevController : Actor
{
    [Header("Health Settings")] [SerializeField]
    private Slider healthBarSlider;

    [SerializeField] private Slider bigCannonHealthBarSlider;

    [Header("Input Settings")] [SerializeField]
    private InputActionAsset actionAsset;

    [SerializeField] private VRHand leftHand;
    [SerializeField] private VRHand rightHand;

    [SerializeField] private float analogThreshold = 0.2f;
    [SerializeField] private Transform hmd;
    [SerializeField] private Transform handsAnchor;

    [Header("Rotation Settings")] [SerializeField]
    private RotationMode rotationMode;

    [SerializeField] private float smoothRotationSpeed = 100.0f;
    [SerializeField] private float snapRotationAmount = 45f;
    [SerializeField] private float snapRotationDelay;

    [Header("Movement Settings")] [SerializeField]
    private LocomotionMode locomotionMode;

    [SerializeField] private float speed = 5.0f;

    [Header("Comfort Settings")] [SerializeField]
    private bool initialRotationVignette;

    [SerializeField] private bool initialLocomotionVignette;

    public bool RotationVignette
    {
        get => _vignetteController.rotationVignette;
        set => _vignetteController.rotationVignette = value;
    }

    public bool LocomotionVignette
    {
        get => _vignetteController.locomotionVignette;
        set => _vignetteController.locomotionVignette = value;
    }
    // todo, teleporter

    // character populated components
    private CharacterController _controller;
    private VignetteController _vignetteController;

    private InputAction _moveForwardAction;
    private InputAction _lookAction;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _lastSnapRotation;

    private void Start()
    {
        currentHealth = baseHealth;

        _vignetteController = GetComponentInChildren<VignetteController>();
        _controller = GetComponent<CharacterController>();
        LocomotionVignette = initialLocomotionVignette;
        RotationVignette = initialRotationVignette;
    }

    public void HapticFeedback()
    {
        leftHand.PlayHapticImpulse(0.5f, 0.5f);
        rightHand.PlayHapticImpulse(0.5f, 0.5f);
    }

    public void PlayLeftFeedback()
    {
        leftHand.PlayHapticImpulse(0.75f, 0.2f);
    }

    public void PlayRightFeedback()
    {
        rightHand.PlayHapticImpulse(0.75f, 0.2f);
    }

    private void OnApplicationFocusChanged(bool hasFocus)
    {
        if (hasFocus)
        {
            var handPos = handsAnchor.localPosition;
            handPos.x = 0;
            handPos.z = 0;
            handPos.y = _controller.height;
            handsAnchor.localPosition = handPos;
            WaveController.paused = false;
        }
        else
        {
            WaveController.paused = true;
        }
    }

    private void Awake()
    {
        _moveForwardAction = actionAsset.FindAction("XRI Left Locomotion/Move", true);
        _moveForwardAction.Enable();

        _lookAction = actionAsset.FindAction("XRI Right Locomotion/Turn", true);
        _lookAction.Enable();

        _moveForwardAction.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _lookAction.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();

        _moveForwardAction.canceled += _ => _moveInput = Vector2.zero;
        _lookAction.canceled += _ => _lookInput = Vector2.zero;

        if (Application.platform != RuntimePlatform.WindowsEditor)
            Application.focusChanged += OnApplicationFocusChanged;
        currentDamage = baseDamage;
        currentHealth = baseHealth;
        currentSpeed = baseSpeed;
        currentAttackRange = baseAttackRange;
    }

    private void OnDestroy()
    {
        if (Application.platform != RuntimePlatform.WindowsEditor)
            Application.focusChanged -= OnApplicationFocusChanged;
    }


    private void OnEnable()
    {
        if (_moveForwardAction != null) _moveForwardAction.Enable();
        if (_lookAction != null) _lookAction.Enable();
    }

    private void OnDisable()
    {
        _moveForwardAction.Disable();
        _lookAction.Disable();
    }

    protected override void Update()
    {
        UpdateHealthBar();
        HandleRotation();
        HandleMovement();
        SynchBaseObjectWithCamera();
        _vignetteController.StopVignette();
    }




    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        
        // Gizmos.DrawSphere(_controller.transform.position, 0.2f);
        Gizmos.color = Color.red;
        // Gizmos.DrawSphere(hmd.transform.position, 0.2f);
    }

    private void SynchBaseObjectWithCamera()
    {
        var hmdPos = hmd.position;
        var targetPosition = new Vector3(hmdPos.x, transform.position.y, hmdPos.z);
        var movementOffset = targetPosition - transform.position;
        
        transform.position += movementOffset;
        hmd.position = hmdPos;
        handsAnchor.localPosition = new Vector3(hmd.localPosition.x, handsAnchor.localPosition.y, hmd.localPosition.z);
        ResizeControllerHeightToHmd();
    }

    // private void OnControllerColliderHit(ControllerColliderHit hit)
    // {
    //     Debug.Log("Hit Normal: " + hit.normal);
    //     if (hit.normal.y < 0.5f || hit.normal.y > 0.5f)
    //     {
    //         Debug.Log("Not wall hit");
    //         return;
    //     }
    //     transform.position = hit.point + hit.normal * _controller.radius;
    // }

    private void ResizeControllerHeightToHmd()
    {
        var hmdPos = hmd.position;
        var controllerPos = _controller.transform.position;
        var heightDifference = hmdPos.y - controllerPos.y;
        _controller.height = heightDifference + 0.1f;
        _controller.center = new Vector3(0, heightDifference / 2 + 0.05f, 0);
    }

    private void HandleRotation()
    {
        if (Mathf.Abs(_lookInput.x) < analogThreshold)
        {
            if (rotationMode == RotationMode.Smooth) _vignetteController.StopRotationLerp();
            return;
        }

        if (rotationMode == RotationMode.Snap)
        {
            if (Time.time - _lastSnapRotation < snapRotationDelay) return;
            _lastSnapRotation = Time.time;
        }

        _vignetteController.StartRotationLerp(rotationMode);

        var angle = rotationMode == RotationMode.Smooth
            ? _lookInput.x * smoothRotationSpeed * Time.fixedDeltaTime
            : _lookInput.x;
        HandleRotationWithVignette(angle);
    }

    private void HandleRotationWithVignette(float angle)
    {
        if (rotationMode == RotationMode.Snap)
        {
            if (angle < -analogThreshold) angle = -snapRotationAmount;
            else if (angle > analogThreshold) angle = snapRotationAmount;
        }

        transform.Rotate(Vector3.up, angle);
    }


    private void HandleMovement()
    {
        if (Mathf.Abs(_moveInput.x) <= analogThreshold) _moveInput.x = 0;
        if (Mathf.Abs(_moveInput.y) <= analogThreshold) _moveInput.y = 0;
        if (_moveInput == Vector2.zero)
        {
            _vignetteController.StopLocomotionLerp();
            return;
        }

        _vignetteController.StartLocomotionLerp();
        var movement = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
        movement = hmd.transform.TransformDirection(movement) * (speed * Time.deltaTime);
        movement.y = Physics.gravity.y * Time.deltaTime;
        _controller.Move(movement);
    }

    public const float healthUpgrade = 1.1f;
    public const float damageUpgrade = 1.1f;
    public const float elementStatusIncrement = 0.1f;
    public const float elementEffectivenessIncrement = 0.1f;

    public Dictionary<ElementFlag, float> elementStatusUpgrades = new Dictionary<ElementFlag, float>
    {
        {ElementFlag.Fire, 1f},
        {ElementFlag.Water, 1f},
        {ElementFlag.Rock, 1f},
        {ElementFlag.Wind, 1f},
        {ElementFlag.Electricity, 1f}
    };

    public Dictionary<ElementFlag, int> elementEffectivenessUpgrades = new Dictionary<ElementFlag, int>
    {
        {ElementFlag.Fire, 1},
        {ElementFlag.Water, 1},
        {ElementFlag.Rock, 1},
        {ElementFlag.Wind, 1},
        {ElementFlag.Electricity, 1}
    };

    private IEnumerator HealthUpRoutine(int targetHp)
    {
        var t = 0f;
        while (t < 2f)
        {
            t += Time.deltaTime;
            currentHealth = (int) Mathf.Lerp(currentHealth, targetHp, t / 3f);
            var healthPercentage = currentHealth / (float) baseHealth;
            healthBarSlider.value = healthPercentage;
            bigCannonHealthBarSlider.value = healthPercentage;
            yield return null;
        }

        currentHealth = baseHealth;
    }

    public int FetchEffectiveElementalDamage(ElementFlag elementFlag)
    {
        return Mathf.CeilToInt(currentDamage * elementEffectivenessUpgrades[elementFlag]);
    }

    public int FetchDamage()
    {
        return currentDamage;
    }


    private void UpdateHealthBar()
    {
        if (currentHealth <= 0 || !healthBarSlider || !bigCannonHealthBarSlider) return;
        var healthPercentage = currentHealth / (float) baseHealth;
        healthBarSlider.value = healthPercentage;
        bigCannonHealthBarSlider.value = healthPercentage;
    }

    protected override void Die(StatusEffectiveness statusEffectiveness)
    {
        if (currentHealth > 0) return;
        base.Die(statusEffectiveness);
        WaveController.EndGame();
        StartCoroutine(HealthUpRoutine(baseHealth));
    }

    public void UpgradeSelected(UpgradeType type, ElementFlag elementFlag)
    {
        if (type == UpgradeType.Damage)
            elementEffectivenessUpgrades[elementFlag]++;
        else if (type == UpgradeType.StatusEffect)
            elementStatusUpgrades[elementFlag] += elementStatusIncrement;
    }

    public void UpgradeSelected(UtilityUpgrade utilityUpgrade)
    {
        if (utilityUpgrade == UtilityUpgrade.Health)
        {
            var curHealth = baseHealth;
            baseHealth = (int) (curHealth * healthUpgrade);
            if (baseHealth == curHealth) baseHealth++;
        }
        else if (utilityUpgrade == UtilityUpgrade.Damage)
        {
            var curDamage = baseDamage;
            baseDamage = (int) (curDamage * damageUpgrade);
            if (baseDamage == curDamage) baseDamage++;
        }
        else if (utilityUpgrade == UtilityUpgrade.HealthRegen)
        {
            if (currentHealth < baseHealth) StartCoroutine(HealthUpRoutine(baseHealth));
            else
            {
                Debug.LogWarning("Current health is already at max?");
                currentHealth = baseHealth;
            }
        }
    }

    public void Test_SetCurrentHealth(int health)
    {
        currentHealth = health;
    }
}