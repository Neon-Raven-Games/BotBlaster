using System.Collections.Generic;
using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class GruntBotAnimator : GpuMeshAnimator
    {
        private string botName = "Grunt";
        private GruntBotAnimation _nextAnimation;
        private GruntBotAnimation _currentAnimation;
        
        private GruntBotAnimation AnimationClip
        {
            get => _currentAnimation;
            set
            {
                _currentAnimation = value;
                SetAnimation(FetchAnimationData(value));
            }
        }
        
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

        private Dictionary<GruntBotAnimation, string> animationLookup = new()
        {
            {GruntBotAnimation.DrawWeapon, _SAnimationNames[0]},
            {GruntBotAnimation.FinalHit, _SAnimationNames[1]},
            {GruntBotAnimation.Hit01, _SAnimationNames[2]},
            {GruntBotAnimation.Hit02, _SAnimationNames[3]},
            {GruntBotAnimation.Hit03, _SAnimationNames[4]},
            {GruntBotAnimation.Idle, _SAnimationNames[5]},
            {GruntBotAnimation.Moving, _SAnimationNames[6]},
            {GruntBotAnimation.Shooting, _SAnimationNames[7]}
        };

        public override void PlayAttackAnimation()
        {
            _nextAnimation = GruntBotAnimation.Moving;
            AnimationClip = GruntBotAnimation.Shooting;
        }

        public override void PlayOneShotHitAnimation()
        {
            base.PlayOneShotHitAnimation();
            _nextAnimation = GruntBotAnimation.Moving;
            var hitIndex = Random.Range(0, 3);
            var hitAnimation = GruntBotAnimation.Hit01 + hitIndex;
            AnimationClip = hitAnimation;
        }

        public void PlayIdle()
        {
            if (AnimationClip == GruntBotAnimation.Idle) return;
            _nextAnimation = GruntBotAnimation.Idle;
            AnimationClip = GruntBotAnimation.Idle;
        }

        protected override void TransitionToNextAnimation()
        {
            base.TransitionToNextAnimation();
            AnimationClip = _nextAnimation;
        }

        protected AnimationData FetchAnimationData(GruntBotAnimation animationData)
        {
            return AnimationManager.GetAnimationData(botName, animationLookup[animationData]);
        }

        protected override AnimationData InitialAnimation()
        {
            _nextAnimation = GruntBotAnimation.Moving;
            return FetchAnimationData(GruntBotAnimation.Moving);
        }
    }
}