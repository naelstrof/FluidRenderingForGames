using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour {
    void Update() {
        transform.rotation = Quaternion.Euler(0f, Time.timeSinceLevelLoad*0.1f*360f, 0f);
    }
}
