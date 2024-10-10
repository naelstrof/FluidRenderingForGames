#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;

namespace FluidRenderingForGames {

public static class FluidParticleSystemSettingsDefaults {
    private static string GetActiveFolderPath() {
        // Can't believe we need to use reflection to call this method!
        MethodInfo getActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
        string folderPath = (string)getActiveFolderPath.Invoke(null, null);
        return folderPath;
    }
    [MenuItem("Assets/Create/Fluid Rendering For Games/Example cum settings", false, 5)]
    private static void CreateExampleCumSettings() {
        FluidParticleSystemSettings cum = (FluidParticleSystemSettings)ScriptableObject.CreateInstance(typeof(FluidParticleSystemSettings));
        var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath("1ffacd69c2e9ceb4d9a1f7aafcb3a486"));
        if (material == null) {
            throw new UnityException("Failed to load asset FluidParticles/FliudParticlesMaterial.mat, did you install Fluid Rendering for Games correctly?");
        }
        cum.SetData(
            baseVelocity: 3f,
            particleBaseSize: 0.1f,
            color: new Color(1,1,1,0.5f),
            heightStrengthBase: 0.03f,
            noiseStrength: 0.2f,
            noiseFrequency: 3f,
            noiseOctaves: 3,
            splatSize: 1f,
            particleMaterial: material
        );
        ProjectWindowUtil.CreateAsset(cum, GetActiveFolderPath() + "/CumSettings.asset");
    }
}

}

#endif