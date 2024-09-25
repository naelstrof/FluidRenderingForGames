using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FluidRenderingForGames {

//[CreateAssetMenu(fileName = "FluidParticleSystemSettings", menuName = "FluidParticleSystemSettings")]
public class FluidParticleSystemSettings : ScriptableObject {

    [field: SerializeField] public float baseVelocity { get; private set; }
    [field: SerializeField] public float particleBaseSize { get; private set; } = 0.1f;
    [field: SerializeField] public Color color { get; private set; } = Color.white;
    [field: SerializeField] public float heightStrengthBase { get; private set; } = 0.1f;
    [field: SerializeField] public float noiseStrength { get; private set; }
    [field: SerializeField] public float noiseFrequency { get; private set; }
    [field: SerializeField] public int noiseOctaves { get; private set; }
    [field: SerializeField] public float splatSize { get; private set; }
    [field: SerializeField] public Material particleMaterial { get; private set; }
    [field: SerializeField] public LayerMask decalableHitMask { get; private set; } = ~0;

    public void SetData(float? baseVelocity = null,
        float? particleBaseSize = null,
        Color? color = null,
        float? heightStrengthBase = null,
        float? noiseStrength = null,
        float? noiseFrequency = null,
        int? noiseOctaves = null,
        float? splatSize = null,
        Material particleMaterial = null,
        LayerMask? decalableHitMask = null) {
        
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
    }
}

}