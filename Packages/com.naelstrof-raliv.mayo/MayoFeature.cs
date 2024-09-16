using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LensFlareRendererFeature : ScriptableRendererFeature
{
    class LensFlarePass : ScriptableRenderPass
    {
        private Material _material;
        private Mesh _mesh;

        public LensFlarePass(Material material, Mesh mesh)
        {
            _material = material;
            _mesh = mesh;
        }

        public override void Execute(ScriptableRenderContext context,
            ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(name: "LensFlarePass");
            cmd.DrawMesh(_mesh, Matrix4x4.identity, _material);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    private LensFlarePass _lensFlarePass;
    public Material material;
    public Mesh mesh;

    public override void Create()
    {
        _lensFlarePass = new LensFlarePass(material, mesh);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        if (material != null && mesh != null)
        {
            renderer.EnqueuePass(_lensFlarePass);
        }
    }
}