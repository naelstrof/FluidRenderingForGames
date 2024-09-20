using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FluidRenderingRendererFeature : ScriptableRendererFeature {
    [SerializeField] private Shader shader;
    [SerializeField] private Shader overrideShader;
    [SerializeField] private Texture fluidMatcap;
    [SerializeField] private LayerMask fluidVFXLayerMask;
    private Material _material;
    private FluidPass _fluidPass;
    
    public override void Create() {
        if (shader == null) {
            return;
        }
        _material = CoreUtils.CreateEngineMaterial(shader);

        uint fluidVFXMask = 0;
        uint fluidSplatMask = 0;
        var maskNames = GraphicsSettings.currentRenderPipeline.renderingLayerMaskNames;
        for (int i = 0; i < maskNames.Length; i++) {
            if (maskNames[i] == "FluidVFX") {
                fluidVFXMask = (uint)(1 << i);
            }

            if (maskNames[i] == "FluidSplats") {
                fluidSplatMask = (uint)(1 << i);
            }
        }

        if (fluidVFXMask == 0) {
            Debug.LogError("Couldn't find rendering layer FluidVFX, please add it to the Rendering Layer Mask list.");
        }
        
        if (fluidSplatMask == 0) {
            Debug.LogError("Couldn't find rendering layer FluidSplats, please add it to the Rendering Layer Mask list.");
        }

        _fluidPass = new FluidPass(RenderPassEvent.BeforeRenderingPostProcessing, _material, fluidVFXMask, fluidVFXLayerMask, fluidSplatMask, overrideShader, fluidMatcap);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData) {
        if (renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView) {
            return;
        }
        _fluidPass.ConfigureInput(ScriptableRenderPassInput.Color);
        _fluidPass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if (renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView) {
            return;
        }
        renderer.EnqueuePass(_fluidPass);
    }
    

    protected override void Dispose(bool disposing) {
        CoreUtils.Destroy(_material);
        _fluidPass?.Dispose();
        _fluidPass = null;
    }
    
}