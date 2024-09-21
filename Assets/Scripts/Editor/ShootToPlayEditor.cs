#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShootToPlay))]
public class ShootToPlayEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var shootToPlay = (ShootToPlay) target;
        if (GUILayout.Button("Start Waves"))
        {
            shootToPlay.StartWaves();
        }
    }
}
#endif