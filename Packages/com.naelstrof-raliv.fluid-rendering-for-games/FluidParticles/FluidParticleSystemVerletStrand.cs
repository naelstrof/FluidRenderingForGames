using System;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class FluidParticleSystemVerletStrand : FluidParticleSystem {
    private Transform a;
    private Vector3 localPointA;
    
    private Transform b;
    private Vector3 localPointB;

    private bool broken;
    private float timeBroken;
    public const float fadeoutTime = 1f;
    public bool GetBroken() => broken;
    public float GetTimeBroken() => timeBroken;

    private const float baseSize = 1f;
    private const float baseHeightStrength = 4f;
    private const float cubicHeightStrength = 25f;
    private const float breakForce = 1.2f;
    
    
    private FluidParticleSystemSettings settings;
    
    public FluidParticleSystemVerletStrand(Transform a, Vector3 localPointA, Transform b, Vector3 localPointB, Material material, FluidParticleSystemSettings fluidParticleSystemSettings, LayerMask collisionLayerMask, int particleCountMax = 25) :
        base(material, fluidParticleSystemSettings, collisionLayerMask, particleCountMax) {
        this.settings = fluidParticleSystemSettings;
        this.a = a;
        this.localPointA = localPointA;
        this.b = b;
        this.localPointB = localPointB;
        Vector3 positionA = a.TransformPoint(localPointA);
        Vector3 positionB = b.TransformPoint(localPointB);
        for (int i = 0; i < _particles.Length; i++) {
            float progress = (float)i / (_particles.Length-1);
            _particles[i].position = Vector3.Lerp(positionA, positionB, progress);
            _particlePhysics[i].lastPosition = _particles[i].position;
            if (progress < 0.5f) {
                _particles[i].size = baseSize * fluidParticleSystemSettings.particleBaseSize; //((1f-Easing.OutPower(progress * 2f, 5))*cubicSize + baseSize) * fluidParticleSystemSettings.particleBaseSize;
                _particles[i].color = fluidParticleSystemSettings.color;
                float easing = 1f - Easing.OutPower(progress * 2f, 4);
                _particles[i].heightStrength = (baseHeightStrength + (easing*cubicHeightStrength)) * fluidParticleSystemSettings.heightStrengthBase;
            } else {
                _particles[i].size = baseSize * fluidParticleSystemSettings.particleBaseSize;// ((Easing.InPower((progress-0.5f) * 2f, 5))*cubicSize + baseSize) * fluidParticleSystemSettings.particleBaseSize;
                _particles[i].color = fluidParticleSystemSettings.color;
                float easing = Easing.InPower((progress - 0.5f) * 2f, 4);
                _particles[i].heightStrength = (baseHeightStrength + (easing*cubicHeightStrength)) * fluidParticleSystemSettings.heightStrengthBase;
            }
        }

        _particles[0].heightStrength = 2f;
    }

    public void SetLocalPointA(Vector3 point) {
        localPointA = point;
    }
    public Vector3 ExpDecay(Vector3 a, Vector3 b, float decay, float dt) {
        return b + (a - b) * Mathf.Exp(-decay * dt);
    }

    private bool CheckBreak(float mag) {
        if (!broken && mag > breakForce / _particles.Length) {
            broken = true;
            timeBroken = Time.time;
        }
        return broken;
    }
    protected override void UpdateParticles() {
        const float constraintStrength = 0.25f; // Range 0f to 0.5f
        const float friction = 0.06f; // Range 0f to 1f
        const float gravityMult = 1f;
        float realFriction = 1f - (friction * friction);
        if (broken) {
            for (var i = 0; i < _particles.Length; i++) {
                float fadeProgress = 1f-(Time.time - timeBroken)/fadeoutTime;
                float progress = (float)i / _particles.Length;
                if (progress < 0.5f) {
                    _particles[i].size = baseSize * settings.particleBaseSize * fadeProgress;
                } else {
                    _particles[i].size = baseSize * settings.particleBaseSize * fadeProgress;
                }
            }
        }
        _particles[0].position = a.TransformPoint(localPointA);
        _particles[^1].position = b.TransformPoint(localPointB);
        for (int iterations = 0; iterations < 10; iterations++) {
            for (var i = 1; i < _particles.Length - 1; i++) {
                var diff = _particles[i - 1].position - _particles[i].position;
                if (i==_particles.Length/2 || i == _particles.Length/2+1) {
                    if (CheckBreak(diff.magnitude)) {
                        continue;
                    }
                }
                if (i - 1 != 0) {
                    _particles[i].position += diff * constraintStrength;
                    _particles[i - 1].position -= diff * constraintStrength;
                } else {
                    _particles[i].position += diff * (constraintStrength*2f);
                }
            }
            for (var i = _particles.Length-2; i >= 1; i--) {
                var diff = _particles[i + 1].position - _particles[i].position;
                if (i==_particles.Length/2 || i == _particles.Length/2+1) {
                    if (CheckBreak(diff.magnitude)) {
                        continue;
                    }
                }
                if (i + 1 != _particles.Length - 1) {
                    _particles[i].position += diff * constraintStrength;
                    _particles[i + 1].position -= diff * constraintStrength;
                } else {
                    _particles[i].position += diff * (constraintStrength*2f);
                }
            }
        }

        for (var i = 1; i < _particles.Length-1; i++) {
            var vel = _particles[i].position - _particlePhysics[i].lastPosition;
            _particlePhysics[i].lastPosition = _particles[i].position;
            if (broken) {
                _particles[i].position += vel * realFriction;
            } else {
                _particles[i].position += vel * realFriction + Physics.gravity * (Time.deltaTime * Time.deltaTime * gravityMult);
            }
        }
    }
}