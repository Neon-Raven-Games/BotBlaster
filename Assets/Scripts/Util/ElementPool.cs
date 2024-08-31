using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Util;
using UnityEngine;



namespace Util
{
    [Serializable]
    public class BlasterElementMaterial
    {
        public ElementFlag elementFlag;
        public Material material;
    }

    public class ElementPool : MonoBehaviour
    {
        [SerializeField] private int elementsToPool;
        public List<ElementProjectile> elementProjectiles;
        private static ElementPool _instance;
        private readonly ConcurrentDictionary<ElementFlag, ElementProjectile> _elementProjectiles = new();

        public void Awake()
        {
            if (_instance != null)
            {
                Destroy(this);
                return;
            }

            _instance = this;
            SpawnProjectiles().Forget();
        }

        public static GameObject GetElement(ElementFlag elementFlag, Vector3 position)
        {
            Debug.Log($"element flag {elementFlag.ToString()}");
            var elementProjectile = _instance._elementProjectiles.TryGetValue(elementFlag, out var projectile)
                ? projectile
                : null;
            if (elementProjectile == null)
            {
                Debug.LogError("Element not found in pool.");
                return null;
            }

            elementProjectile.index = (elementProjectile.index + 1) % _instance.elementsToPool;
            var obj = projectile.GetObject();
            if (obj.activeInHierarchy)
            {
                obj = elementProjectile.projectiles.Find(x => !x.activeInHierarchy);
                if (obj == null)
                {
                    elementProjectile.index = 0;
                    obj = Instantiate(elementProjectile.projectile, _instance.transform).gameObject;
                    obj.SetActive(false);
                    elementProjectile.projectiles.Add(obj);
                }
            }

            obj.transform.position = position;
            return obj;
        }

        private async UniTaskVoid SpawnProjectiles()
        {
            await UniTask.DelayFrame(2);
            foreach (var elementProjectile in elementProjectiles)
            {
                for (var i = 0; i < elementsToPool; i++)
                {
                    GameObject obj = Instantiate(elementProjectile.projectile, transform);
                    await UniTask.Yield();
                    obj.SetActive(false);
                    elementProjectile.projectiles.Add(obj);
                    _elementProjectiles.TryAdd(elementProjectile.elementFlag, elementProjectile);
                    await UniTask.Yield();
                }
            }
        }
    }
}