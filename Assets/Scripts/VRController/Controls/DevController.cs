using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
public class DevController : MonoBehaviour
{
    [Header("Input Settings")] [SerializeField]
    private InputActionAsset actionAsset;

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
        _vignetteController = GetComponentInChildren<VignetteController>();
        _controller = GetComponent<CharacterController>();
        ConfigurationManager.throwConfigIndex =
            SceneManager.GetActiveScene().buildIndex != 0 ? 1 : 0;
        LocomotionVignette = initialLocomotionVignette;
        RotationVignette = initialRotationVignette;
    }

    private void OnApplicationFocusChanged(bool hasFocus)
    {
        if (hasFocus)
        {
            Time.timeScale = 1;
            actionAsset.Enable();
        }
        else
        {
            Time.timeScale = 0;
            actionAsset.Disable();
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

#if !UNITY_EDITOR
        Application.focusChanged += OnApplicationFocusChanged;
#endif
    }

    private void OnDestroy()
    {
#if !UNITY_EDITOR
        Application.focusChanged -= OnApplicationFocusChanged;
#endif
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

    private void Update()
    {
        SynchBaseObjectWithCamera();
        HandleRotation();
        HandleMovement();
        _vignetteController.StopVignette();
    }

    private void SynchBaseObjectWithCamera()
    {
        var hmdPos = hmd.position;
        var targetPosition = new Vector3(hmdPos.x, transform.position.y, hmdPos.z);
        var movementOffset = targetPosition - transform.position;
        _controller.Move(movementOffset);
        handsAnchor.position -= movementOffset;
        hmd.position = hmdPos;
        ResizeControllerHeightToHmd();
    }

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
}