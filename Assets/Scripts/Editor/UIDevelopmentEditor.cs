#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIDevelopment))]
public class UIDevelopmentEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var ui = (UIDevelopment) target;
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