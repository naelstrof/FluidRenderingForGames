using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FluidRenderingForGames {
public class FluidRenderingRendererFeature : ScriptableRendererFeature {
    private static List<FluidParticleSystem> systems = new();

    public static void AddParticleSystem(FluidParticleSystem system) {
        systems.Add(system);
    }

    public static void RemoveParticleSystem(FluidParticleSystem system) {
        systems.Remove(system);
    }

    private class FluidData : ContextItem {
        public TextureHandle heightTexture;
        public TextureHandle fluidColorTexture;

        public void Init(RenderGraph renderGraph, TextureDesc targetDescriptor) {
            var heightDesc = targetDescriptor;
            heightDesc.depthBufferBits = (int)DepthBits.None;
            heightDesc.colorFormat = GraphicsFormat.R32_SFloat;
            heightDesc.name = "_FluidHeightBuffer";
            renderGraph.CreateTextureIfInvalid(in heightDesc, ref heightTexture);
            
            var fluidDesc = targetDescriptor;
            fluidDesc.depthBufferBits = (int)DepthBits.None;
            fluidDesc.colorFormat = GraphicsFormat.R8G8B8A8_SRGB;
            fluidDesc.name = "_FluidColorBuffer";
            renderGraph.CreateTextureIfInvalid(in fluidDesc, ref fluidColorTexture);
        }

        public override void Reset() {
            heightTexture = TextureHandle.nullHandle;
            fluidColorTexture = TextureHandle.nullHandle;
        }
    }
    private class FluidHeightPass : ScriptableRenderPass {
        private const string outputName = "_FluidHeightBuffer";
        private int outputId = Shader.PropertyToID(outputName);


        public void Setup() {
            ConfigureInput(ScriptableRenderPassInput.Depth);
        }
        
        public FluidHeightPass(RenderPassEvent renderPassEvent) {
            this.renderPassEvent = renderPassEvent;
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
            using (var builder = renderGraph.AddRasterRenderPass<FluidData>("FluidRenderHeightPass", out var passData)) {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                var colorDesc = renderGraph.GetTextureDesc(resourceData.cameraOpaqueTexture);
                passData = frameData.GetOrCreate<FluidData>();
                passData.Init(renderGraph, colorDesc);
                
                // Setup as a render target via UseTextureFragment and UseTextureFragmentDepth, which are the equivalent of using the old cmd.SetRenderTarget(color,depth)
                builder.SetRenderAttachment(passData.heightTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.cameraDepthTexture, AccessFlags.Read);

                // Assign the ExecutePass function to the render pass delegate, which will be called by the render graph when executing the pass.
                builder.SetRenderFunc(static (FluidData data, RasterGraphContext context) => ExecutePass(data, context));
                builder.SetGlobalTextureAfterPass(in passData.heightTexture, outputId);
            }
        }
        
        static void ExecutePass(FluidData data, RasterGraphContext context) {
            context.cmd.ClearRenderTarget(RTClearFlags.Color, Color.black, 1,0);
            foreach (var system in systems) {
                system.RenderHeight(context.cmd);
            }
        }

    }
    
    private class FluidColorPass : ScriptableRenderPass {
        private const string outputName = "_FluidColorBuffer";
        private int outputId = Shader.PropertyToID(outputName);

        public FluidColorPass(RenderPassEvent renderPassEvent) {
            this.renderPassEvent = renderPassEvent;
        }

        public void Setup() {
            ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
            using (var builder = renderGraph.AddRasterRenderPass<FluidData>("FluidRenderColorPass", out var passData)) {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                var source = resourceData.cameraOpaqueTexture;
                var colorDesc = renderGraph.GetTextureDesc(source);
                passData = frameData.GetOrCreate<FluidData>();
                passData.Init(renderGraph, colorDesc);
                // Setup as a render target via UseTextureFragment and UseTextureFragmentDepth, which are the equivalent of using the old cmd.SetRenderTarget(color,depth)
                builder.SetRenderAttachment(passData.fluidColorTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.cameraDepthTexture, AccessFlags.Read);

                // Assign the ExecutePass function to the render pass delegate, which will be called by the render graph when executing the pass.
                builder.SetRenderFunc(static (FluidData data, RasterGraphContext context) => ExecutePass(data, context));
                builder.SetGlobalTextureAfterPass(in passData.fluidColorTexture, outputId);
            }
        }
        
        static void ExecutePass(FluidData data, RasterGraphContext context) {
            context.cmd.ClearRenderTarget(RTClearFlags.Color, Color.clear, 1,0);
            foreach (var system in systems) {
                system.RenderColor(context.cmd);
            }
        }
    }
    
    private class FluidBlitPass : ScriptableRenderPass {
        private Material material;
        
        public FluidBlitPass(RenderPassEvent renderPassEvent, Material material) {
            this.material = material;
            this.renderPassEvent = renderPassEvent;
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            if (resourceData.isActiveTargetBackBuffer) {
                Debug.LogError($"Skipping render pass. FluidRenderingRendererFeature requires an intermediate ColorTexture, we can't use the BackBuffer as a texture input.");
                return;
            }
            var source = resourceData.activeColorTexture;
            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = $"CameraColor-FluidRender";
            destinationDesc.clearBuffer = false;
            
            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);
            
            var fluidData = frameData.Get<FluidData>();
            if (fluidData == null) {
                return;
            }
            RenderGraphUtils.BlitMaterialParameters para = new(fluidData.heightTexture, destination, material, 0);

            using (var builder = renderGraph.AddBlitPass(para, passName: "FluidBlitPass", returnBuilder: true)) {
                builder.UseAllGlobalTextures(true);
            }
            resourceData.cameraColor = destination;
        }
    }

    [SerializeField] private Material fullscreenBlitMaterial;
    [SerializeField] private Texture fluidMatcap;
    
    private FluidHeightPass _fluidHeightPass;
    private FluidColorPass _fluidColorPass;
    private FluidBlitPass _fluidBlitPass;

    public override void Create() {
#if UNITY_EDITOR
        EnsureWeHaveFullscreenBlitMaterial();
        if (fullscreenBlitMaterial == null) {
            return;
        }
#endif
        _fluidHeightPass = new FluidHeightPass(RenderPassEvent.BeforeRenderingPostProcessing);
        _fluidColorPass = new FluidColorPass(RenderPassEvent.BeforeRenderingPostProcessing);
        _fluidBlitPass = new FluidBlitPass(RenderPassEvent.BeforeRenderingPostProcessing, fullscreenBlitMaterial);
        Shader.SetGlobalTexture("_FluidMatcap", fluidMatcap);
    }

#if UNITY_EDITOR
    private void EnsureWeHaveFullscreenBlitMaterial() {
        SerializedObject obj = new SerializedObject(this);
        var blitMat = obj.FindProperty(nameof(fullscreenBlitMaterial));
        if (blitMat.objectReferenceValue == null) {
            blitMat.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>( AssetDatabase.GUIDToAssetPath("e6cb23922d304c94e89fd2de80c7293a"));
            obj.ApplyModifiedPropertiesWithoutUndo();
        }
    }
#endif

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if (renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView) {
            return;
        }

        if (UniversalRenderer.IsOffscreenDepthTexture(ref renderingData.cameraData)) {
            return;
        }

        _fluidColorPass.Setup();
        _fluidHeightPass.Setup();
        renderer.EnqueuePass(_fluidColorPass);
        renderer.EnqueuePass(_fluidHeightPass);
        renderer.EnqueuePass(_fluidBlitPass);
    }


    protected override void Dispose(bool disposing) {
        _fluidHeightPass = null;
        _fluidColorPass = null;
        _fluidBlitPass = null;
    }

}

}