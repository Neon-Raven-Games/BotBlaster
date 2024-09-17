using System;
using UnityEditor;
using UnityEngine;

namespace UI
{
    [CustomEditor(typeof(EnemyHealthBar))]
    public class EnemyHealthBarEditor : Editor
    {
        
        private SerializedProperty _foreGround;
        private SerializedProperty _maxValue;
        private SerializedProperty _currentValue;
        private EnemyHealthBar _enemyHealthBar;

        public override void OnInspectorGUI()
        {
            
            serializedObject.Update();
            EditorGUILayout.PropertyField(_foreGround);
            EditorGUILayout.PropertyField(_maxValue);
            EditorGUILayout.PropertyField(_currentValue);
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Fill Max"))
            {
                _enemyHealthBar.FillMax();
            }
            
            if (GUILayout.Button("Fill Empty"))
            {
                _enemyHealthBar.FillEmpty();
            }
            
            if (GUILayout.Button("Reduce Value"))
            {
                _enemyHealthBar.ReduceValue(10);
            }
            
            if (GUILayout.Button("Increase Value"))
            {
                _enemyHealthBar.IncreaseValue(10);
            }
            
            if (GUILayout.Button("Set Value"))
            {
                _enemyHealthBar.SetValue(50);
            }
            
            if (GUILayout.Button("Set Max Value"))
            {
                _enemyHealthBar.SetMaxValue(100);
            }
        }

        private void OnEnable()
        {
            _foreGround = serializedObject.FindProperty("foreGround");
            _maxValue = serializedObject.FindProperty("maxValue");
            _currentValue = serializedObject.FindProperty("currentValue");
            _enemyHealthBar = (EnemyHealthBar) target;
        }
        
    }
}