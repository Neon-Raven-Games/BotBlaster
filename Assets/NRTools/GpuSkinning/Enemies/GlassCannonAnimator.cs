using System.Collections.Generic;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class GlassCannonAnimator : GpuMeshAnimator
    {
        private const string _BOT_NAME = "GlassCannon";

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
        
        private static readonly Dictionary<GlassCannonAnimation, string> animationLookup = new()
        {
            {GlassCannonAnimation.DashLeft, _SAnimationNames[0]},
            {GlassCannonAnimation.DashRight, _SAnimationNames[1]},
            {GlassCannonAnimation.FlyLeft, _SAnimationNames[2]},
            {GlassCannonAnimation.FlyRight, _SAnimationNames[3]},
            {GlassCannonAnimation.HitLeft, _SAnimationNames[4]},
            {GlassCannonAnimation.HitRight, _SAnimationNames[5]},
            {GlassCannonAnimation.Idle, _SAnimationNames[6]},
            {GlassCannonAnimation.Shoot, _SAnimationNames[7]}
        };

        private GlassCannonAnimation _currentAnimation;
        private GlassCannonAnimation _nextAnimation;
        public GlassCannonAnimation AnimationClip
        {
            get => _currentAnimation;
            set
            {
                TransitionToAnimation(FetchAnimationData(value));
                _currentAnimation = value;
            }
        }

        public override void PlayAttackAnimation()
        {
            _nextAnimation = GlassCannonAnimation.Idle;
            AnimationClip = GlassCannonAnimation.Shoot;
        }

        public void PlayDashAnimation(bool isLeft)
        {
            Debug.Log("Dashing animation");
            // _nextAnimation = isLeft ? GlassCannonAnimation.FlyLeft : GlassCannonAnimation.FlyRight;
            AnimationClip = isLeft ? GlassCannonAnimation.DashLeft : GlassCannonAnimation.DashRight;
        }
        
        public override void PlayOneShotHitAnimation()
        {
            base.PlayOneShotHitAnimation();
            if (AnimationClip == GlassCannonAnimation.HitLeft || AnimationClip == GlassCannonAnimation.HitRight)
            {
                return;
            }
            
            _nextAnimation = GlassCannonAnimation.Idle;
            var hitIndex = Random.Range(0, 2);
            var hitAnimation = GlassCannonAnimation.HitLeft + hitIndex;
            SetAnimation(FetchAnimationData(hitAnimation));
        } 
        
        protected override void TransitionToNextAnimation()
        {
            
            base.TransitionToNextAnimation();
        }

        private AnimationData FetchAnimationData(GlassCannonAnimation animationData)
        {
            return AnimationManager.GetAnimationData(_BOT_NAME, animationLookup[animationData]);
        }
        
        protected override AnimationData InitialAnimation()
        {
            _nextAnimation = GlassCannonAnimation.Idle;
            return FetchAnimationData(GlassCannonAnimation.Idle);
        }

        public void PlayIdle()
        {
            if (_currentAnimation == GlassCannonAnimation.Idle) return;
            AnimationClip = GlassCannonAnimation.Idle;
        }
    }
}