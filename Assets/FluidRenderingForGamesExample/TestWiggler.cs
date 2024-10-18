using FluidRenderingForGames;
using JigglePhysics;
using UnityEngine;
using UnityEngine.Serialization;

public class TestWiggler : MonoBehaviour {

    [FormerlySerializedAs("strengthCurve")] [SerializeField] private AnimationCurve velocityCurve;
    [SerializeField] private AnimationCurve volumeCurve;
    [SerializeField] private AnimationCurve stiffnessCurve;
    [SerializeField] private JiggleRigBuilder jiggleRigBuilder;
    [SerializeField] private JiggleSettingsBlend jiggleBlend;
    private Quaternion startRotation;

    private void Awake() {
        startRotation = transform.rotation;
        jiggleBlend = Instantiate(jiggleBlend);
        jiggleRigBuilder.jiggleRigs[0].jiggleSettings = jiggleBlend;
    }

    void Update() {
        var velocity = velocityCurve.Evaluate(Mathf.Repeat(Time.timeSinceLevelLoad*1f, 1f));
        var volume = volumeCurve.Evaluate(Mathf.Repeat(Time.timeSinceLevelLoad*1f, 1f));
        var stiffness = stiffnessCurve.Evaluate(Mathf.Repeat(Time.timeSinceLevelLoad*1f, 1f));
        GetComponentInChildren<FluidEmitter>().SetVelocityMultiplier(velocity);
        GetComponentInChildren<FluidEmitter>().setHeightStrengthMultiplier(volume);
        transform.rotation = startRotation * Quaternion.Euler(0f, 30f * Mathf.PerlinNoise(Time.timeSinceLevelLoad * 0.8f, -Time.timeSinceLevelLoad * 1.11f), 0f);
        jiggleBlend.normalizedBlend = Mathf.Clamp01(stiffness);
    }

}
