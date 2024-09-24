#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// put this in an Editor folder

[CustomEditor(typeof(UIDevelopment))]
public class UIDevelopmentEditor : Editor
{
    
    private bool showContent;
    public override void OnInspectorGUI()
    {
        // foldout to show content
        showContent = EditorGUILayout.Foldout(showContent, "Something");
        if (showContent) base.OnInspectorGUI();
        
        var ui = (UIDevelopment) target;
        
        // button to call target method
        if (GUILayout.Button("OpenMenu"))
        {
            ui.OpenMenu();
        }
        
        if (GUILayout.Button("CloseMenu"))
        {
            ui.CloseMenu();
        }
    }
}
#endif