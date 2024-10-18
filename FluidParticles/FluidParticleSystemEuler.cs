using UnityEngine;

namespace FluidRenderingForGames {

    public class FluidParticleSystemEuler : FluidParticleSystem {
        
        public FluidParticleSystemEuler(
            Material material, 
            FluidParticleSystemSettings fluidParticleSystemSettings, 
            LayerMask collisionLayerMask, 
            int particleCountMax = 1000
            ) : base(material, fluidParticleSystemSettings, collisionLayerMask, particleCountMax) {
        }
        
        protected override void UpdateParticles(float dt) {
            for (var index = 0; index < _particles.Length; index++) {
                if (_particles[index].heightStrength <= 0.01f) continue;
                var positionStep = _particlePhysics[index].velocity * dt;
                if (_particlePhysics[index].colliding && _particles[index].heightStrength>0f) {
                    if (Physics.Raycast(_particles[index].position, positionStep, out var hit, positionStep.magnitude, _collisionLayerMask)) {
                        TriggerParticleCollisionEvent(new ParticleCollision() {
                            collider = hit.collider,
                            position = hit.point,
                            normal = -_particlePhysics[index].velocity,
                            size = _particles[index].size,
                            color = _particles[index].color,
                            heightStrength = _particles[index].heightStrength,
                            stretch = GetStretch(index)
                        }, index);
                    }
                }
            }
            for (var index = 0; index < _particles.Length; index++) {
                if (_particles[index].heightStrength == 0f) continue;
                _particles[index].position += _particlePhysics[index].velocity * dt;
                // TODO: can fade based on proximity to being respawned
                //_particles[index].volume = Mathf.Max(0f, _particles[index].volume-dt*0.3f);
                _particlePhysics[index].velocity += Physics.gravity * dt;
            }
        }

        Vector3 GetStretch(int index) {
            var walk = index;
            var failsafe = 0;
            do {
                walk=(walk+1)%_particles.Length;
                failsafe++;
                if (failsafe>_particles.Length*0.25f) return _particlePhysics[index].velocity.normalized * _particles[index].size;
            } while (!_particlePhysics[walk].colliding);
            var stretchTarget = _particles[walk].position;
            var stretch = stretchTarget - _particles[index].position;
            if (stretch.magnitude>_particlePhysics[index].velocity.magnitude*0.05f) return _particlePhysics[index].velocity.normalized * _particles[index].size;
            //Debug.DrawLine(
            //    stretchTarget,
            //    _particles[index].position,
            //    Color.green,
            //    0.5f
            //  );
            stretch = Vector3.ProjectOnPlane(stretch, _particlePhysics[index].velocity) * 1.3f;
            var stretchMagnitude = stretch.magnitude;
            stretchMagnitude += _particles[index].size * 0.5f;
            //if (stretchMagnitude < _particles[index].size * 0.5f) stretchMagnitude = _particles[index].size * 0.5f;
            stretch = stretch.normalized * stretchMagnitude;
            return stretch;
        }
        
    }

}

