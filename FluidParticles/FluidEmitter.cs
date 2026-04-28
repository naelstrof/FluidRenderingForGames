using SkinnedMeshDecals;
using UnityEngine;
using DecalProjector = SkinnedMeshDecals.DecalProjector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FluidRenderingForGames {
    
    public class FluidEmitter : MonoBehaviour {

        
        internal static Material sourceDecalProjectorAlphaWrite;

        [SerializeField] private FluidParticleSystemSettings fluidParticleSystemSettings;
        [SerializeField, Range(0f, 1f)] private float _velocityMultiplier = 1f;
        [SerializeField, Range(0f, 1f)] private float _heightStrengthMultiplier = 1f;

        private FluidParticleSystem _fluidParticleSystem;
        private Vector3 _previousPosition;
        private Vector3 _previousForward;
        private float _velocity;
        private float _previousVelocity;
        private float _previousHeightStrength;
        #if UNITY_EDITOR
        private SceneView targetSceneView;
        #endif
        private float _accumulatedTickTime;
        private float _tickTime;

        private void OnEnable() {
            _fluidParticleSystem = new FluidParticleSystemEuler(fluidParticleSystemSettings.particleMaterial,
                fluidParticleSystemSettings, fluidParticleSystemSettings.decalableHitMask);
            _fluidParticleSystem.particleCollisionEvent += OnFluidCollision;
            FluidRenderingRendererFeature.AddParticleSystem(_fluidParticleSystem);
        }

        private void OnFluidCollision(FluidParticleSystem.ParticleCollision particleCollision) {
            fluidParticleSystemSettings.OnFluidCollision(particleCollision);
        }
        
        private void OnDisable() {
            FluidRenderingRendererFeature.RemoveParticleSystem(_fluidParticleSystem);
            _fluidParticleSystem.particleCollisionEvent -= OnFluidCollision;
            _fluidParticleSystem.Cleanup();
        }

        private void LateUpdate() {
            var tickTime = 0.02f;
            _accumulatedTickTime += Time.deltaTime;
            var queuedTicks = Mathf.Floor(_accumulatedTickTime / tickTime);
            var clampedQueuedTicks = Mathf.Min(queuedTicks, 2);
            for (int currentTick=0;currentTick<clampedQueuedTicks;currentTick++) {
                var subTBegin = currentTick / clampedQueuedTicks;
                var subTEnd = (currentTick+1)/clampedQueuedTicks;
                Tick(tickTime, subTBegin, subTEnd);
            }
            if (queuedTicks > 0) {
                _accumulatedTickTime -= queuedTicks*tickTime;
                _previousForward = transform.forward;
                _previousPosition = transform.position;
                _previousVelocity = _velocity;
                _previousHeightStrength = fluidParticleSystemSettings.heightStrengthBase * _heightStrengthMultiplier;
            }
        }

        private void Tick(float deltaTime, float subTBegin=0f, float subTEnd=1f) {
            if (!Application.isPlaying) {
                return;
            }

            _tickTime += deltaTime;
            if (_tickTime > 10000f) _tickTime = 0f;
            _velocity = fluidParticleSystemSettings.baseVelocity * _velocityMultiplier;
            var _heightStrength = fluidParticleSystemSettings.heightStrengthBase * _heightStrengthMultiplier;
            int subParticles = 5 + (int)(_velocity * 5);
            for (int i = 0; i < subParticles; i++) {
                _fluidParticleSystem.SpawnParticle(
                    new FluidParticleSystem.InterpolatedParticleInfo() {
                        position = transform.position,
                        forward = transform.forward,
                        velocity = _velocity,
                        heightStrength = _heightStrength
                    },
                    new FluidParticleSystem.InterpolatedParticleInfo() {
                        position = _previousPosition,
                        forward = _previousForward,
                        velocity = _previousVelocity,
                        heightStrength = _previousHeightStrength
                    },
                    fluidParticleSystemSettings.particleBaseSize,
                    fluidParticleSystemSettings.color,
                    deltaTime,
                    _tickTime,
                    Mathf.Lerp(subTBegin, subTEnd, (float)i / subParticles),
                    (float)i / subParticles,
                    i == 0
                );
            }
            _fluidParticleSystem.Update(deltaTime);
        }

        public void SetVelocityMultiplier(float velocityMultiplier) {
            _velocityMultiplier = velocityMultiplier;
        }

        public void SetHeightStrengthMultiplier(float heightStrengthMultiplier) {
            _heightStrengthMultiplier = heightStrengthMultiplier;
        }

    }

}