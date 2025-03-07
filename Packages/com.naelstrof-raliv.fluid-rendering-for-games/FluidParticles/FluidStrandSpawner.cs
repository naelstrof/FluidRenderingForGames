using System;
using System.Collections.Generic;
using SkinnedMeshDecals;
using UnityEngine;

namespace FluidRenderingForGames {
public class FluidStrandSpawner : MonoBehaviour {
    [SerializeField] private Material particleMaterial;
    [SerializeField] private FluidParticleSystemSettings fluidParticleSystemSettings;
    [SerializeField] private LayerMask decalableHitMask = ~0;
    [SerializeField] private List<StrandAnchor> strandAnchors;

    [Serializable]
    private struct StrandAnchor {
        public Vector3 position;
        [NonSerialized] public FluidParticleSystemVerletStrand strand;
    }

    private Collider selfCollider;
    private Material decalProjectorAlphaWrite;

    private void OnEnable() {
        selfCollider = GetComponentInChildren<Collider>();
        decalProjectorAlphaWrite = Instantiate(FluidEmitter.sourceDecalProjectorAlphaWrite);
    }

    private void OnDisable() {
        for (int i = 0; i < strandAnchors.Count; i++) {
            var strandAnchor = strandAnchors[i];
            FluidPass.RemoveParticleSystem(strandAnchor.strand);
            strandAnchor.strand?.Cleanup();
            strandAnchor.strand = null;
            strandAnchors[i] = strandAnchor;
        }
    }

    private void FixedUpdate() {
        for (int i = 0; i < strandAnchors.Count; i++) {
            strandAnchors[i].strand?.SetLocalPointA(strandAnchors[i].position);
            if ((strandAnchors[i].strand?.GetBroken() ?? false) && Time.time - strandAnchors[i].strand.GetTimeBroken() >
                FluidParticleSystemVerletStrand.fadeoutTime) {
                FluidPass.RemoveParticleSystem(strandAnchors[i].strand);
                strandAnchors[i].strand.Cleanup();
                var strand = strandAnchors[i];
                strand.strand = null;
                strandAnchors[i] = strand;
            }

            strandAnchors[i].strand?.Update(Time.deltaTime);
        }
    }

    private void OnFluidCollision(FluidParticleSystem.ParticleCollision particleCollision) {
        var projection =
            new DecalProjection(
                particleCollision.position,
                particleCollision.normal,
                particleCollision.size
            );
        decalProjectorAlphaWrite.color = particleCollision.color;
        PaintDecal.QueueDecal(particleCollision.collider,
            decalProjectorAlphaWrite,
            projection
        );
        PaintDecal.QueueDecal(particleCollision.collider,
            new DecalProjector(DecalProjectorType.SphereAdditive,
                new Color(particleCollision.heightStrength, 0f, 0f, 1f)),
            projection,
            new DecalSettings(
                textureName: "_FluidHeight",
                renderTextureFormat: RenderTextureFormat.RFloat,
                renderTextureReadWrite: RenderTextureReadWrite.Linear,
                dilation: DilationType.Additive
            )
        );
    }

    private void OnTriggerStay(Collider other) {
        for (int i = 0; i < strandAnchors.Count; i++) {
            var anchor = strandAnchors[i];
            var anchorPoint = selfCollider.transform.TransformPoint(anchor.position);
            if (anchor.strand == null && other.ClosestPoint(anchorPoint) == anchorPoint) {
                anchor.strand = new FluidParticleSystemVerletStrand(selfCollider.transform, anchor.position,
                    other.transform, other.transform.InverseTransformPoint(anchorPoint), particleMaterial,
                    fluidParticleSystemSettings, decalableHitMask);
                FluidPass.AddParticleSystem(anchor.strand);
                OnFluidCollision(new FluidParticleSystem.ParticleCollision() {
                    collider = other,
                    color = fluidParticleSystemSettings.color,
                    heightStrength = fluidParticleSystemSettings.heightStrengthBase,
                    normal = Vector3.forward,
                    size = fluidParticleSystemSettings.splatSize,
                    position = anchorPoint,
                    stretch = Vector3.zero,
                });
                OnFluidCollision(new FluidParticleSystem.ParticleCollision() {
                    collider = selfCollider,
                    color = fluidParticleSystemSettings.color,
                    heightStrength = fluidParticleSystemSettings.heightStrengthBase,
                    normal = Vector3.forward,
                    size = fluidParticleSystemSettings.splatSize,
                    position = anchorPoint,
                    stretch = Vector3.zero,
                });
            }

            strandAnchors[i] = anchor;
        }
    }

    private void OnDrawGizmosSelected() {
        if (strandAnchors == null) {
            return;
        }

        selfCollider = GetComponentInChildren<Collider>();
        if (selfCollider == null) {
            return;
        }

        foreach (var anchor in strandAnchors) {
            Gizmos.DrawWireSphere(selfCollider.transform.TransformPoint(anchor.position), 0.1f);
        }
    }
}

}