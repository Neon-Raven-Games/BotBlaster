using System.Collections.Generic;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class GruntBotAnimator : GpuMeshAnimator
    {
        private string botName = "Grunt";
        private static readonly List<string> _SAnimationNames = new()
        {
            "Grunt_Draw_Wep",
            "Grunt_Final_Hit",
            "Grunt_Hit_01",
            "Grunt_Hit_02",
            "Grunt_Hit_03",
            "Grunt_Idle",
            "Grunt_Moving",
            "Grunt_Shooting"
        };

        protected override AnimationData DeserializeAnimationData()
        {
            return AnimationManager.GetAnimationData(botName, _SAnimationNames[Random.Range(0, _SAnimationNames.Count)]);
        }
    }
}