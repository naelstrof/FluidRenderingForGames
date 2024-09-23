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
    [SerializeField] private Renderer tempRenderer;
    
    private FluidParticleSystem _fluidParticleSystem;
    private Vector3 _previousPosition;
    private Vector3 _previousForward;
    private float _previousStrength;
    private bool rendered = false;
    private SceneView targetSceneView;
    private bool printedConfigurationIssue = false;

    private void OnEnable() {
        _fluidParticleSystem = new FluidParticleSystemEuler(particleMaterial, fluidParticleSystemSettings, decalableHitMask);
        _fluidParticleSystem.particleCollisionEvent += OnFluidCollision;
        FluidPass.AddParticleSystem(_fluidParticleSystem);
    }

    private void OnFluidCollision(RaycastHit hit, float particlevolume) {
        if (!hit.collider.TryGetComponent(out DecalableCollider decalableCollider)) {
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

    private void OnDisable() {
        FluidPass.RemoveParticleSystem(_fluidParticleSystem);
        _fluidParticleSystem.particleCollisionEvent -= OnFluidCollision;
        _fluidParticleSystem.Cleanup();
    }

    //private void Update() {
        //_fluidParticleSystem?.Render();
    //}

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
