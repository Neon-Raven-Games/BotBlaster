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
        
        private Dictionary<TankAnimation, string> animationLookup = new()
        {
            {TankAnimation.DashLeft, _SAnimationNames[0]},
            {TankAnimation.DashRight, _SAnimationNames[1]},
            {TankAnimation.HitLeft, _SAnimationNames[2]},
            {TankAnimation.HitMiddle, _SAnimationNames[3]},
            {TankAnimation.HitRight, _SAnimationNames[4]},
            {TankAnimation.Idle, _SAnimationNames[5]},
            {TankAnimation.Shooting, _SAnimationNames[6]}
        };
        
        private TankAnimation _currentAnimation;
        private TankAnimation _nextAnimation;
        
        public TankAnimation AnimationClip
        {
            get => _currentAnimation;
            set
            {
                TransitionToAnimation(FetchAnimationData(value));
                _currentAnimation = value;
            }
        }
        public TankAnimation AnimationClipNoBlend
        {
            get => _currentAnimation;
            set
            {
                SetAnimation(FetchAnimationData(value));
                _currentAnimation = value;
            }
        } 
        public void PlayIdle()
        {
            AnimationClip = TankAnimation.Idle;
        }
        
        public override void PlayOneShotHitAnimation()
        {
            if (AnimationClip is TankAnimation.HitMiddle or TankAnimation.HitLeft or TankAnimation.HitRight) 
                return;
            
            base.PlayOneShotHitAnimation();
            _nextAnimation = TankAnimation.Idle;
            var hitIndex = Random.Range(0, 3);
            var hitAnimation = TankAnimation.HitMiddle + hitIndex;
            AnimationClip = hitAnimation;
        }
        
        public override void PlayAttackAnimation()
        {
            _nextAnimation = TankAnimation.Idle;
            AnimationClip = TankAnimation.Shooting;
        }
        
        protected override void TransitionToNextAnimation()
        {
            base.TransitionToNextAnimation();
        }
        
        private AnimationData FetchAnimationData(TankAnimation animationData)
        {
            return AnimationManager.GetAnimationData(botName, animationLookup[animationData]);
        }
        
        protected override AnimationData InitialAnimation()
        {
            return FetchAnimationData(TankAnimation.Shooting);
        }
    }
}