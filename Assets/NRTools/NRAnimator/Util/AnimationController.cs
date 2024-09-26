using System;
using System.Collections.Generic;
using NRTools.Animator.NRNodes;
using NRTools.GpuSkinning;
using UnityEngine;

namespace NRTools.CustomAnimator
{
    public class AnimationController : MonoBehaviour
    {
        public GameObject tankPrefab;
        public GameObject gruntPrefab;
        public GameObject glassCannonPrefab;
        public GameObject swarmPrefab;
        public GameObject blasterPrefab;
        
        public static AnimationController instance;
        public static string currentAnimator = "Tank";
        public static event Action OnLoaded;
        public static event Action<AnimationTransitionData, string> OnTransitionSelected;
        public static event Action<string> OnEditorAnimationChanged;
        public static event Action<List<string>> OnAnimatorChanged;
        public static event Action<AnimatorNode> OnAnimationChanged;
        public static event Action<AnimatorNode> OnTransition;
        public static bool IsLoaded;
        public static void RaiseTransitionSelected(AnimationTransitionData data, string from) => OnTransitionSelected?.Invoke(data, from);
        public static void RaiseAnimationChanged(AnimatorNode animation) => OnAnimationChanged?.Invoke(animation);
        public static void RaiseEditorAnimationChanged(string guid) => OnEditorAnimationChanged?.Invoke(guid);
        public static void RaiseTransition(AnimatorNode data) => 
            OnTransition?.Invoke(data);
        public static List<string> GetAnimations(string animator)
        {
            var anim = AnimationManager.GetAnimations(animator);
            currentAnimator = animator;
            return anim;
        }
        
        public static void RaiseAnimations(string animator)
        {
            var anim = AnimationManager.GetAnimations(animator);
            currentAnimator = animator;
            OnAnimatorChanged?.Invoke(anim);
        }

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
                case "Blaster":
                    return instance.blasterPrefab;
            }
            return instance.swarmPrefab;
        }

        public static void RaiseOnLoaded()
        {
            OnLoaded?.Invoke();
            IsLoaded = true;
        }
    }
}