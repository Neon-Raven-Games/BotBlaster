using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SharpenEffectRenderFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        public Material sharpenMaterial;
        private RTHandle source;
        private RTHandle tempTexture;
        private RenderTextureDescriptor cameraTextureDescriptor;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Get the camera's render texture descriptor
            cameraTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;

            if (renderingData.cameraData.camera.stereoEnabled)
            {
                cameraTextureDescriptor.width *= 2;
                cameraTextureDescriptor.msaaSamples = 1; // Disable MSAA in this pass
            }

            // Try using cameraColorTarget directly if cameraColorTargetHandle is null or uninitialized
            if (renderingData.cameraData.renderer.cameraColorTargetHandle == null || renderingData.cameraData.renderer.cameraColorTargetHandle.rt == null)
            {
                Debug.LogWarning("cameraColorTargetHandle is not valid. Falling back to cameraColorTarget.");

                // does not work
                // source = renderingData.cameraData.renderer.cameraColorTarget;
            }
            else
            {
                source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            }

            if (source == null)
            {
                Debug.LogError("Source render target is null!");
            }
            else if (source.rt == null)
            {
                Debug.LogError("Source RTHandle is null!");
            }
        }


        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cameraTextureDescriptor.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, cameraTextureDescriptor, FilterMode.Bilinear, name: "_TemporaryColorTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (sharpenMaterial == null)
            {
                Debug.LogError("Missing Sharpen Material");
                return;
            }

            if (source == null || !source.rt.IsCreated())
            {
                Debug.LogError("Source RTHandle is not valid or not created!");
                return;
            }

            if (tempTexture == null || !tempTexture.rt.IsCreated())
            {
                Debug.LogError("TempTexture RTHandle is not valid or not created!");
                return;
            }
            CommandBuffer cmd = CommandBufferPool.Get("SharpenEffect");
            cmd.Blit(source.rt, tempTexture.rt, sharpenMaterial);
            cmd.Blit(tempTexture.rt, source.rt); 

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (tempTexture != null)
            {
                tempTexture.Release();
                tempTexture = null;
            }
        }
    }

    [System.Serializable]
    public class SharpenSettings
    {
        public Material sharpenMaterial = null;
    }

    public SharpenSettings settings = new SharpenSettings();
    CustomRenderPass customRenderPass;

    public override void Create()
    {
        customRenderPass = new CustomRenderPass
        {
            sharpenMaterial = settings.sharpenMaterial,
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.sharpenMaterial != null)
        {
            renderer.EnqueuePass(customRenderPass);
        }
    }
}
