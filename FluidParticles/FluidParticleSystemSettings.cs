using System;
using System.Collections;
using System.Collections.Generic;
using SkinnedMeshDecals;
using UnityEngine;

namespace FluidRenderingForGames {

    //[CreateAssetMenu(fileName = "FluidParticleSystemSettings", menuName = "FluidParticleSystemSettings")]
    public class FluidParticleSystemSettings : ScriptableObject {
        
        [NonSerialized]
        public static bool noCollide;
        
        public enum HeightModulate { Add, Clear }
        
        [field: SerializeField] public virtual float baseVelocity { get; protected set; }
        [field: SerializeField] public virtual float particleBaseSize { get; protected set; } = 0.1f;
        [field: SerializeField] public virtual Color color { get; protected set; } = Color.white;
        [field: SerializeField] public virtual float heightStrengthBase { get; protected set; } = 0.1f;
        [field: SerializeField] public virtual float noiseStrength { get; protected set; }
        [field: SerializeField] public virtual float noiseFrequency { get; protected set; }
        [field: SerializeField] public virtual int noiseOctaves { get; protected set; }
        [field: SerializeField] public virtual float splatSize { get; protected set; }
        [field: SerializeField] public virtual Material particleMaterial { get; protected set; }
        [field: SerializeField] public virtual LayerMask decalableHitMask { get; protected set; } = ~0;
        [field: SerializeField] public virtual HeightModulate heightModulate { get; protected set; }

        public void SetData(float? baseVelocity = null,
            float? particleBaseSize = null,
            Color? color = null,
            float? heightStrengthBase = null,
            float? noiseStrength = null,
            float? noiseFrequency = null,
            int? noiseOctaves = null,
            float? splatSize = null,
            Material particleMaterial = null,
            LayerMask? decalableHitMask = null,
            HeightModulate? heightModulate = null) {
            
            this.baseVelocity = baseVelocity ?? this.baseVelocity;
            this.particleBaseSize = particleBaseSize ?? this.particleBaseSize;
            this.color = color ?? this.color;
            this.heightStrengthBase = heightStrengthBase ?? this.heightStrengthBase;
            this.noiseStrength = noiseStrength ?? this.noiseStrength;
            this.noiseFrequency = noiseFrequency ?? this.noiseFrequency;
            this.noiseOctaves = noiseOctaves ?? this.noiseOctaves;
            this.splatSize = splatSize ?? this.splatSize;
            this.particleMaterial = particleMaterial ? particleMaterial : this.particleMaterial;
            this.decalableHitMask = decalableHitMask ?? this.decalableHitMask;
            this.heightModulate = heightModulate ?? this.heightModulate;
        }

        public virtual void OnFluidCollision(FluidParticleSystem.ParticleCollision particleCollision) {
            if (noCollide) return;
            var stretch = particleCollision.stretch;
            var bounds =
                new Vector3(particleCollision.size * splatSize, stretch.magnitude,
                    particleCollision.size * 6f * splatSize); // the magic number is depth for misaligned colliders
            var rotation = Quaternion.LookRotation(-particleCollision.normal, stretch);
            PaintDecal.QueueDecal(particleCollision.collider,
                new DecalProjector(DecalProjectorType.SphereAlpha, particleCollision.color),
                new DecalProjection(particleCollision.position, rotation, bounds * 1.5f )
            );
            if (heightModulate == HeightModulate.Add) {
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
            if (heightModulate == HeightModulate.Clear) {
                PaintDecal.QueueDecal(particleCollision.collider,
                    new DecalProjector(DecalProjectorType.SphereAlpha,
                        new Color(0f, 0f, 0f, 1f)),
                    new DecalProjection(particleCollision.position, rotation, bounds),
                    new DecalSettings(
                        textureName: "_FluidHeight",
                        renderTextureFormat: RenderTextureFormat.RFloat,
                        renderTextureReadWrite: RenderTextureReadWrite.Linear,
                        dilation: DilationType.Additive
                    )
                );
            }
        }

    }

}