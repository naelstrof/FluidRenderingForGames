using UnityEngine;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FluidRenderingRendererFeature : ScriptableRendererFeature {
    [SerializeField] private Material fullscreenBlitMaterial;
    [SerializeField] private Texture fluidMatcap;
    private FluidPass _fluidPass;
    
    public override void Create() {
#if UNITY_EDITOR
        EnsureWeHaveFullscreenBlitMaterial();
        if (fullscreenBlitMaterial == null) {
            return;
        }
#endif
        
        _fluidPass = new FluidPass(RenderPassEvent.BeforeRenderingPostProcessing, fullscreenBlitMaterial);
        Shader.SetGlobalTexture("_FluidMatcap", fluidMatcap);
    }
    
#if UNITY_EDITOR
    private void EnsureWeHaveFullscreenBlitMaterial() {
        SerializedObject obj = new SerializedObject(this);
        var blitMat = obj.FindProperty(nameof(fullscreenBlitMaterial));
        if (blitMat.objectReferenceValue == null) {
            blitMat.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath("e6cb23922d304c94e89fd2de80c7293a"));
            obj.ApplyModifiedPropertiesWithoutUndo();
        }
    }
#endif
    
    

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
        _fluidPass?.Dispose();
        _fluidPass = null;
    }
    
}