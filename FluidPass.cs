using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FluidPass : ScriptableRenderPass {
    private Material material;
    
    private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("FluidRendering_RenderFeature");
    //private const string k_OutputName = "_FluidRT";
    //private int m_OutputId = Shader.PropertyToID(k_OutputName);
    
    private RTHandle m_CameraColorTarget;
    private RTHandle m_CopiedColor;
    
    public FluidPass(RenderPassEvent renderPassEvent, Material material) {
        this.material = material;
        this.renderPassEvent = renderPassEvent;
    }

    public void SetTarget(RTHandle colorHandle) {
        m_CameraColorTarget = colorHandle;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
        ConfigureTarget(m_CameraColorTarget);
        ReAllocate(renderingData.cameraData.cameraTargetDescriptor);
    }

    private static void ExecuteCopyColorPass(CommandBuffer cmd, RTHandle sourceTexture) {
        Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1, 1, 0, 0), 0.0f, false);
    }
    
    void ReAllocate(RenderTextureDescriptor desc) {
        desc.msaaSamples = 1;
        desc.depthBufferBits = (int)DepthBits.None;
        RenderingUtils.ReAllocateIfNeeded(ref m_CopiedColor, desc, name: "_FullscreenPassColorCopy");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        if (material == null)
            return;

        ref var cameraData = ref renderingData.cameraData;
        if (cameraData.cameraType != CameraType.Game && cameraData.cameraType != CameraType.SceneView) {
            return;
        }
        
        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, m_ProfilingSampler)) {
            CoreUtils.SetRenderTarget(cmd, m_CopiedColor);
            ExecuteCopyColorPass(cmd, cameraData.renderer.cameraColorTargetHandle);
            
            CoreUtils.SetRenderTarget(cmd, cameraData.renderer.cameraColorTargetHandle);
            Blitter.BlitCameraTexture(cmd, m_CopiedColor, m_CameraColorTarget, material, 0);
        }
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public void Dispose() {
    }
}