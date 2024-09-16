using System.Collections.Generic;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class SwarmUnitAnimator : GpuMeshAnimator
    {
        private string botName = "Swarm";
        private static readonly List<string> _SAnimationNames = new()
        {
            "Swarm_Diving",
            "Swarm_Floating",
            "Swarm_Hit_01",
            "Swarm_Hit_02",
            "Swarm_Hit_03",
        };
        
        private Dictionary<SwarmBotAnimation, string> animationLookup = new()
        {
            {SwarmBotAnimation.Diving, _SAnimationNames[0]},
            {SwarmBotAnimation.Floating, _SAnimationNames[1]},
            {SwarmBotAnimation.Hit01, _SAnimationNames[2]},
            {SwarmBotAnimation.Hit02, _SAnimationNames[3]},
            {SwarmBotAnimation.Hit03, _SAnimationNames[4]},
        };
        
                
        private SwarmBotAnimation _currentAnimation;
        private SwarmBotAnimation _nextAnimation;
        
        public SwarmBotAnimation AnimationClip
        {
            get => _currentAnimation;
            set
            {
                _currentAnimation = value;
                SetAnimation(FetchAnimationData(value));
            }
        }
        
        public override void PlayOneShotHitAnimation()
        {
            base.PlayOneShotHitAnimation();
            _nextAnimation = SwarmBotAnimation.Floating;
            var hitIndex = Random.Range(0, 3);
            var hitAnimation = SwarmBotAnimation.Hit01 + hitIndex;
            AnimationClip = hitAnimation;
        }
        
        public override void PlayAttackAnimation()
        {
            _nextAnimation = SwarmBotAnimation.Diving;
            AnimationClip = SwarmBotAnimation.Diving;
        }
        
        protected override void TransitionToNextAnimation()
        {
            base.TransitionToNextAnimation();
            AnimationClip = _nextAnimation;
        }
        private AnimationData FetchAnimationData(SwarmBotAnimation animationData)
        {
            return AnimationManager.GetAnimationData(botName, animationLookup[animationData]);
        }
        
        protected override AnimationData InitialAnimation()
        {
            return FetchAnimationData(SwarmBotAnimation.Floating);
        }
    }
}