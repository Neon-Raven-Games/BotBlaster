using System;
using System.Collections.Generic;
using NRTools.Animator.NRNodes;
using NRTools.GpuSkinning;
using UnityEngine;

namespace NRTools.CustomAnimator
{
    
    // todo,
    // this is for the editor code
    // we can populate this stuff with our lookup data, but this is to have finer control during debugging
    public class AnimationController : MonoBehaviour
    {
        // todo, we need to have some data that lets us set up all that animation jazz
        public GameObject tankPrefab;
        public GameObject gruntPrefab;
        public GameObject glassCannonPrefab;
        public GameObject swarmPrefab;
        public GameObject blasterPrefab;
        
        public static AnimationController instance;
        
        public static string currentAnimator = "Tank";
        private static event Action onLoaded;
        public static event Action OnLoaded
        {
            add
            {
                onLoaded += value;
            }
            remove
            {
                onLoaded -= value;
            }
        }

        public static event Action<AnimationTransitionData> OnTransitionSelected;
        public static event Action<List<string>> OnAnimatorChanged;
        public static event Action<AnimatorNode> OnAnimationChanged;
        public static event Action<AnimatorNode> OnTransition;
        public static bool IsLoaded;
        public static void RaiseTransitionSelected(AnimationTransitionData data) => OnTransitionSelected?.Invoke(data);
        public static void RaiseAnimationChanged(AnimatorNode animation) => OnAnimationChanged?.Invoke(animation);
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
            onLoaded?.Invoke();
            IsLoaded = true;
        }
    }
}