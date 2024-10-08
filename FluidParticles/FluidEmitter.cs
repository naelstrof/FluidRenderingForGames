using SkinnedMeshDecals;
using UnityEngine;
using DecalProjector = SkinnedMeshDecals.DecalProjector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FluidRenderingForGames {
public class FluidEmitter : MonoBehaviour {
    
    internal static Material sourceDecalProjectorAlphaWrite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void LoadDecalProjectorAlphaWrite() {
        sourceDecalProjectorAlphaWrite = Resources.Load<Material>("SphereProjectorAlphaWrite");
    }

    [SerializeField] private FluidParticleSystemSettings fluidParticleSystemSettings;
    [SerializeField, Range(0f, 1f)] private float _velocityMultiplier = 1f;
    [SerializeField, Range(0f, 1f)] private float _heightStrengthMultiplier = 1f;

    private FluidParticleSystem _fluidParticleSystem;
    private Vector3 _previousPosition;
    private Vector3 _previousForward;
    private float _velocity;
    private float _previousVelocity;
    private SceneView targetSceneView;
    private Material decalProjectorAlphaWrite;

    private void OnEnable() {
        _fluidParticleSystem = new FluidParticleSystemEuler(fluidParticleSystemSettings.particleMaterial,
            fluidParticleSystemSettings, fluidParticleSystemSettings.decalableHitMask);
        _fluidParticleSystem.particleCollisionEvent += OnFluidCollision;
        FluidPass.AddParticleSystem(_fluidParticleSystem);
        decalProjectorAlphaWrite = Instantiate(sourceDecalProjectorAlphaWrite);
    }

    private void OnFluidCollision(FluidParticleSystem.ParticleCollision particleCollision) {
        var stretch = particleCollision.stretch;
        var bounds =
            new Vector3(particleCollision.size, stretch.magnitude,
                particleCollision.size * 6f); // the magic number is depth for misaligned colliders
        var rotation = Quaternion.LookRotation(-particleCollision.normal, stretch);
        //Debug.DrawLine(
        //    particleCollision.position-rotation*Vector3.up*stretch.magnitude,
        //    particleCollision.position+rotation*Vector3.up*stretch.magnitude,
        //    Color.red,
        //    0.5f
        //    );
        decalProjectorAlphaWrite.color = particleCollision.color;
        PaintDecal.QueueDecal(particleCollision.collider,
            decalProjectorAlphaWrite,
            new DecalProjection(
                particleCollision.position,
                rotation,
                bounds * 1.5f
            )
        );
        PaintDecal.QueueDecal(particleCollision.collider,
            new DecalProjector(DecalProjectorType.SphereAdditive,
                new Color(particleCollision.heightStrength, 0f, 0f, 1f)),
            new DecalProjection(particleCollision.position, rotation, bounds),
            new DecalSettings(
                textureName: "_FluidHeight",
                renderTextureFormat: RenderTextureFormat.RFloat,
                renderTextureReadWrite: RenderTextureReadWrite.Linear,
                dilation: DilationType.Additive
            )
        );
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
                (float)i / subParticles,
                i == 0
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

}