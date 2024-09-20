using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidEmitter : MonoBehaviour {

    [SerializeField] private Material particleMaterial;
    
    private FluidParticleSystem _fluidParticleSystem;
    private float _strength;
    private Vector3 _previousPosition;
    private Vector3 _previousForward;
    private float _previousStrength;

    private void OnEnable() {
        _fluidParticleSystem = new FluidParticleSystem(particleMaterial);
    }

    private void OnDisable() {
        _fluidParticleSystem.Cleanup();
    }

    private void Update() {
        _fluidParticleSystem.Render();
    }

    private void FixedUpdate() {
        int subParticles = 1 + (int)(_strength * 8);
        for (int i = 0; i < subParticles; i++) {
            _fluidParticleSystem.SpawnParticle(
                transform.position, 
                _previousPosition, 
                transform.forward, 
                _previousForward, 
                _strength, 
                _previousStrength, 
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
    
}
