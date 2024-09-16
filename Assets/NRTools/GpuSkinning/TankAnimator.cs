using System.Collections.Generic;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class TankAnimator : GpuMeshAnimator
    {
        private string botName = "Tank";
        private static readonly List<string> _SAnimationNames = new()
        {
            "Tank_Dash_L",
            "Tank_Dash_R",
            "Tank_HIT_L",
            "Tank_HIT_M",
            "Tank_HIT_R",
            "Tank_Idle",
            "Tank_Shooting",
        };
        
        protected override AnimationData DeserializeAnimationData()
        {
            if (botName == "Tank")
            {
                Debug.Log(_SAnimationNames[0]);
            }
            return AnimationManager.GetAnimationData(botName, 
                _SAnimationNames[Random.Range(0, _SAnimationNames.Count)]);
        }
    }
}