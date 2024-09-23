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

    private const float baseThickness = 2f;
    private const float cubicThickness = 1f;
    
    public FluidParticleSystemVerletStrand(Transform a, Vector3 localPointA, Transform b, Vector3 localPointB, Material material, FluidParticleSystemSettings fluidParticleSystemSettings, LayerMask collisionLayerMask, int particleCountMax = 25) :
        base(material, fluidParticleSystemSettings, collisionLayerMask, particleCountMax) {
        this.a = a;
        this.localPointA = localPointA;
        this.b = b;
        this.localPointB = localPointB;
        Vector3 positionA = a.TransformPoint(localPointA);
        Vector3 positionB = b.TransformPoint(localPointB);
        for (int i = 0; i < _particles.Length; i++) {
            float progress = (float)i / _particles.Length;
            _particles[i].position = Vector3.Lerp(positionA, positionB, progress);
            _particlePhysics[i].lastPosition = _particles[i].position;
            if (progress < 0.5f) {
                _particles[i].size = (1f-Easing.OutCubic(progress * 2f))*cubicThickness + baseThickness;
                _particles[i].color = new Color(1, 1, 1, 0.25f);
            } else {
                _particles[i].size = (Easing.InCubic(progress * 2f-1f))*cubicThickness + baseThickness;
                _particles[i].color = new Color(1, 1, 1, 0.25f);
            }
        }
    }
    public Vector3 ExpDecay(Vector3 a, Vector3 b, float decay, float dt) {
        return b + (a - b) * Mathf.Exp(-decay * dt);
    }

    private bool CheckBreak(int i, float mag) {
        if (!broken && mag > 1f / _particles.Length) {
            broken = true;
            timeBroken = Time.time;
        }
        return broken;
    }
    protected override void UpdateParticles() {
        const float constraintStrength = 0.35f; // Range 0f to 0.5f
        const float friction = 0.12f; // Range 0f to 1f
        const float gravityMult = 1f;
        float realFriction = 1f - (friction * friction);
        if (broken) {
            for (var i = 0; i < _particles.Length; i++) {
                float fadeProgress = 1f-(Time.time - timeBroken)/fadeoutTime;
                float progress = (float)i / _particles.Length;
                if (progress < 0.5f) {
                    _particles[i].size = ((1f-Easing.OutCubic(progress * 2f))*cubicThickness + baseThickness) * fadeProgress;
                    _particles[i].color = new Color(1, 1, 1, 0.25f);
                } else {
                    _particles[i].size = (Easing.InCubic(progress * 2f-1f)*cubicThickness + baseThickness) * fadeProgress;
                    _particles[i].color = new Color(1, 1, 1, 0.25f);
                }
            }
        }
        for (int iterations = 0; iterations < 10; iterations++) {
            _particles[0].position = a.TransformPoint(localPointA);
            _particles[^1].position = b.TransformPoint(localPointB);
            for (var i = 1; i < _particles.Length - 1; i++) {
                var diff = _particles[i - 1].position - _particles[i].position;
                if (i==_particles.Length/2 || i == _particles.Length/2+1) {
                    if (CheckBreak(i, diff.magnitude)) {
                        continue;
                    }
                }
                _particles[i].position += diff * constraintStrength;
                _particles[i - 1].position -= diff * constraintStrength;
            }
            for (var i = _particles.Length-2; i >= 1; i--) {
                var diff = _particles[i + 1].position - _particles[i].position;
                if (i==_particles.Length/2 || i == _particles.Length/2+1) {
                    if (CheckBreak(i, diff.magnitude)) {
                        continue;
                    }
                }
                _particles[i].position += diff * constraintStrength;
                _particles[i + 1].position -= diff * constraintStrength;
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