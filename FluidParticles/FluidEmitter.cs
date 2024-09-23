using System;
using System.Collections;
using System.Collections.Generic;
using SkinnedMeshDecals;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using DecalProjector = SkinnedMeshDecals.DecalProjector;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering.Universal;
#endif

public class FluidEmitter : MonoBehaviour {

    [SerializeField] private FluidParticleSystemSettings fluidParticleSystemSettings;
    [SerializeField, Range(0f,1f)] private float _velocityMultiplier = 1f;
    [SerializeField, Range(0f,1f)] private float _heightStrengthMultiplier = 1f;
    
    private FluidParticleSystem _fluidParticleSystem;
    private Vector3 _previousPosition;
    private Vector3 _previousForward;
    private float _velocity;
    private float _previousVelocity;
    private bool rendered = false;
    private SceneView targetSceneView;
    private bool printedConfigurationIssue = false;

    private void OnEnable() {
        _fluidParticleSystem = new FluidParticleSystemEuler(fluidParticleSystemSettings.particleMaterial, fluidParticleSystemSettings, fluidParticleSystemSettings.decalableHitMask);
        _fluidParticleSystem.particleCollisionEvent += OnFluidCollision;
        FluidPass.AddParticleSystem(_fluidParticleSystem);
    }

    private void OnFluidCollision(RaycastHit hit, FluidParticleSystem.Particle particle) {
        if (!hit.collider.TryGetComponent(out DecalableCollider decalableCollider)) {
            return;
        }

        foreach (var rend in decalableCollider.decalableRenderers) {
            if (!rend) continue;
            PaintDecal.RenderDecal(rend, 
                new DecalProjector(DecalProjectorType.SphereAlpha, particle.color),
                new DecalProjection(hit.point, Quaternion.LookRotation(-hit.normal, Vector3.up), new Vector3(particle.size, particle.size, particle.size*6f)*1.5f)
            );
            PaintDecal.RenderDecal(rend, 
                new DecalProjector(DecalProjectorType.SphereAlpha, new Color(1f, 0f, 0f, particle.heightStrength)),
                new DecalProjection(hit.point, Quaternion.LookRotation(-hit.normal, Vector3.up), new Vector3(particle.size, particle.size, particle.size*6f)),
                new DecalSettings(DecalResolutionType.Auto, textureName:"_FluidHeight", renderTextureFormat:RenderTextureFormat.RFloat, renderTextureReadWrite:RenderTextureReadWrite.Linear)
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
        _velocity = fluidParticleSystemSettings.baseVelocity * _velocityMultiplier;
        var _heightStrength = fluidParticleSystemSettings.heightStrengthBase * _heightStrengthMultiplier;
        int subParticles = 1 + (int)(_velocity * 8);
        for (int i = 0; i < subParticles; i++) {
            _fluidParticleSystem.SpawnParticle(
                transform.position, 
                _previousPosition, 
                transform.forward, 
                _previousForward, 
                _velocity, 
                _previousVelocity,
                fluidParticleSystemSettings.particleBaseSize,
                fluidParticleSystemSettings.color,
                _heightStrength,
                (float)i/subParticles,
                i==0
                );
        }
        _fluidParticleSystem.FixedUpdate();
        _previousForward = transform.forward;
        _previousPosition = transform.position;
        _previousVelocity = _velocity;
    }

    public void SetVelocityMultiplier(float velocityMultiplier) {
        _velocityMultiplier = velocityMultiplier;
    }
    
}
