using System;
using System.Linq;
using System.Reflection;
using Gameplay.Util.Editor;
using UnityEditor;
using UnityEngine;

public class TimerManagerEditor : EditorWindow
{
    private Vector2 _scrollPosition;
    private CustomTreeView _treeView;
    private object _splitterState;
    private bool _autoRefresh;
    private const float _REFRESH_INTERVAL = 0.1f;
    private float _lastRefreshTime;


    [MenuItem("Window/Timers")]
    public static void OpenWindow()
    {
        GetWindow<TimerManagerEditor>("Timers").Show();
    }

    private void OnEnable()
    {
        _splitterState = SplitterGUILayout.CreateSplitterState(new float[] {75f, 25f}, new int[] {32, 32}, null);
        _treeView = new CustomTreeView();
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        _lastRefreshTime = 0f;
    }

    private void OnGUI()
    {
        RenderHeadPanel();

        SplitterGUILayout.BeginVerticalSplit(this._splitterState);
        {
            RenderTable();
        }
        SplitterGUILayout.EndVerticalSplit();

        if (CustomTreeView.SelectedTrackedItem != null)
        {
            HandleSelectionChange(CustomTreeView.SelectedTrackedItem);
        }
    }

    private void HandleSelectionChange(TrackedItem selectedItem)
    {
        Debug.Log($"Selected item: {selectedItem.Type}");
        Repaint();
    }

    private void RenderHeadPanel()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.ExpandWidth(true);

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton) || _autoRefresh)
        {
            CustomTreeView.GetTrackedEvents();
            _treeView.Reload();
        }
        else
        {
            _lastRefreshTime = Time.time;
        }

        _autoRefresh = GUILayout.Toggle(_autoRefresh, "Auto Refresh", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();
    }

    private void Update()
    {
        if (_autoRefresh && Time.time - _lastRefreshTime > _REFRESH_INTERVAL)
        {
            CustomTreeView.GetTrackedEvents();
            _treeView.Reload();
            _lastRefreshTime = Time.time;
            Repaint();
        }
        else
        {
            _lastRefreshTime = Time.time;
        }
    }

    private void RenderTable()
    {
        EditorGUILayout.BeginVertical("CN Box");
        var controlRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        _treeView.OnGUI(controlRect);
        EditorGUILayout.EndVertical();
    }

    public class TrackedItem
    {
        public string Type { get; }
        public float ElapsedTime { get; }
        public string Status { get; }
        public string Details { get; }
        public object Target { get; } // Store the target object

        public TrackedItem(string type, float elapsedTime, string status, string details, object target)
        {
            Type = type;
            ElapsedTime = elapsedTime;
            Status = status;
            Details = details;
            Target = target;
        }
    }

    static class SplitterGUILayout
    {
        static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                    BindingFlags.Static;

        static Lazy<Type> splitterStateType = new Lazy<Type>(() =>
        {
            var type = typeof(EditorWindow).Assembly.GetTypes().First(x => x.FullName == "UnityEditor.SplitterState");
            return type;
        });

        static Lazy<ConstructorInfo> splitterStateCtor = new Lazy<ConstructorInfo>(() =>
        {
            var type = splitterStateType.Value;
            return type.GetConstructor(flags, null, new Type[] {typeof(float[]), typeof(int[]), typeof(int[])}, null);
        });

        static Lazy<Type> splitterGUILayoutType = new Lazy<Type>(() =>
        {
            var type = typeof(EditorWindow).Assembly.GetTypes()
                .First(x => x.FullName == "UnityEditor.SplitterGUILayout");
            return type;
        });

        static Lazy<MethodInfo> beginVerticalSplit = new Lazy<MethodInfo>(() =>
        {
            var type = splitterGUILayoutType.Value;
            return type.GetMethod("BeginVerticalSplit", flags, null,
                new Type[] {splitterStateType.Value, typeof(GUILayoutOption[])}, null);
        });

        static Lazy<MethodInfo> endVerticalSplit = new Lazy<MethodInfo>(() =>
        {
            var type = splitterGUILayoutType.Value;
            return type.GetMethod("EndVerticalSplit", flags, null, Type.EmptyTypes, null);
        });

        public static object CreateSplitterState(float[] relativeSizes, int[] minSizes, int[] maxSizes)
        {
            return splitterStateCtor.Value.Invoke(new object[] {relativeSizes, minSizes, maxSizes});
        }

        public static void BeginVerticalSplit(object splitterState, params GUILayoutOption[] options)
        {
            beginVerticalSplit.Value.Invoke(null, new object[] {splitterState, options});
        }

        public static void EndVerticalSplit()
        {
            endVerticalSplit.Value.Invoke(null, Type.EmptyTypes);
        }
    }
}