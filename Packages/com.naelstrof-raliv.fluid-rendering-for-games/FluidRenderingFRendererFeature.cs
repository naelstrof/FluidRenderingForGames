using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FluidRenderingRendererFeature : ScriptableRendererFeature {
    
    class FluidPass : ScriptableRenderPass {
        private Material material;
        private Material material2;
        private Mesh _mesh;
        private static readonly ShaderTagId ShaderTagID = new ShaderTagId(nameof(FluidPass));
        private static readonly int tempRTPropertyID = Shader.PropertyToID("_FluidRT");
        private static readonly int grabTexturePropertyID = Shader.PropertyToID("_MyGrabTexture");
        
        private RenderTextureDescriptor textureDescriptor;
        private RTHandle textureHandle;
        
        public FluidPass(Material material, Material material2) {
            this.material = material;
            this.material2 = material2;
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            textureDescriptor = new RenderTextureDescriptor(Screen.width,
                Screen.height, RenderTextureFormat.Default, 0);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
            textureDescriptor.width = cameraTextureDescriptor.width;
            textureDescriptor.height = cameraTextureDescriptor.height;
            RenderingUtils.ReAllocateIfNeeded(ref textureHandle, textureDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            var cmd = CommandBufferPool.Get();
            RTHandle cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            Blit(cmd, cameraTargetHandle, textureHandle, material);
            Blit(cmd, textureHandle, cameraTargetHandle, material);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            //var drawingSettings =
            //    CreateDrawingSettings(ShaderTagID, ref renderingData, SortingCriteria.CommonTransparent);
            //var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            //context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
        }

        private void Dispose() {
            
#if UNITY_EDITOR
            if (EditorApplication.isPlaying) {
                Destroy(material);
            } else {
                DestroyImmediate(material);
            }
#else
            Object.Destroy(material);
#endif

            if (textureHandle != null) textureHandle.Release();
            
        }
    }

    [SerializeField] private Material material;
    [SerializeField] private Material material2;
    private FluidPass _fluidPass;

    public override void Create() {
        _fluidPass = new FluidPass(material, material2);
        _fluidPass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(_fluidPass);
    }

    protected override void Dispose(bool disposing) {
        
        //CoreUtils.Destroy(material);
    }
    
}