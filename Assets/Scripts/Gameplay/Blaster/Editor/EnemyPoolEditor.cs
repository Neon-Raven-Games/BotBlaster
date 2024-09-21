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
                EnemyPool.TestSpawnGlassCannon();
            }
            if (GUILayout.Button("Spawn Tank"))
            {
                EnemyPool.TestSpawnTank();
            }
            if (GUILayout.Button("Spawn Swarm"))
            {
                EnemyPool.TestSpawnSwarm();
            }
            if (GUILayout.Button("Spawn Grunt"))
            {
                EnemyPool.TestSpawnGrunt();
            }
        }
    }
}