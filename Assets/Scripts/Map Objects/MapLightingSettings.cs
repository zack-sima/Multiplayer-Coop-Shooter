using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLightingSettings : MonoBehaviour {

	void Start() {
		Light light = GetComponent<Light>();
		if (light == null) return;

		//0 is standard bright, -1 dim, -2 more bright
		switch (PlayerPrefs.GetInt("light_brightness")) {
			case -1:
				light.intensity -= 0.25f;
				break;
			case 1:
				light.intensity += 0.25f;
				break;
		}
	}
}
