using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FluidParticleSystemSettings", menuName = "FluidParticleSystemSettings")]
public class FluidParticleSystemSettings : ScriptableObject {

    [field:SerializeField] public float baseVelocity { get; private set; }
    [field: SerializeField] public float particleBaseSize { get; private set; } = 0.1f;
    [field: SerializeField] public Color color { get; private set; } = Color.white;
    [field: SerializeField] public float heightStrengthBase { get; private set; } = 0.1f;
    [field:SerializeField] public float noiseStrength { get; private set; }
    [field:SerializeField] public float noiseFrequency { get; private set; }
    [field:SerializeField] public int noiseOctaves { get; private set; }
    [field:SerializeField] public float splatSize { get; private set; }
    [field:SerializeField] public Material particleMaterial { get; private set; }
    [field:SerializeField] public LayerMask decalableHitMask { get; private set; } = ~0;

}