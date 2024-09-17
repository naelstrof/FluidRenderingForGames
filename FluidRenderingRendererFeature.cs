using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FluidRenderingRendererFeature : ScriptableRendererFeature {
    [SerializeField] private Shader shader;
    private Material _material;
    private FluidPass _fluidPass;
    
    public override void Create() {
        if (shader == null) {
            return;
        }
        _material = CoreUtils.CreateEngineMaterial(shader);
        _fluidPass = new FluidPass(RenderPassEvent.BeforeRenderingPostProcessing, _material);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData) {
        if (renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView) {
            return;
        }
        _fluidPass.ConfigureInput(ScriptableRenderPassInput.Color);
        _fluidPass.SetTarget(renderer.cameraColorTargetHandle);
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