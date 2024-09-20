using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FluidParticleSystemSettings", menuName = "FluidParticleSystemSettings")]
public class FluidParticleSystemSettings : ScriptableObject {

    [field:SerializeField, Range(0f,2f)] public float noiseStrength { get; private set; }
    [field:SerializeField] public float noiseFrequency { get; private set; }
    [field:SerializeField] public int noiseOctaves { get; private set; }

}
