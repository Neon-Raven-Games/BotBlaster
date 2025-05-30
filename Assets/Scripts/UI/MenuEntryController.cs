using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuEntryController : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private InputActionAsset inputActions;
    
    private InputAction _menuAction;
    private Canvas _menuCanvas;


    private void Awake()
    {
        _menuAction = inputActions.FindAction("XRI LeftHand/Menu");
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += SceneUnloaded;
        _menuCanvas = menu.GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        _menuAction.performed += MenuAction;
        _menuAction.Enable();
    }

    private void OnDisable()
    {
        _menuAction.performed -= MenuAction;
        _menuAction.Disable();
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= SceneUnloaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PopulateHandStateControllers();
    }

    private void SceneUnloaded(Scene scene)
    {
        BallPool.Sleep();
        menu.SetActive(false);
    }


    private void LateUpdate()
    {
        // todo bring in hand lasers
        // if (menu.activeInHierarchy && _leftHandController && _rightHandController)
        // {
        //     if (_leftHandController.State == HandState.Laser) _menuCanvas.worldCamera = _leftHandController.LaserCamera();
        //     if (_rightHandController.State == HandState.Laser) _menuCanvas.worldCamera = _rightHandController.LaserCamera();
        // }
    }

    private void PopulateHandStateControllers()
    {
        // todo, simplify setup for this game, just hold a reference
        // var handControllers = FindObjectsByType<HandStateController>(FindObjectsSortMode.None);
        // if (handControllers.Length == 0)
        // {
        //     Debug.LogWarning("No hand controllers found.");
        //     return;
        // }
        //
        // if (handControllers.Length == 1)
        // {
        //     Debug.LogWarning("More than 2 hand controllers found.");
        // }
        //
        // if (handControllers.Length > 2)
        // {
        //     Debug.LogWarning("More than 2 hand controllers found.");
        // }
        //
        // foreach (var controller in handControllers)
        // {
        //     if (controller.handSide == HandSide.LEFT)
        //     {
        //         _leftHandController = controller;
        //     }
        //     else if (controller.handSide == HandSide.RIGHT)
        //     {
        //         _rightHandController = controller;
        //     }
        // }
    }

    // todo, hand state controller coupling of ui functionality should move up to this level
    private void MenuAction(InputAction.CallbackContext obj)
    {
        // if (menu.activeInHierarchy)
        // {
        //     _rightHandController.ui = false;
        //     if (_rightHandController.State == HandState.Laser)
        //         _rightHandController.ChangeState(HandState.Idle);
        //     
        //     _leftHandController.ui = false;
        //     if (_leftHandController.State == HandState.Laser)
        //         _leftHandController.ChangeState(HandState.Idle);
        //     
        //     menu.GetComponent<PlayerMenuController>().HideMenu();
        // }
        // else
        // {
        //     menu.SetActive(true);
        //     _rightHandController.ChangeState(HandState.Laser);
        //     _rightHandController.ui = true;
        //     _leftHandController.ui = true;
        // }
    }
}