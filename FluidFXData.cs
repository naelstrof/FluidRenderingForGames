using UnityEngine;
using UnityEngine.VFX;

class FluidFXData : VFXSpawnerCallbacks {
    
    private static readonly int PositionPropertyId = Shader.PropertyToID("ObjectPositionWS");
    private static readonly int ForwardPropertyId = Shader.PropertyToID("ObjectForwardWS");
    private static readonly int StrengthPropertyId = Shader.PropertyToID("ObjectStrength");

    private static readonly int OldEffectPositionAttributeId = Shader.PropertyToID("oldEffectPosition");
    private static readonly int OldEffectForwardAttributeId = Shader.PropertyToID("oldEffectForward");
    private static readonly int OldEffectStrengthAttributeId = Shader.PropertyToID("oldEffectStrength");

    private Vector3 oldPosition;
    private Vector3 oldForward;
    private float oldStrength;

    public class InputProperties {
        public Vector3 ObjectPositionWS = Vector3.zero;
        public Vector3 ObjectForwardWS = Vector3.zero;
        public float ObjectStrength = 0f;
    }

    public override void OnPlay(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent) {
        oldPosition = vfxValues.GetVector3(PositionPropertyId);
        oldForward = vfxValues.GetVector3(ForwardPropertyId);;
        oldStrength = vfxValues.GetFloat(StrengthPropertyId);;
    }

    public override void OnUpdate(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent) {
        //if (!state.playing || state.deltaTime == 0) return;

        state.vfxEventAttribute.SetVector3(OldEffectPositionAttributeId, oldPosition);
        state.vfxEventAttribute.SetVector3(OldEffectForwardAttributeId, oldForward);
        state.vfxEventAttribute.SetFloat(OldEffectStrengthAttributeId, oldStrength);
        oldPosition = vfxValues.GetVector3(PositionPropertyId);
        oldForward = vfxValues.GetVector3(ForwardPropertyId);
        oldStrength = vfxValues.GetFloat(StrengthPropertyId);
    }

    public override void OnStop(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent) {

    }
    
}