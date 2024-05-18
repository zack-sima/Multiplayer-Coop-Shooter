using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuRedLight : MonoBehaviour {
	Light light;
	float originalIntensity = 1;

	void Start() {
		light = GetComponent<Light>();
		originalIntensity = light.intensity;
	}

	void Update() {
		light.intensity = originalIntensity * (1f + Mathf.Sin(Time.time / 1.5f) * 0.3f);
	}
}
