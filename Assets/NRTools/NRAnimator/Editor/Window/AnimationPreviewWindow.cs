using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Enemies;
using GraphProcessor;
using NRTools.Animator.NRNodes;
using NRTools.CustomAnimator;
using NRTools.GpuSkinning;
using OpenCover.Framework.Model;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using AnimatorNode = NRTools.Animator.NRNodes.AnimatorNode;

public class AnimationPreviewWindow : EditorWindow
{
    public GameObject previewPrefab;
    public static float currentTime;
    private readonly float animationDuration = 5f;

    internal float GetDuration()
    {
        if (_meshAnimator == null) return 5f;
        return _meshAnimator.AnimationDuration;
    }

    // todo, this needs to be brought into preferences
    private static void CreateAtlasMaterial()
    {
        var materialPath = "Assets/NRTools/GpuSkinning/Skinning2.0.mat";
        _material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        _animationManager.atlasMaterial = _material;
    }

    private float _distance = 5f;
    private static Camera _previewCamera;
    private RenderTexture _previewTexture;
    private static GameObject _previewObject;
    private Scene _previewScene;
    private Vector2 _cameraRotation = Vector2.zero;
    private static PreviewPlayBar _playBar;
    private readonly float _rotationSpeed = 0.5f;
    private Rect _previewRect;
    private static GpuMeshAnimator _meshAnimator;
    private static AnimationManager _animationManager;
    private static Material _material;

    private static readonly int _SColor0 = Shader.PropertyToID("_Color0");
    private static readonly int _SColor1 = Shader.PropertyToID("_Color1");
    private static readonly int _SExp = Shader.PropertyToID("_Exp");
    private static readonly int _SIntensity = Shader.PropertyToID("_Intensity");
    private static readonly int _SRotation = Shader.PropertyToID("_Rotation");
    private AnimationTransitionController _transitionController;
    private bool restored;
    
    [MenuItem("Development/Animation Preview")]
    public static void ShowWindow()
    {
        GetWindow<AnimationPreviewWindow>("Animation Preview");
    }

    private static void OnFrameUpdate(float deltaSeconds)
    {
        if (_meshAnimator == null) return;
        _meshAnimator.EditorUpdate(deltaSeconds);
        
    }

    private void OnBeforeAssemblyReload()
    {
        OnDestroy();
        AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
    }
    
    private void OnAfterAssemblyReload()
    {
        restored = false;
        EditorApplication.delayCall += RestoreModel;
    }

    private void RestoreModel()
    {
        if (restored) return;
        restored = true;
        
        EditorApplication.delayCall -= RestoreModel;
        OnEnable();
        
    }

    private void OnDestroy()
    {
        if (_animationManager != null) _animationManager.ReleaseBuffer();
        if (_previewCamera != null) DestroyImmediate(_previewCamera.gameObject);
        if (_previewTexture != null) _previewTexture.Release();
        if (_previewScene.IsValid()) EditorSceneManager.ClosePreviewScene(_previewScene);
        if (_playBar != null) _playBar.Disable();
        
        _playBar = null;
        _meshAnimator = null;
        AnimationController.OnAnimatorChanged -= OnAnimatorChanged;
        AnimationController.OnAnimationChanged -= OnAnimationChanged;
        // AnimationController.OnTransition -= OnPreviewTransition;
    }

    private void OnEnable()
    {
        CreateCamera();

        _distance = 6;
        minSize = new Vector2(320, 340);
        _cameraRotation = new Vector2(145, 15);

        _previewTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
        _previewTexture.Create();

        PopulateAndDeserializeAnimManager();

        if (_playBar != null) _playBar.Disable();

        _playBar = new PreviewPlayBar(this);
        _playBar.onUpdateFrame += OnFrameUpdate;
        _playBar.Enable();

        AnimationController.OnAnimatorChanged += OnAnimatorChanged;
        AnimationController.OnAnimationChanged += OnAnimationChanged;
        // AnimationController.OnTransition += OnPreviewTransition;
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
    }
    
    private static void CreateCamera()
    {
        var previewCameraObject = new GameObject("Preview Camera");
        _previewCamera = previewCameraObject.AddComponent<Camera>();
    }

    private void SetupCameraScene()
    {
        _previewCamera.cameraType = CameraType.Preview;
        _previewCamera.scene = _previewScene;
        _previewCamera.targetTexture = _previewTexture;
        _previewCamera.clearFlags = CameraClearFlags.Skybox;
        CreateSkyboxMaterial();
    }

    private void OnPreviewTransition(AnimatorNode animatorNode)
    {
        _meshAnimator.TransitionTo(animatorNode.transitionsTo.Values.FirstOrDefault());
        _playBar.SetPlaying(true);
    }

    private void OnAnimationChanged(AnimatorNode obj)
    {
        currentTime = 0;
        _meshAnimator.PlayAnimation(obj);
        _playBar.SetPlaying(true);
    }

