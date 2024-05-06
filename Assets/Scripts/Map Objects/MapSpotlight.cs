using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpotlight : MonoBehaviour {
	void Update() {
		if (GameStatsSyncer.instance == null) return;

		transform.localEulerAngles = new Vector3(transform.localEulerAngles.x,
			GameStatsSyncer.instance.GetLocalTime() / 3f % 360, transform.localEulerAngles.z);
	}
}
