using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FluidDeltaData : MonoBehaviour {
    
    private Vector3 previousWorldPosition;
    private Vector3 previousWorldForward;

    private void Awake() {
        previousWorldPosition = transform.position;
        previousWorldForward = transform.forward;
        GetComponent<VisualEffect>().pause = true;
    }

    void FixedUpdate() {
        GetComponent<VisualEffect>().SetVector3("PositionDelta", transform.position-previousWorldPosition);
        GetComponent<VisualEffect>().SetVector3("ForwardDelta", transform.forward-previousWorldForward);
        previousWorldPosition = transform.position;
        previousWorldForward = transform.forward;
        GetComponent<VisualEffect>().Simulate(Time.fixedDeltaTime);
    }
}
