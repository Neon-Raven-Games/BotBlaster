using UnityEditor;
using UnityEngine;
using Gameplay.Enemies;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Blaster
{
    [CustomEditor(typeof(EnemyPool))]
    public class EnemyPoolEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EnemyPool pool = (EnemyPool) target;
            if (GUILayout.Button("Spawn Glass Cannon"))
            {
                pool.TestSpawnGlassCannon();
            }
        }
    }
}