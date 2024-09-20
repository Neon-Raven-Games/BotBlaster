using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Enemies.EnemyBehaviors.Base;
using UnityEngine;
using Random = UnityEngine.Random;

// Scripted events:
// Military Planes:
// The glass cannons spawn in under stage, then they jump straight up, hover a bit
// then a tank slams down in the middle. the glass cannons start moving towards the player
// offset from eachother

// SideSwarm Barrage:
// the swarms spawn off either side of the stage
// then they 'hop' over the wall and close in on the player

// Tank Rush:
// the tanks spawn in on the left and right side of the stage
// then they charge towards the player, criss crossing eachother

namespace Gameplay.Enemies
{
    public enum TransitionType
    {
        Teleport,
        UnderStageBezier,
        SideBezier,
        GlassCannonZip,
        GlassCannonKamakze
    }

    public struct IntroData
    {
        public TransitionType transitionType;
        public Vector3 startPosition;
        public Vector3 controlPoint;
        public Vector3 endPosition;
        public float time;
    }

    public class IntroController : MonoBehaviour
    {
        public Transform spawnUnderStage;
        public Transform spawnLeftSide;
        public Transform spawnRightSide;
        public Transform spawnOnStage;

        public float introDuration = 2f;
        private bool _introFinished;
        private readonly ConcurrentDictionary<Enemy, IntroData> _enemyIntro = new();
        private readonly List<Enemy> _toRemove = new();

        private static IntroController _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        public static void StartIntro(Enemy enemy)
        {
            switch (enemy.enemyType)
            {
                case EnemyType.Tank:
                case EnemyType.Grunt:
                    _instance.TeleportIntro(enemy);
                    break;
                case EnemyType.Swarm:
                case EnemyType.GlassCannon:
                    var belowData = _instance.GenerateBelowSpawnPath();
                    Debug.Log($"Created spawn path: {belowData.startPosition}");
                    Debug.DrawLine(belowData.startPosition, belowData.controlPoint, Color.red, 5f);
                    _instance._enemyIntro.TryAdd(enemy, belowData);
                    enemy.gameObject.SetActive(true);
                    break;
                default:
                    var sideData = _instance.GenerateSideSpawnPath();
                    _instance._enemyIntro.TryAdd(enemy, sideData);
                    enemy.gameObject.SetActive(true);
                    break;
            }
        }

        private IntroData GenerateBelowSpawnPath()
        {
            var introData = new IntroData
            {
                transitionType = TransitionType.UnderStageBezier,
                startPosition = GetValidPosition(spawnUnderStage),
                endPosition = GetValidPosition(spawnOnStage),
                time = introDuration,
            };
            introData.controlPoint = (introData.startPosition + introData.endPosition) / 2 + Vector3.up * 20f +
                                     Vector3.left * 5;
            return introData;
        }

        private IntroData GenerateSideSpawnPath()
        {
            var sideSpawn = Random.value > 0.5f ? spawnLeftSide : spawnRightSide;

            var introData = new IntroData
            {
                transitionType = TransitionType.UnderStageBezier,
                startPosition = GetValidPosition(sideSpawn),
                endPosition = GetValidPosition(spawnOnStage),
                time = introDuration,
            };
            introData.controlPoint = (introData.startPosition + introData.endPosition) / 2 + Vector3.up * 5f;
            return introData;
        }

        private void Update()
        {
            if (_enemyIntro.IsEmpty) return;

            foreach (var enemyData in _enemyIntro)
            {
                var enemy = enemyData.Value;
                enemy.time -= Time.deltaTime;
                if (enemy.time <= 0f)
                {
                    _toRemove.Add(enemyData.Key);
                }
                else
                {
                    switch (enemy.transitionType)
                    {
                        case TransitionType.Teleport:
                            enemyData.Key.transform.position = Vector3.Lerp(enemy.startPosition, enemy.endPosition,
                                1 - enemy.time / introDuration);
                            break;
                        case TransitionType.UnderStageBezier:

                            var t = (introDuration - enemy.time) / introDuration;
                            var point1 = Vector3.Lerp(enemy.startPosition, enemy.controlPoint, t);
                            var point2 = Vector3.Lerp(enemy.controlPoint, enemy.endPosition, t);
                            enemyData.Key.transform.position = Vector3.Lerp(point1, point2, t);
                            break;
                    }
                }

                _enemyIntro[enemyData.Key] = enemy;
            }

            foreach (var enemy in _toRemove)
            {
                if (!_enemyIntro.TryRemove(enemy, out _))
                {
                    Debug.LogWarning($"Failed to remove enemy {enemy.name} from the dictionary.");
                    continue;
                }

                enemy.FinishIntro();
            }

            _toRemove.Clear();
        }

        private void TeleportIntro(Enemy enemy)
        {
            enemy.transform.position = GetValidPosition(spawnOnStage);
            PlayTeleportVFX(enemy).Forget();
        }

        private Vector3 GetValidPosition(Transform spawnArea)
        {
            return new Vector3(Random.Range(spawnArea.position.x - spawnArea.localScale.x / 2,
                    spawnArea.position.x + spawnArea.localScale.x / 2), spawnArea.position.y,
                Random.Range(spawnArea.position.z - spawnArea.localScale.z / 2,
                    spawnArea.position.z + spawnArea.localScale.z / 2));
        }

        private async UniTaskVoid PlayTeleportVFX(Enemy enemy, int delay = 200)
        {
            await UniTask.Delay(delay);
            enemy.gameObject.SetActive(true);
            enemy.FinishIntro();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if (spawnOnStage) DrawGizmoBounds(spawnOnStage);
            Gizmos.color = Color.red;
            if (spawnUnderStage) DrawGizmoBounds(spawnUnderStage);
            Gizmos.color = Color.blue;
            if (spawnLeftSide) DrawGizmoBounds(spawnLeftSide);
            Gizmos.color = Color.green;
            if (spawnRightSide) DrawGizmoBounds(spawnRightSide);
        }

        private static void DrawGizmoBounds(Transform spawnArea)
        {
            if (spawnArea == null) return;
            Gizmos.DrawCube(spawnArea.position, spawnArea.localScale);
        }
    }
}