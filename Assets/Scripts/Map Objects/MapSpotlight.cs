using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpotlight : MonoBehaviour {

	private Vector3 localOffset;

	private void Start() {
		localOffset = transform.localEulerAngles;
	}
	void Update() {
		if (GameStatsSyncer.instance == null) return;

		transform.localEulerAngles = localOffset + new Vector3(0,
			Mathf.Sin(GameStatsSyncer.instance.GetLocalTime() / 3f) * 70f, 0);
	}
}
