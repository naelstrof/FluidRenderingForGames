using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FluidParticleSystemEuler : FluidParticleSystem {
    public FluidParticleSystemEuler(Material material, FluidParticleSystemSettings fluidParticleSystemSettings, LayerMask collisionLayerMask, int particleCountMax = 3000) :
        base(material, fluidParticleSystemSettings, collisionLayerMask, particleCountMax) {
    }
    protected override void UpdateParticles() {
        for (var index = 0; index < _particles.Length; index++) {
            var positionStep = _particlePhysics[index].velocity * Time.deltaTime;
            if (_particlePhysics[index].Colliding) {
                if (Physics.Raycast(_particles[index].position, positionStep, out var hit, positionStep.magnitude, _collisionLayerMask)) {
                    TriggerParticleCollisionEvent(hit, _particles[index].volume);
                    var walk = index;
                    do {
                        _particles[walk].volume = 0f;
                        walk = (walk + 1) % _particlePhysics.Length;
                    } while (!_particlePhysics[walk].Colliding);
                }
            }
            _particles[index].position += positionStep;
            // TODO: can fade based on proximity to being respawned
            //_particles[index].volume = Mathf.Max(0f, _particles[index].volume-Time.deltaTime*0.3f);
            _particlePhysics[index].velocity += Physics.gravity * Time.deltaTime;
        }
    }
}