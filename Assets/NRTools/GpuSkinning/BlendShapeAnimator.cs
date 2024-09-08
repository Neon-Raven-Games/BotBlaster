using UnityEngine;

namespace NRTools.GpuSkinning
{
    public class BlendShapeAnimator : MonoBehaviour
    {
        public Material material; // The material with the blend shape animation shader
        public float animationSpeed = 1.0f;
        public int numFrames = 100; // The number of frames in the baked animation

        private float currentFrame = 0;

        void Update()
        {
            // Increment the frame based on the animation speed and frame count
            currentFrame += Time.deltaTime * animationSpeed;
            if (currentFrame >= numFrames)
                currentFrame = 0;

            // Pass the current frame index to the shader
            material.SetFloat("_FrameIndex", currentFrame);
        }
    }
}