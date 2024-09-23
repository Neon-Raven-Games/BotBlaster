using System;
using System.Collections.Generic;
using UnityEngine;

namespace NRTools.CustomAnimator
{
    public class AnimationController : MonoBehaviour
    {
        public static List<string> animators = new()
        {
            "Tank",
            "Grunt",
            "GlassCannon",
            "Swarm"
        };

        private static readonly List<string> _STankAnimations = new()
        {
            "Tank_Dash_L",
            "Tank_Dash_R",
            "Tank_HIT_L",
            "Tank_HIT_M",
            "Tank_HIT_R",
            "Tank_Idle",
            "Tank_Shooting"
        };

        private static readonly List<string> _SGruntAnimations = new()
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

        private static readonly List<string> _SGlassCannonAnimations = new()
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

        private static readonly List<string> _SSwarmAnimations = new()
        {
            "Swarm_Diving",
            "Swarm_Floating",
            "Swarm_Hit_01",
            "Swarm_Hit_02",
            "Swarm_Hit_03",
        };

        public static List<string> GetAnimations(string animator)
        {
            var anim = animator switch
            {
                "Tank" => _STankAnimations,
                "Grunt" => _SGruntAnimations,
                "GlassCannon" => _SGlassCannonAnimations,
                "Swarm" => _SSwarmAnimations,
                _ => new List<string>()
            };
            currentAnimator = animator;
            return anim;
        }
        public static void RaiseAnimations(string animator)
        {
            var anim = animator switch
            {
                "Tank" => _STankAnimations,
                "Grunt" => _SGruntAnimations,
                "GlassCannon" => _SGlassCannonAnimations,
                "Swarm" => _SSwarmAnimations,
                _ => new List<string>()
            };
            if (currentAnimator == animator) return;
            currentAnimator = animator;
            OnAnimatorChanged?.Invoke(anim);
        }

        public static event Action<List<string>> OnAnimatorChanged;
        public static event Action<string> OnAnimationChanged;
        public static void RaiseAnimationChanged(string animation) => OnAnimationChanged?.Invoke(animation);
        public static string currentAnimator = "Tank";
        
        // we need a slick way to populate these prefabs
        // Assets/Enemies/Final is ok for now
        public GameObject tankPrefab;
        public GameObject gruntPrefab;
        public GameObject glassCannonPrefab;
        public GameObject swarmPrefab;
        
        public static AnimationController instance;
        public static void SetInstance(AnimationController animController) => instance = animController;
        
        public static GameObject GetPrefab()
        {
            switch (currentAnimator)
            {
                case "Tank":
                    return instance.tankPrefab;
                case "Grunt":
                    return instance.gruntPrefab;
                case "GlassCannon":
                    return instance.glassCannonPrefab;
                case "Swarm":
                    return instance.swarmPrefab;
            }

            return instance.swarmPrefab;
        }

    }
}