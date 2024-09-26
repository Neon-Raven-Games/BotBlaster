using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using GraphProcessor;
using NRTools.Animator.NRNodes;
using NRTools.GpuSkinning;

public class AnimationWindow : BaseGraphWindow
{
    CustomToolbarView _toolbarView;

    [MenuItem("Development/Animation Graph")]
    public static AnimationWindow OpenWithLoadedGraph()
    {
        var graphWindow = GetWindow<AnimationWindow>();
        var graph = AssetDatabase.LoadAssetAtPath<AnimationGraph>("Assets/animations.asset");

        if (graph == null)
        {
            graph = CreateInstance<AnimationGraph>();
            AssetDatabase.CreateAsset(graph, "Assets/animations.asset");
            AssetDatabase.SaveAssets();
        }

        graphWindow.InitializeGraph(graph);

        return graphWindow;
    }


    protected override void OnDisable()
    {
        EditorUtility.SetDirty(graph);
        AssetDatabase.SaveAssets();
        base.OnDisable();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        graphView?.Dispose();
    }

    protected override void InitializeWindow(BaseGraph graph)
    {
        titleContent = new GUIContent("animation graph");

        graphView = new AnimationGraphView(this);
        graphView.Initialize(graph);
        _toolbarView = new CustomToolbarView(graphView);
        graphView.Add(_toolbarView);

        rootView.Add(graphView);
    }



    protected override void InitializeGraphView(BaseGraphView view)
    {
        base.InitializeGraphView(view);
        view.SyncSerializedPropertyPathes();
        // graphView.OpenPinned<ExposedParameterView>();
        _toolbarView.UpdateButtonStatus();
    }


    public void FocusOnNode(string nodeGUID)
    {
        var nodeView = graphView.nodeViews.FirstOrDefault(view => view.nodeTarget.GUID == nodeGUID);

        if (nodeView != null)
        {
            graphView.ClearSelection();
            graphView.AddToSelection(nodeView);
            graphView.FrameSelection();
        }
        else
        {
            Debug.LogWarning("Node not found in the graph");
        }
    }
}