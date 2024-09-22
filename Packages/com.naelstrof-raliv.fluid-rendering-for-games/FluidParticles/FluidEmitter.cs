using System;
using System.Collections;
using System.Collections.Generic;
using SkinnedMeshDecals;
using UnityEngine;
using UnityEngine.Rendering;
using DecalProjector = SkinnedMeshDecals.DecalProjector;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering.Universal;
#endif

public class FluidEmitter : MonoBehaviour {

    [SerializeField] private Material particleMaterial;
    [SerializeField] private FluidParticleSystemSettings fluidParticleSystemSettings;
    [SerializeField] private LayerMask decalableHitMask = ~0;
    [SerializeField, Range(0f,1f)] private float _volume;
    [SerializeField, Range(0f,5f)] private float _strength;
    
    private FluidParticleSystem _fluidParticleSystem;
    private Vector3 _previousPosition;
    private Vector3 _previousForward;
    private float _previousStrength;
    private bool rendered = false;
    private SceneView targetSceneView;
    private bool printedConfigurationIssue = false;

    private void OnEnable() {
#if UNITY_EDITOR
        EditorApplication.pauseStateChanged += OnPauseChanged;
#endif
        _fluidParticleSystem = new FluidParticleSystemEuler(particleMaterial, fluidParticleSystemSettings, decalableHitMask);
        _fluidParticleSystem.particleCollisionEvent += OnFluidCollision;
    }

    private void OnFluidCollision(RaycastHit hit, float particlevolume) {
        if (!hit.collider.TryGetComponent(out DecalableCollider decalableCollider)) {
#if UNITY_EDITOR
            if (printedConfigurationIssue) return;
            Debug.LogWarning($"No decalable collider found on {hit.collider}. <color=cyan>Please configure using a DecalableCollider Monobehavior, or set up your layers to be more specific!</color>", hit.collider.gameObject);
            printedConfigurationIssue = true;
#endif
            return;
        }

        foreach (var rend in decalableCollider.decalableRenderers) {
            if (!rend) continue;
            var particleSize = particlevolume * fluidParticleSystemSettings.splatSize;
            PaintDecal.RenderDecal(rend, 
                new DecalProjector(DecalProjectorType.SphereAlpha, new Color(1,1,1,particlevolume * 0.05f)),
                new DecalProjection(hit.point, Quaternion.LookRotation(-hit.normal, Vector3.up), new Vector3(particleSize, particleSize, particleSize*6f))
                );
        }
    }

#if UNITY_EDITOR
    private void OnPauseChanged(PauseState obj) {
        if (obj == PauseState.Paused) {
            rendered = false;
            SceneView.duringSceneGui += OnSceneGUI;
        } else {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }

    private void OnSceneGUI(SceneView obj) {
        switch (Event.current.type) {
            case EventType.Repaint:
                if (SceneView.currentDrawingSceneView == null || !SceneView.currentDrawingSceneView.hasFocus) {
                    rendered = false;
                    return;
                }
                if (!rendered) {
                    _fluidParticleSystem?.Render();
                    rendered = true;
                }
                break;
        }
    }
#endif

    private void OnDisable() {
#if UNITY_EDITOR
        EditorApplication.pauseStateChanged += OnPauseChanged;
        SceneView.duringSceneGui -= OnSceneGUI;
#endif
        _fluidParticleSystem.particleCollisionEvent -= OnFluidCollision;
        _fluidParticleSystem.Cleanup();
    }

    private void Update() {
        _fluidParticleSystem?.Render();
    }

    private void FixedUpdate() {
        if (!Application.isPlaying) {
            return;
        }
        int subParticles = 1 + (int)(_strength * 8);
        for (int i = 0; i < subParticles; i++) {
            _fluidParticleSystem.SpawnParticle(
                transform.position, 
                _previousPosition, 
                transform.forward, 
                _previousForward, 
                _strength, 
                _previousStrength,
                _volume,
                (float)i/subParticles,
                i==0
                );
        }
        _fluidParticleSystem.FixedUpdate();
        _previousForward = transform.forward;
        _previousPosition = transform.position;
        _previousStrength = _strength;
    }

    public void SetStrength(float strength) {
        _strength = strength;
    }
    
    public void SetVolume(float volume) {
        _volume = volume;
    }
    
}
