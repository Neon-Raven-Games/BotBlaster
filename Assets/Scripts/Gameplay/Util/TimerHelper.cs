using System;
using UnityEngine;

namespace Gameplay.Util
{
    public class TimerHelper : MonoBehaviour
    {
        public void Update() => TimerManager.Update();

        public void OnDisable() => TimerManager.ClearTimers();
    }
}