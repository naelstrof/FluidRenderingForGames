using UnityEngine;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FluidRenderingRendererFeature : ScriptableRendererFeature {
    [SerializeField] private Material fullscreenBlitMaterial;
    private FluidPass _fluidPass;
    
    public override void Create() {
#if UNITY_EDITOR
        EnsureWeHaveFullscreenBlitMaterial();
        if (fullscreenBlitMaterial == null) {
            return;
        }
        EnsureLayersAreCorrect();
#endif
        
        _fluidPass = new FluidPass(RenderPassEvent.BeforeRenderingPostProcessing, fullscreenBlitMaterial, LayerMask.GetMask("FluidVFX"));
        
    }
    
#if UNITY_EDITOR
    public static void EnsureLayersAreCorrect() {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        for (int i = 0; i < layers.arraySize; i++) {
            var layerProp = layers.GetArrayElementAtIndex(i);
            if (layerProp.stringValue == "FluidVFX") {
                return;
            }
        }

        for (int i = 0; i < layers.arraySize; i++) {
            var layerProp = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layerProp.stringValue)) {
                Debug.Log($"FluidRenderingForGames: Created FluidVFX layer in slot {i}, feel free to move it under the layer settings.");
                layerProp.stringValue = "FluidVFX";
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                return;
            }
        }
        throw new UnityException( "Failed to find FluidVFX layer, and failed to create one automatically. Please create one or remove an unused layer!");
    }

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