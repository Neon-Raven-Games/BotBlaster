using System;
using System.Collections.Generic;
using Gameplay.Enemies;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gameplay.Util
{
    [Serializable]
    public class ElementProjectile
    {
        public ElementFlag elementFlag;
        public GameObject projectile;
        [HideInInspector] public int index;
        [HideInInspector] public List<GameObject> projectiles;

        public GameObject GetObject()
        {
            if (projectiles.Count < index) return Object.Instantiate(projectile);
            return projectiles[index];
        }
    }
}