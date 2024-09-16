using System.Collections.Generic;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class GlassCannonAnimator : GpuMeshAnimator
    {
        private string botName = "GlassCannon";
        private static readonly List<string> _SAnimationNames = new()
        {
            "GCannon_Dash_L",
            "GCannon_Dash_R",
            "GCannon_Fly_L",
            "GCannon_Fly_R",
            "GCannon_Hit_L",
            "GCannon_Hit_R",
            "GCannon_Idle",
            "GCannon_Shoot"
        };

        protected override AnimationData DeserializeAnimationData()
        {
            return AnimationManager.GetAnimationData(botName, _SAnimationNames[Random.Range(0, _SAnimationNames.Count)]);
        }

    }
}