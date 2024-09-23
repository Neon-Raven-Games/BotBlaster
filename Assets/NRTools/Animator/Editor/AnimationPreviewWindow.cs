using System.Collections.Generic;
using Gameplay.Enemies;
using NRTools.CustomAnimator;
using NRTools.GpuSkinning;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private float _distance = 5f;
    private static Camera _previewCamera;
    private RenderTexture _previewTexture;
    private static GameObject _previewObject;
    private Scene _previewScene;
    private Vector2 _cameraRotation = Vector2.zero;
    private static PreviewPlayBar _playBar;
    private float _rotationSpeed = 0.5f;
    private Rect _previewRect;
    private static GpuMeshAnimator _meshAnimator;
    private static AnimationManager _animationManager;

    private static readonly int _SColor0 = Shader.PropertyToID("_Color0");
    private static readonly int _SColor1 = Shader.PropertyToID("_Color1");
    private static readonly int _SExp = Shader.PropertyToID("_Exp");
    private static readonly int _SIntensity = Shader.PropertyToID("_Intensity");
    private static readonly int _SRotation = Shader.PropertyToID("_Rotation");

    [MenuItem("Development/Animation Preview")]
    public static void ShowWindow()
    {
        GetWindow<AnimationPreviewWindow>("Animation Preview");
    }

    private void OnFrameUpdate(float obj)
    {
        if (_meshAnimator == null) return;
        _meshAnimator.EditorUpdate(obj);
    }

    private  void OnBeforeAssemblyReload()
    {
        _animationManager.ReleaseBuffer();
        AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
    }

    private  void OnAfterAssemblyReload()
    {
        _animationManager = _previewCamera.GetComponent<AnimationManager>();
        AnimationManager.OnLoaded += SetPreviewModel;
        _animationManager.EditorDeserialize();
        
        AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
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
    }

    private void OnEnable()
    {
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        CreateCamera();

        _distance = 6;
        minSize = new Vector2(320, 340);
        _cameraRotation = new Vector2(145, 15);

        _previewTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
        _previewTexture.Create();

        PopulateAndDeserializeAnimManager();

        if (_playBar != null) _playBar.Disable();
        
        // update the UI
        _playBar = new PreviewPlayBar(this);
        _playBar.onUpdateFrame += OnFrameUpdate;
        _playBar.Enable();
        // make the scene dirty

        AnimationController.OnAnimatorChanged += OnAnimatorChanged;
        AnimationController.OnAnimationChanged += OnAnimationChanged;
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

    private void OnAnimationChanged(string obj)
    {
        currentTime = 0;
        _meshAnimator.PlayAnimation(AnimationController.currentAnimator, obj);
    }

    private void OnAnimatorChanged(List<string> obj)
    {
        _playBar.Disable();
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

    private static Material material;

    private void OnGUI()
    {
        if (!previewPrefab) return;
        if (_animationManager == null || _previewCamera == null) return;
        if (_previewObject == null) return;

        // do we want to add element select?
        // if (_meshAnimator != null && GUILayout.Button("PlayAttack"))
            // _meshAnimator.UpdateElement(ElementFlag.Electricity);


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
        _previewScene = EditorSceneManager.NewPreviewScene();
        SetupCameraScene();

        SceneManager.MoveGameObjectToScene(_previewCamera.gameObject, _previewScene);

        // populate the preview scene we our new spawned dood
        _previewObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(_previewObject, _previewScene);

        _previewCamera.transform.position = _previewObject.transform.position + new Vector3(0, 1, -5);
        _previewCamera.transform.LookAt(_previewObject.transform.position);

        // set the mesh animator to preview from the prefab.
        GetMeshAnimator();
        SetMeshAnimatorEnemyType();
        EditorSceneManager.MarkSceneDirty(_previewScene);
    }

    private static void SetMeshAnimatorEnemyType()
    {
        var enemy = _previewObject.GetComponent<Enemy>();
        if (!enemy) _meshAnimator.enemyType = EnemyType.Swarm;
        else _meshAnimator.enemyType = enemy.enemyType;
        
        _meshAnimator.UpdateElement(ElementFlag.Electricity);
    }

    private static void GetMeshAnimator()
    {
        _meshAnimator = _previewObject.GetComponent<GpuMeshAnimator>();
    }

    private void PopulateAndDeserializeAnimManager()
    {
        // get the animation controller and preview prefab
        var animController = FindObjectOfType<AnimationController>();
        AnimationController.SetInstance(animController);
        previewPrefab = AnimationController.GetPrefab();

        // create the animation manager
        _animationManager = _previewCamera.AddComponent<AnimationManager>();

        // sets material
        CreateAtlasMaterial();

        // material used for preview mode, so has to go last
        AnimationManager.OnLoaded += SetPreviewModel;
        _animationManager.EditorDeserialize();
    }

    private static void CreateAtlasMaterial()
    {
        var materialPath = "Assets/NRTools/GpuSkinning/Skinning2.0.mat";
        material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        _animationManager.atlasMaterial = material;
    }

    private void SetPreviewModel()
    {
        InitializePreviewObjectInPreviewScene(previewPrefab);
    }

    private void HandleCameraRotation()
    {
        Event e = Event.current;

        if (_previewRect.Contains(e.mousePosition))
        {
            if (e.type == EventType.MouseDrag && e.button == 0) // Left mouse button drag
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
                _distance = Mathf.Clamp(_distance + zoomAmount, 1f, 10f); // Adjust min and max distances

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