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

    private void OnFluidCollision(FluidParticleSystem.ParticleCollision particleCollision) {
        if (!particleCollision.collider.TryGetComponent(out DecalableCollider decalableCollider)) {
            return;
        }

        foreach (var rend in decalableCollider.decalableRenderers) {
            if (!rend) continue;
            var stretch = particleCollision.stretch;
            var bounds = new Vector3(particleCollision.size, stretch.magnitude, particleCollision.size * 6f); // the magic number is depth for misaligned colliders
            var rotation = Quaternion.LookRotation(-particleCollision.normal, stretch);
            //Debug.DrawLine(
            //    particleCollision.position-rotation*Vector3.up*stretch.magnitude,
            //    particleCollision.position+rotation*Vector3.up*stretch.magnitude,
            //    Color.red,
            //    0.5f
            //    );
            PaintDecal.RenderDecal(rend, 
                new DecalProjector(DecalProjectorType.SphereAlpha, particleCollision.color),
                new DecalProjection(
                    particleCollision.position,
                    rotation, 
                    bounds*1.5f
                    )
            );
            PaintDecal.RenderDecal(rend, 
                new DecalProjector(DecalProjectorType.SphereAlpha, new Color(1f, 0f, 0f, particleCollision.heightStrength)),
                new DecalProjection(particleCollision.position, rotation, bounds),
                new DecalSettings(
                    DecalResolutionType.Auto,
                    textureName:"_FluidHeight",
                    renderTextureFormat:RenderTextureFormat.RFloat,
                    renderTextureReadWrite:RenderTextureReadWrite.Linear
                    )
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
    public void setHeightStrengthMultiplier(float heightStrengthMultiplier) {
        _heightStrengthMultiplier = heightStrengthMultiplier;
    }
    
}
