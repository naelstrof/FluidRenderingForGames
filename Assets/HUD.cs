using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        var slider = GetComponent<UIDocument>().rootVisualElement.Q<Slider>();
        slider.RegisterValueChangedCallback(SliderChanged);
        slider.value = 100f;
    }

    private void SliderChanged(ChangeEvent<float> evt) {
        Application.targetFrameRate = (int)evt.newValue;
    }
}
