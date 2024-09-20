using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FluidEmitter : MonoBehaviour {

    [SerializeField] private Material particleMaterial;
    
    private FluidParticleSystem _fluidParticleSystem;
    private float _strength;
    private Vector3 _previousPosition;
    private Vector3 _previousForward;
    private float _previousStrength;
    private bool rendered = false;
    private SceneView targetSceneView;

    private void OnEnable() {
#if UNITY_EDITOR
        EditorApplication.pauseStateChanged += OnPauseChanged;
#endif
        _fluidParticleSystem = new FluidParticleSystem(particleMaterial);
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