    private void OnAnimatorChanged(List<string> obj)
    {
        if (_playBar != null) _playBar.Disable();
        OnDestroy();
        OnEnable();
    }

    private static void CreateSkyboxMaterial()
    {
        var gradientSkyboxMaterial = new Material(Shader.Find("MK/Toon/Gradient Skybox"));
        gradientSkyboxMaterial.SetColor(_SColor0, new Color(0.6f, 0.8f, 1f));
        gradientSkyboxMaterial.SetColor(_SColor1, new Color(0.1f, 0.1f, 0.3f));
        gradientSkyboxMaterial.SetFloat(_SExp, 3f);
        gradientSkyboxMaterial.SetFloat(_SIntensity, 0.5f);
        gradientSkyboxMaterial.SetVector(_SRotation, new Vector4(0, 0, 0, 0));

        var cameraSkybox = _previewCamera.GetComponent<Skybox>();
        if (cameraSkybox == null) cameraSkybox = _previewCamera.gameObject.AddComponent<Skybox>();

        cameraSkybox.material = gradientSkyboxMaterial;
    }


    private void OnGUI()
    {
        if (!previewPrefab) return;
        if (_animationManager == null || _previewCamera == null) return;
        if (_previewObject == null) return;

        _previewRect = GUILayoutUtility.GetAspectRect(1.5f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        if (_previewTexture != null && _previewCamera != null)
        {
            GUI.DrawTexture(_previewRect, _previewTexture, ScaleMode.ScaleToFit, false);
            _previewCamera.Render();
        }
        else
        {
            GUILayout.Label("Preview texture not available!");
        }

        _playBar?.Draw();
        HandleCameraZoom();
        HandleCameraRotation();
        UpdateCameraTransform();

        Repaint();
    }


    private void InitializePreviewObjectInPreviewScene(GameObject prefab)
    {
        if (_previewScene.IsValid()) EditorSceneManager.ClosePreviewScene(_previewScene);
        _previewScene = EditorSceneManager.NewPreviewScene();
        if (!_previewCamera) CreateCamera();
        SetupCameraScene();

        SceneManager.MoveGameObjectToScene(_previewCamera.gameObject, _previewScene);

        _previewObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(_previewObject, _previewScene);

        _previewCamera.transform.position = _previewObject.transform.position + new Vector3(0, 1, -5);
        _previewCamera.transform.LookAt(_previewObject.transform.position);

        GetMeshAnimator();
        SetMeshAnimatorEnemyType();
        EditorSceneManager.MarkSceneDirty(_previewScene);
    }

    private static void SetMeshAnimatorEnemyType()
    {
        var enemy = _previewObject.GetComponent<Enemy>();

        if (!enemy)
        {
            if (AnimationController.currentAnimator == "Swarm") _meshAnimator.enemyType = EnemyType.Swarm;
        }
        else _meshAnimator.enemyType = enemy.enemyType;

        _meshAnimator.UpdateElement(ElementFlag.Electricity);
    }

    private static void GetMeshAnimator()
    {
        _meshAnimator = _previewObject.GetComponent<GpuMeshAnimator>();
        _meshAnimator.SetTransitionController();
    }

    private void PopulateAndDeserializeAnimManager()
    {
        var animController = FindObjectOfType<AnimationController>();
        AnimationController.SetInstance(animController);
        previewPrefab = AnimationController.GetPrefab();

        _animationManager = _previewCamera.AddComponent<AnimationManager>();
        CreateAtlasMaterial();

        AnimationManager.OnLoaded += SetPreviewModel;
        
        _animationManager.EditorDeserialize();
    }

    private void SetPreviewModel()
    {
        InitializePreviewObjectInPreviewScene(previewPrefab);
        Repaint();
        AnimationController.RaiseOnLoaded();
    }
    
    private void HandleCameraRotation()
    {
        Event e = Event.current;

        if (_previewRect.Contains(e.mousePosition))
        {
            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                _cameraRotation.x += e.delta.x * _rotationSpeed;
                _cameraRotation.y += e.delta.y * _rotationSpeed;

                _cameraRotation.y = Mathf.Clamp(_cameraRotation.y, -90f, 90f);

                e.Use();
            }
        }
    }
    private void HandleCameraZoom()
    {
        var e = Event.current;

        if (_previewRect.Contains(e.mousePosition))
        {
            if (e.type == EventType.ScrollWheel)
            {
                float zoomAmount = e.delta.y * 0.1f;
                _distance = Mathf.Clamp(_distance + zoomAmount, 1f, 10f);

                e.Use();
            }
        }
    }

    private void UpdateCameraTransform()
    {
        var rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0);
        var direction = rotation * Vector3.forward;

        _previewCamera.transform.position =
            _previewObject.transform.position - direction * _distance;

        _previewCamera.transform.LookAt(_previewObject.transform.position);
    }
}