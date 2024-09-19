using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;

public class FluidPass : ScriptableRenderPass {
    private Material material;
    private Shader overrideShader;
    private LayerMask fluidVFXMask;
    private LayerMask fluidSplatMask;
    
    private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("FluidRendering_RenderFeature");
    
    private RTHandle m_CameraColorTarget;
    private RTHandle m_CameraDepthTarget;
    private RTHandle m_FluidBuffer;
    
    public FluidPass(RenderPassEvent renderPassEvent, Material material, LayerMask fluidVFXMask, LayerMask fluidSplatMask, Shader overrideShader) {
        this.material = material;
        this.overrideShader = overrideShader;
        this.renderPassEvent = renderPassEvent;
        this.fluidVFXMask = fluidVFXMask;
        this.fluidSplatMask = fluidSplatMask;
    }

    public void SetTarget(RTHandle colorHandle, RTHandle depthHandle) {
        m_CameraColorTarget = colorHandle;
        m_CameraDepthTarget = depthHandle;
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
        desc.depthBufferBits = (int)DepthBits.None;
        desc.colorFormat = RenderTextureFormat.Default;
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
            var shaderTags = new ShaderTagId[] {
                new("UniversalForward"),
                new("UniversalForwardOnly"),
                new("LightweightForward"),
                new("SRPDefaultUnlit"),
                new("Forward")
            };
            
            if (cameraData.camera.TryGetCullingParameters(out var cullingParameters)) {
                
                CoreUtils.SetRenderTarget(cmd, m_FluidBuffer, m_CameraDepthTarget);
                CoreUtils.ClearRenderTarget(cmd, ClearFlag.Color, Color.black);
                { // FLUID VFX PASS
                    cullingParameters.cullingMask = (uint)fluidVFXMask.value;
                    var cullingResults = context.Cull(ref cullingParameters);
                    var desc = new RendererListDesc(shaderTags, cullingResults, cameraData.camera) {
                        renderQueueRange = RenderQueueRange.all,
                        sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags,
                        layerMask = fluidVFXMask,
                    };
                    var rendererList = context.CreateRendererList(desc);
                    cmd.DrawRendererList(rendererList);
                }
                Blitter.BlitCameraTexture(cmd, m_FluidBuffer, m_CameraColorTarget, material, 0);
                
                CoreUtils.SetRenderTarget(cmd, m_FluidBuffer, m_CameraDepthTarget);
                CoreUtils.ClearRenderTarget(cmd, ClearFlag.Color, Color.black);
                { // FLUID SPLAT PASS
                    var desc = new RendererListDesc(shaderTags, renderingData.cullResults, cameraData.camera) {
                        overrideShader = overrideShader,
                        renderQueueRange = RenderQueueRange.all,
                        sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags,
                        layerMask = fluidSplatMask,
                    };
                    var rendererList = context.CreateRendererList(desc);
                    cmd.DrawRendererList(rendererList);
                }
                Blitter.BlitCameraTexture(cmd, m_FluidBuffer, m_CameraColorTarget, material, 0);
                
            }

        }
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public void Dispose() {
    }
}