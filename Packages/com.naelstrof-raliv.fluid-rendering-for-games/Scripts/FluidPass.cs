using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FluidRenderingForGames {
public class FluidPass : ScriptableRenderPass {
    private Material material;

    private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("FluidRendering_RenderFeature");

    private RTHandle m_CameraColorTarget;
    private RTHandle m_CameraDepthTarget;
    private RTHandle m_FluidHeightBuffer;
    private RTHandle m_FluidColorBuffer;

    private const string outputColorName = "_FluidColorBuffer";
    private int outputColorId = Shader.PropertyToID(outputColorName);

    private static List<FluidParticleSystem> systems = new();

    public FluidPass(RenderPassEvent renderPassEvent, Material material) {
        this.material = material;
        this.renderPassEvent = renderPassEvent;
    }

    public static void AddParticleSystem(FluidParticleSystem system) {
        systems.Add(system);
    }

    public static void RemoveParticleSystem(FluidParticleSystem system) {
        systems.Remove(system);
    }

    public void SetTarget(RTHandle colorHandle, RTHandle depthHandle) {
        m_CameraColorTarget = colorHandle;
        m_CameraDepthTarget = depthHandle;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
        ConfigureTarget(m_CameraColorTarget);
        ReAllocate(renderingData.cameraData.cameraTargetDescriptor);
    }

    void ReAllocate(RenderTextureDescriptor desc) {
        desc.depthBufferBits = (int)DepthBits.None;
        desc.colorFormat = RenderTextureFormat.RFloat;
        desc.sRGB = false;
        RenderingUtils.ReAllocateIfNeeded(ref m_FluidHeightBuffer, desc, name: "_FluidHeightBuffer");
        
        desc.depthBufferBits = (int)DepthBits.None;
        desc.colorFormat = RenderTextureFormat.Default;
        desc.sRGB = true;
        RenderingUtils.ReAllocateIfNeeded(ref m_FluidColorBuffer, desc, name: outputColorName);
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
            CoreUtils.SetRenderTarget(cmd, m_FluidHeightBuffer, m_CameraDepthTarget);
            CoreUtils.ClearRenderTarget(cmd, ClearFlag.Color, Color.black);
            foreach (var system in systems) {
                system.RenderHeight(cmd);
            }

            CoreUtils.SetRenderTarget(cmd, m_FluidColorBuffer, m_CameraDepthTarget);
            CoreUtils.ClearRenderTarget(cmd, ClearFlag.Color, Color.clear);
            foreach (var system in systems) {
                system.RenderColor(cmd);
            }

            cmd.SetGlobalTexture(outputColorId, m_FluidColorBuffer);
            Blitter.BlitCameraTexture(cmd, m_FluidHeightBuffer, m_CameraColorTarget, material, 0);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public void Dispose() {
    }
}

}