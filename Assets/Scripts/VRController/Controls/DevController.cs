using System.Collections;
using System.Collections.Generic;
using Gameplay.Enemies;
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
    [SerializeField] private Transform camOffset;

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

    // input properties
    private InputAction _moveForwardAction;
    private InputAction _lookAction;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _lastSnapRotation;

    private void Start()
    {
        currentHealth = baseHealth;
        cappedHealth = baseHealth;
        _vignetteController = GetComponentInChildren<VignetteController>();
        _controller = GetComponent<CharacterController>();
        LocomotionVignette = initialLocomotionVignette;
        RotationVignette = initialRotationVignette;
        ResetHandAnchor();
    }

    public void HapticFeedback(HandSide handSide)
    {
        if (handSide == HandSide.LEFT) PlayLeftFeedback();
        else PlayRightFeedback();
    }
    public void HapticFeedback()
    {
        leftHand.PlayHapticImpulse(0.75f, 0.5f);
        rightHand.PlayHapticImpulse(0.75f, 0.5f);
        _vignetteController.PunchTweenDamageVignette();
    }

    public void PlayLeftFeedback()
    {
        leftHand.PlayHapticImpulse(0.35f, 0.2f);
    }

    public void PlayRightFeedback()
    {
        rightHand.PlayHapticImpulse(0.35f, 0.2f);
    }

    private void OnApplicationFocusChanged(bool hasFocus)
    {
        if (hasFocus)
        {
            ResetHandAnchor();
            WaveController.paused = false;
        }
        else
        {
            WaveController.paused = true;
        }
    }

    private void ResetHandAnchor()
    {
        var handPos = handsAnchor.localPosition;
        handPos.x = 0;
        handPos.z = 0;
        handPos.y = camOffset.localPosition.y;
        handsAnchor.localPosition = handPos;
    }

    protected override void Awake()
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

        base.Awake();
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
        ResetHandAnchor();
        UpdateHealthBar();
        HandleRecentDamageTimer();
        // HandleRotation();
        // HandleMovement();
        SyncBaseObjectWithCamera();
        _vignetteController.StopVignette();
    }

    private void SyncBaseObjectWithCamera()
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

    // todo, this doesn't need to deviate from enemy scaling
    // also needs to be designed better lol
    // we can redo this system when moving over status FX to damaging
    public const float healthUpgrade = 1.1f;
    public const float damageUpgrade = 1.1f;
    public const float elementStatusIncrement = 0.1f;
    public const float elementEffectivenessIncrement = 0.1f;

    public readonly Dictionary<ElementFlag, float> elementStatusUpgrades = new()
    {
        {ElementFlag.Fire, 1f},
        {ElementFlag.Water, 1f},
        {ElementFlag.Rock, 1f},
        {ElementFlag.Wind, 1f},
        {ElementFlag.Electricity, 1f}
    };

    public readonly Dictionary<ElementFlag, float> elementEffectivenessUpgrades = new()
    {
        {ElementFlag.Fire, 1.2f},
        {ElementFlag.Water, 1.2f},
        {ElementFlag.Rock, 1.2f},
        {ElementFlag.Wind, 1.2f},
        {ElementFlag.Electricity, 1.2f}
    };

    private IEnumerator HealthUpRoutine(int targetHp)
    {
        var t = 0f;
        while (t < 2f)
        {
            t += Time.deltaTime;
            currentHealth = (int) Mathf.Lerp(currentHealth, targetHp, t / 3f);
            var healthPercentage = currentHealth / (float) cappedHealth;
            healthBarSlider.value = healthPercentage;
            bigCannonHealthBarSlider.value = healthPercentage;
            yield return null;
        }

        currentHealth = targetHp;
    }

    public int FetchEffectiveElementalDamage(ElementFlag elementFlag)
    {
        if (elementFlag == ElementFlag.None) return currentDamage;
        return Mathf.CeilToInt(currentDamage * elementEffectivenessUpgrades[elementFlag]);
    }

    public int FetchDamage()
    {
        return currentDamage;
    }

    private void UpdateHealthBar()
    {
        if (currentHealth <= 0 || !healthBarSlider || !bigCannonHealthBarSlider) return;
        var healthPercentage = currentHealth / (float) cappedHealth;
        healthBarSlider.value = healthPercentage;
        bigCannonHealthBarSlider.value = healthPercentage;
    }

    protected override void Die(StatusEffectiveness statusEffectiveness)
    {
        if (currentHealth > 0) return;
        base.Die(statusEffectiveness);
        WaveController.EndGame();
        cappedHealth = baseHealth;
        StartCoroutine(HealthUpRoutine(baseHealth));
    }

    public void UpgradeSelected(UpgradeType type, ElementFlag elementFlag)
    {
        if (type == UpgradeType.Damage)
            elementEffectivenessUpgrades[elementFlag] += elementEffectivenessIncrement;
        else if (type == UpgradeType.StatusEffect)
            elementStatusUpgrades[elementFlag] += elementStatusIncrement;
    }

    public int cappedHealth;

    public void UpgradeSelected(UtilityUpgrade utilityUpgrade)
    {
        if (utilityUpgrade == UtilityUpgrade.Health)
        {
            var currentCap = cappedHealth;
            cappedHealth = (int) (cappedHealth * healthUpgrade);
            if (cappedHealth == currentCap) cappedHealth++;
            var healthPercentage = currentHealth / (float) baseHealth;
            currentHealth = (int) (cappedHealth * healthPercentage);
        }
        else if (utilityUpgrade == UtilityUpgrade.Damage)
        {
            currentDamage = (int) (currentDamage * damageUpgrade);
            if (baseDamage == currentDamage) currentDamage++;
        }
        else if (utilityUpgrade == UtilityUpgrade.HealthRegen)
        {
            if (currentHealth < baseHealth) StartCoroutine(HealthUpRoutine(baseHealth));
            else
            {
                currentHealth = baseHealth;
            }
        }
    }

    public void Test_SetCurrentHealth(int health)
    {
        currentHealth = health;
    }

    public void EnableThumbstick(HandSide handSide)
    {
        if (handSide == HandSide.LEFT)
        {
            _moveForwardAction.Enable();
        }
        else
        {
            _lookAction.Enable();
        }
    }

    public void DisableThumbstick(HandSide handSide)
    {
        if (handSide == HandSide.LEFT)
        {
            _moveForwardAction.Disable();
        }
        else
        {
            _lookAction.Disable();
        }
    }
    

    private int recentDamageTaken;
    private float recentDamageTimerSeconds = 4f;
    private float recentDamageTimer;
    public float GetRecentDamageTaken()
    {
        return recentDamageTaken;
    }

    public void AddRecentDamageTaken(int damage)
    {
        recentDamageTaken += damage;
    }
    
    private void HandleRecentDamageTimer()
    {
        if (recentDamageTimer > 0)
        {
            recentDamageTimer -= Time.deltaTime;
            return;
        }
        recentDamageTimer = recentDamageTimerSeconds;
        recentDamageTaken = 0;
    }

}