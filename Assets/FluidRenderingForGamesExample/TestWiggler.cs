using FluidRenderingForGames;
using JigglePhysics;
using UnityEngine;
public class TestWiggler : MonoBehaviour {

    [SerializeField] private AnimationCurve strengthCurve; 
    [SerializeField] private JiggleSettingsBlend jiggleBlend; 
    private Quaternion startRotation;
    private float pulse;

    private void Awake() {
        startRotation = transform.rotation;
    }

    void Update() {
        pulse = strengthCurve.Evaluate(Mathf.Repeat(Time.timeSinceLevelLoad*1f, 1f));
        GetComponentInChildren<FluidEmitter>().SetVelocityMultiplier(pulse);
        GetComponentInChildren<FluidEmitter>().setHeightStrengthMultiplier(Mathf.Pow(pulse, 0.3f));
        transform.rotation = startRotation * Quaternion.Euler(0f, 30f * Mathf.PerlinNoise(Time.timeSinceLevelLoad * 0.8f, -Time.timeSinceLevelLoad * 1.11f), 0f);
        jiggleBlend.normalizedBlend = Mathf.Clamp01(strengthCurve.Evaluate(Mathf.Repeat((Time.timeSinceLevelLoad+0.2f)*1f, 1f)));
    }

}
