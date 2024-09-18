using System.Collections;
using DG.Tweening;
using UnityEngine;

public class VignetteController : MonoBehaviour
{
    [Header("Damage Vignette Settings")] [SerializeField]
    private float damageApertureSize = 0.715f;
    [SerializeField] private float damageFeatheringEffect = 0.833f;
    [SerializeField] private Color damageVignetteColor = new(1, 0, 0, 1f);
    [SerializeField] private float vignetteDuration = 0.5f;
    [SerializeField] private float vignetteResetDuration = 0.75f;

    [Header("Comfort Vignette Settings")] [SerializeField]
    private float entranceTime = 0.4f;

    [SerializeField] private float exitTime = 0.4f;
    [SerializeField] private float targetApertureSize = 0.7f;

    public bool rotationVignette;
    public bool locomotionVignette;

    private bool _lerping;
    private MaterialPropertyBlock _propertyBlock;
    private MeshRenderer _meshRenderer;
    private static readonly int _SApertureSize = Shader.PropertyToID("_ApertureSize");
    private static readonly int _SFeatheringEffect = Shader.PropertyToID("_FeatheringEffect");
    private static readonly int _SVignetteColor = Shader.PropertyToID("_VignetteColor");
    private static readonly int _SVignetteColorBlend = Shader.PropertyToID("_VignetteColorBlend");
    
    // private damage tween
    private float _currentApertureSize;
    private float _currentFeatheringEffect;
    private Color _currentVignetteColor;
    private Tween _vignetteTween;

    public void PunchTweenDamageVignette()
    {
        // If there's already a tween playing, kill it to prevent overlapping
        _vignetteTween?.Kill();

        // Get current property block settings
        _meshRenderer.GetPropertyBlock(_propertyBlock);

        // Animate the aperture size, feathering, and color using DOTween
        _vignetteTween = DOTween.To(() => _currentApertureSize, x => SetApertureSize(x), damageApertureSize * 0.5f,
                vignetteDuration)
            .SetEase(Ease.OutBack) // Punch effect ease
            .OnComplete(() => ResetVignetteEffect());

        DOTween.To(() => _currentFeatheringEffect, x => SetFeatheringEffect(x), damageFeatheringEffect * 1.5f,
                vignetteDuration)
            .SetEase(Ease.OutBack);

        DOTween.To(() => _currentVignetteColor, x => SetVignetteColor(x), damageVignetteColor, vignetteDuration)
            .SetEase(Ease.OutBack);
    }

    private void SetApertureSize(float value)
    {
        _currentApertureSize = value;
        _propertyBlock.SetFloat(_SApertureSize, _currentApertureSize);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void SetFeatheringEffect(float value)
    {
        _currentFeatheringEffect = value;
        _propertyBlock.SetFloat(_SFeatheringEffect, _currentFeatheringEffect);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void SetVignetteColor(Color value)
    {
        _currentVignetteColor = value;
        _propertyBlock.SetColor(_SVignetteColor, _currentVignetteColor);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void ResetVignetteEffect()
    {
        // Reset all the properties back to normal over time
        DOTween.To(() => _currentApertureSize, x => SetApertureSize(x), damageApertureSize, vignetteResetDuration)
            .SetEase(Ease.InOutSine);

        DOTween.To(() => _currentFeatheringEffect, x => SetFeatheringEffect(x), damageFeatheringEffect,
                vignetteResetDuration)
            .SetEase(Ease.InOutSine);

        DOTween.To(() => _currentVignetteColor, x => SetVignetteColor(x), new Color(1, 0, 0, 0), vignetteResetDuration)
            .SetEase(Ease.InOutSine);
    }

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _propertyBlock = new MaterialPropertyBlock();

        _meshRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetFloat(_SApertureSize, 1);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
        _currentApertureSize = damageApertureSize;
        _currentFeatheringEffect = damageFeatheringEffect;
        _currentVignetteColor = damageVignetteColor;
    }


    public void StartLocomotionLerp()
    {
        if (!locomotionVignette || _locomotion) return;
        if (_lerping) StopAllCoroutines();
        _lerping = true;
        _locomotion = true;
        StartCoroutine(LerpRotation(targetApertureSize, entranceTime));
    }

    public void StartRotationLerp(RotationMode rotation)
    {
        if (rotation == RotationMode.Snap)
        {
            StartCoroutine(SnapRotation());
            _rotation = true;
            return;
        }

        if (!rotationVignette || _rotation) return;
        if (_lerping) StopAllCoroutines();
        _lerping = true;
        _rotation = true;
        StartCoroutine(LerpRotation(targetApertureSize, entranceTime));
    }

    private bool _locomotion;
    private bool _rotation;

    public void StopLocomotionLerp()
    {
        _locomotion = false;
    }

    public void StopRotationLerp()
    {
        _rotation = false;
    }

    public void StopVignette()
    {
        if (_locomotion || _rotation) return;
        _lerping = false;
        StopAllCoroutines();
        StartCoroutine(LerpRotation(1f, entranceTime));
    }

    IEnumerator SnapRotation()
    {
        var startApertureSize = _propertyBlock.GetFloat(_SApertureSize);
        var time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / entranceTime;
            _propertyBlock.SetFloat(_SApertureSize, Mathf.Lerp(startApertureSize, targetApertureSize, time));
            _meshRenderer.SetPropertyBlock(_propertyBlock);
            yield return null;
        }

        yield return null;
        StopRotationLerp();

        time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / exitTime;
            _propertyBlock.SetFloat(_SApertureSize, Mathf.Lerp(targetApertureSize, 1f, time));
            _meshRenderer.SetPropertyBlock(_propertyBlock);
            yield return null;
        }

        _lerping = false;
    }

    IEnumerator LerpRotation(float apertureSize, float transitionTime)
    {
        var startApertureSize = _propertyBlock.GetFloat(_SApertureSize);
        var time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / transitionTime;
            _propertyBlock.SetFloat(_SApertureSize, Mathf.Lerp(startApertureSize, apertureSize, time));
            _meshRenderer.SetPropertyBlock(_propertyBlock);
            yield return null;
        }
    }


    private void LerpVignette()
    {
        // todo, do we need this?
        // var thisTransform = transform;
        // var localPosition = thisTransform.localPosition;
        // if (!Mathf.Approximately(localPosition.y, parameters.apertureVerticalPosition))
        // {
        //     localPosition.y = parameters.apertureVerticalPosition;
        //     thisTransform.localPosition = localPosition;
        // }
    }
}