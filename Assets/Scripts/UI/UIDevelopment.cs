using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Enemies;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public class UIDevelopment : MonoBehaviour
{
    [SerializeField] private InputActionAsset actionAsset;
    [SerializeField] private VRHand vrHand;
    [SerializeField] private DevController devController;
    private HandSide handSide => vrHand.handSide;

    [SerializeField] private float radialMenuStartAngle;
    [SerializeField] private float radialMenuSpread;
    [SerializeField] private float radialMenuAngle;

    [SerializeField] private float menuDuration;

    [SerializeField] private ElementFlag firstElement;
    [SerializeField] private ElementFlag secondElement;
    [SerializeField] private ElementFlag thirdElement;
    [SerializeField] private ElementFlag fourthElement;
    [SerializeField] private ElementFlag fifthElement;

    private Vector3 _radialDirection;
    private Sequence _menuSequence;

    private ElementFlag[] _elementFlags;
    private Transform _highlightedChild;
    private Transform[] _children;

    private InputAction _gripAction;
    private InputAction _radialMenuAction;
    private Vector2 _radialMenuInput;

    private void OnDrawGizmos()
    {
        return;
        foreach (Transform child in transform)
        {
            Gizmos.color = Color.magenta;
            if (child.gameObject.name.ToLower().Contains("fire")) Gizmos.color = Color.red;
            else if (child.gameObject.name.ToLower().Contains("elect")) Gizmos.color = Color.yellow;
            else if (child.gameObject.name.ToLower().Contains("water")) Gizmos.color = Color.blue;
            else if (child.gameObject.name.ToLower().Contains("rock")) Gizmos.color = new Color(.7f, .3f, .1f);
            else if (child.gameObject.name.ToLower().Contains("wind")) Gizmos.color = Color.white;

            if (Gizmos.color == Color.magenta) continue;
            Gizmos.DrawWireSphere(child.transform.position, 0.01f);
        }
    }

    private void CacheMenuChildren()
    {
        int childCount = transform.childCount;
        _children = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            _children[i] = transform.GetChild(i);
        }
    }

    private void OnEnable()
    {
        foreach (Transform child in transform)
            child.transform.position = Vector3.zero;
    }

    private void Start()
    {
        _menuSequence = DOTween.Sequence();
        _radialDirection = handSide == HandSide.RIGHT ? Vector2.left : Vector2.right;
        PopulateInput();
        CacheMenuChildren();
        CacheElementFlags();
    }

    private void OnDestroy()
    {
        _gripAction.performed -= GripPerformedAction;
        _gripAction.canceled -= GripReleasedAction;

        _radialMenuAction.performed -= RadialMenuPerformedAction;
        _radialMenuAction.canceled -= RadialMenuCanceledAction;
    }

    private void PopulateInput()
    {
        var handSideString = "Right";
        if (handSide == HandSide.LEFT) handSideString = "Left";

        _gripAction = actionAsset.FindAction($"XRI {handSideString} Interaction/Select");

        if (handSide == HandSide.LEFT)
            _radialMenuAction = actionAsset.FindAction($"XRI {handSideString} Locomotion/Turn");
        else _radialMenuAction = actionAsset.FindAction($"XRI {handSideString} Locomotion/Move");

        _gripAction.Enable();
        _gripAction.performed += GripPerformedAction;
        _gripAction.canceled += GripReleasedAction;
    }

    private void RadialMenuCanceledAction(InputAction.CallbackContext ctx)
    {
        _radialMenuInput = Vector2.zero;
        _highlightedChild = null;
    }

    private void RadialMenuPerformedAction(InputAction.CallbackContext ctx)
    {
        _radialMenuInput = ctx.ReadValue<Vector2>();
        HighlightChildBasedOnInput();
    }

    private void GripPerformedAction(InputAction.CallbackContext obj)
    {
        devController.DisableThumbstick(handSide);
        _radialMenuAction.performed += RadialMenuPerformedAction;
        _radialMenuAction.canceled += RadialMenuCanceledAction;
        _radialMenuAction.Enable();
        OpenMenu();
    }

    private void GripReleasedAction(InputAction.CallbackContext obj)
    {
        _radialMenuAction.Disable();
        _radialMenuAction.performed -= RadialMenuPerformedAction;
        _radialMenuAction.canceled -= RadialMenuCanceledAction;
        _radialMenuInput = Vector2.zero;
        devController.EnableThumbstick(handSide);

        CloseMenu();
    }

    public void OpenMenu()
    {
        AnimateOpenMenu();
    }

    private async UniTaskVoid OffsetCloseMenu()
    {
        await UniTask.WaitUntil(() => !_animating);
        _animating = true;
        var offset = 0.1f;
        foreach (Transform child in transform)
        {
            child.DOScale(Vector3.zero, offset).SetEase(Ease.OutBounce)
                .OnComplete(() => child.gameObject.SetActive(false));
            
            await UniTask.WaitForSeconds(offset / 2);
        }

        _animating = false;
    }

    private bool _animating;

    public void CloseMenu()
    {
        OffsetCloseMenu().Forget();
        if (_highlightedChild == null) return;

        var highlightedIndex = System.Array.IndexOf(_children, _highlightedChild);
        var selectedElement = _elementFlags[highlightedIndex];
        SelectElement(selectedElement);
    }


    private void CacheElementFlags()
    {
        _elementFlags = new[] {firstElement, secondElement, thirdElement, fourthElement, fifthElement};
    }


    private void SelectElement(ElementFlag selectedElement)
    {
        Debug.Log($"Ship element to blaster: {selectedElement}");
    }

    private void HighlightChildBasedOnInput()
    {
        var closestDistance = float.MaxValue;
        Transform closestChild = null;

        // Normalize the input direction and project onto the X-Y plane
        var input = _radialMenuInput;
        input.Normalize();

        // Iterate over all precomputed positions and compare with the input
        for (int i = 0; i < _children.Length; i++)
        {
            // Compute the direction to the child from the center, using the cached position
            Vector3 childPosition = _precomputedChildPositions[i];
            Vector2 directionToChild = new Vector2(childPosition.x, childPosition.y).normalized;

            // Compute the angular difference between the input direction and the direction to the child
            float distance = Vector2.Distance(new Vector2(input.x, input.y), directionToChild);

            // Find the closest child by comparing the smallest distance
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestChild = _children[i];
            }
        }

        if (closestChild != null && closestChild != _highlightedChild)
        {
            _highlightedChild = closestChild;
            HighlightMenuItem(_highlightedChild);
        }
    }

    private void HighlightMenuItem(Object item)
    {
        foreach (var child in _children)
        {
            if (child == item)
            {
                child.localScale = Vector3.one * 1.2f;
            }
            else
            {
                child.localScale = Vector3.one;
            }
        }
    }

    private float[] _precomputedChildAngles;
    private Vector3[] _precomputedChildPositions;

    private void AnimateOpenMenu()
    {
        _animating = true;
        _menuSequence.Kill();
        _menuSequence = DOTween.Sequence();

        // Use precomputed positions if they exist to avoid recalculation
        if (_precomputedChildPositions != null)
        {
            AnimateWithPrecomputedPositions();
            return;
        }

        var radialStart = GetRadialStart();
        var childCount = _children.Length;

        // todo, don't need this anymore
        var directionMultiplier = 1;

        _precomputedChildAngles = new float[childCount];
        _precomputedChildPositions = new Vector3[childCount];

        for (var i = 0; i < childCount; i++)
        {
            var child = _children[i];

            child.localScale = Vector3.zero;
            child.localPosition = Vector3.zero;

            var angleStep = (radialMenuAngle / (childCount - 1)) * i;
            var angle = radialMenuAngle + angleStep * directionMultiplier;
            var radian = angle * Mathf.Deg2Rad;

            var x = Mathf.Cos(radian) * radialMenuSpread;
            var y = Mathf.Sin(radian) * radialMenuSpread;
            var newPos = new Vector3(x, y, 0) + radialStart +
                         radialMenuStartAngle * directionMultiplier * Vector3.right;

            _precomputedChildPositions[i] = newPos;
            _precomputedChildAngles[i] = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
            if (_precomputedChildAngles[i] < 0) _precomputedChildAngles[i] += 360f;
            if (handSide == HandSide.RIGHT)
            {
                child.transform.position = newPos;
            }
            else
            {
                child.transform.localPosition = Vector3.zero;
                _menuSequence.Join(child.DOLocalMove(newPos, menuDuration).SetEase(Ease.InBounce));
            }
            
            _menuSequence.Join(child.DOScale(Vector3.one, menuDuration).SetEase(Ease.OutBounce));
            child.gameObject.SetActive(true);
        }

        _menuSequence.Play().OnComplete(() => _animating = false);
    }

    private void AnimateWithPrecomputedPositions()
    {
        for (var i = 0; i < _children.Length; i++)
        {
            var child = _children[i];
            child.DOKill();
            var newPos = _precomputedChildPositions[i];

            if (handSide == HandSide.RIGHT)
            {
                child.transform.position = newPos;
            }
            else
            {
                child.transform.localPosition = Vector3.zero;
                _menuSequence.Join(child.DOLocalMove(newPos, menuDuration).SetEase(Ease.InBounce));
            }
            _menuSequence.Join(child.DOScale(Vector3.one, menuDuration).SetEase(Ease.OutBounce));
            child.gameObject.SetActive(true);
        }

        _menuSequence.Play().OnComplete(() => _animating = false);
    }


    private Vector3 GetRadialStart()
    {
        var startAngleRad = radialMenuStartAngle * Mathf.Deg2Rad;
        var rotatedDirection = new Vector3(
            _radialDirection.x * Mathf.Cos(startAngleRad) + _radialDirection.y * Mathf.Sin(startAngleRad),
            _radialDirection.x * Mathf.Sin(startAngleRad) + _radialDirection.y * Mathf.Cos(startAngleRad),
            0
        );

        var startPos = transform.localPosition;
        startPos += rotatedDirection * radialMenuSpread;

        return startPos;
    }
}