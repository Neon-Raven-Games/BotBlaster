using System.Collections;
using UnityEngine;

public class VignetteController : MonoBehaviour
{
    [SerializeField] private float entranceTime = 0.4f;
    [SerializeField] private float exitTime = 0.4f;
    [SerializeField] private float targetApertureSize = 0.7f;
    [SerializeField] private  bool rotationVignette;
    [SerializeField] private bool locomotionVignette;
    private bool _lerping;
    private MaterialPropertyBlock _propertyBlock;
    private MeshRenderer _meshRenderer;
    private static readonly int _SApertureSize = Shader.PropertyToID("_ApertureSize");
    private static readonly int _SFeatheringEffect = Shader.PropertyToID("_FeatheringEffect");
    private static readonly int _SVignetteColor = Shader.PropertyToID("_VignetteColor");
    private static readonly int _SVignetteColorBlend = Shader.PropertyToID("_VignetteColorBlend");

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _propertyBlock = new MaterialPropertyBlock();
        
        _meshRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetFloat(_SApertureSize, 1);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
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
