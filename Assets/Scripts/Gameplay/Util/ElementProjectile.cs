using System;
using System.Collections.Generic;
using UnityEngine;

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
            return projectiles[index];
        }
    }
}