using UnityEngine;

namespace NRTools.GpuSkinning.Blasters
{
    public class FirstBlasterGpuAnimator : GpuMeshAnimator
    {
        BlasterAnimations _currentAnimation;
        BlasterAnimations _nextAnimation;

        private BlasterAnimations AnimationClip
        {
            get => _currentAnimation;
            set
            {
                _currentAnimation = value;
                SetAnimation(FetchAnimationData(value));
            }
        }

        protected override void TransitionToNextAnimation()
        {
            if (FetchAnimationData(AnimationClip).loop)
            {
                base.TransitionToNextAnimation();
                AnimationClip = _nextAnimation;
            }
        }

        public override void PlayOneShotHitAnimation()
        {
            base.PlayOneShotHitAnimation();
            AnimationClip = BlasterAnimations.Shoot;
        }
        private AnimationData FetchAnimationData(BlasterAnimations animationData)
        {
            return AnimationManager.GetAnimationData("Blaster", "Ball Launch");
        }
        
        protected override AnimationData InitialAnimation()
        {
            return FetchAnimationData(BlasterAnimations.Shoot);
        }
    }
}