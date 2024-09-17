using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;

public class FluidPass : ScriptableRenderPass {
    private Material material;
    private LayerMask vfxMask;
    
    private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("FluidRendering_RenderFeature");
    
    private RTHandle m_CameraColorTarget;
    private RTHandle m_FluidBuffer;
    
    public FluidPass(RenderPassEvent renderPassEvent, Material material, LayerMask vfxMask) {
        this.material = material;
        this.renderPassEvent = renderPassEvent;
        this.vfxMask = vfxMask;
    }

    public void SetTarget(RTHandle colorHandle) {
        m_CameraColorTarget = colorHandle;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
        ConfigureTarget(m_CameraColorTarget);
        ReAllocate(renderingData.cameraData.cameraTargetDescriptor);
    }

    //private static void ExecuteCopyColorPass(CommandBuffer cmd, RTHandle sourceTexture) {
        //Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1, 1, 0, 0), 0.0f, false);
    //}
    
    void ReAllocate(RenderTextureDescriptor desc) {
        desc.msaaSamples = 1;
        desc.depthBufferBits = (int)DepthBits.Depth32;
        //desc.colorFormat = RenderTextureFormat.Default;
        RenderingUtils.ReAllocateIfNeeded(ref m_FluidBuffer, desc, name: "_FluidBuffer");
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
            CoreUtils.SetRenderTarget(cmd, m_FluidBuffer);
            CoreUtils.ClearRenderTarget(cmd, ClearFlag.All, Color.black);
            if (cameraData.camera.TryGetCullingParameters(out var cullingParameters)) {
                //cullingParameters.cullingMask = (uint)vfxMask.value;
                var shaderTags = new ShaderTagId[] {
                    new ShaderTagId("DepthOnly"),
                    new ShaderTagId("UniversalForward"),
                    new ShaderTagId("UniversalForwardOnly"),
                    new ShaderTagId("LightweightForward"),
                    new ShaderTagId("SRPDefaultUnlit"),
                    new ShaderTagId("Forward")
                };
                //ShaderTagId shaderTagID = new ShaderTagId("UniversalForward");
                var cullingResults = context.Cull(ref cullingParameters);
                var desc = new RendererListDesc(shaderTags, cullingResults, cameraData.camera) {
                    renderQueueRange = RenderQueueRange.all,
                };
                var rendererList = context.CreateRendererList(desc);
                cmd.DrawRendererList(rendererList);
            }
            Blitter.BlitCameraTexture(cmd, m_FluidBuffer, m_CameraColorTarget, material, 0);
        }
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public void Dispose() {
    }
}