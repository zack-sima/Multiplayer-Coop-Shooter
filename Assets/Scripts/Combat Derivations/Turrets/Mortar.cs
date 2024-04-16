using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Video;

public class Mortar : Turret {

	#region Statics & Consts

	#endregion

	#region References

	[SerializeField] private GameObject mortarCore;

	#endregion

	#region Members

	private float targetHeight = -45; // degrees
	private float elevationRate = 90f;

	private float distance = 10f;

	#endregion

	#region Functions

	private static float CalculateLaunchAngle(float velocity, float distance) {

		float asinVal = Mathf.Abs(Physics.gravity.y) * distance / Mathf.Pow(velocity, 2);
		if (asinVal < -1f || asinVal > 1f) return -58.5f; //with 0.3f adjustment -- 45f * 1.3f

		float angle = 90f - (.5f * Mathf.Asin(asinVal) * 180 / Mathf.PI);

		//NOTE: linear adjustment because ground is not level
		angle += (90f - angle) * 0.3f;

		return -angle;
	}

	public void SetDistance(float d) {
		distance = d;
	}

	protected override void Update() {
		base.Update();

		targetHeight = CalculateLaunchAngle(GetBulletPrefab().GetComponent<Lobbing>().GetSpeed(), distance);

		try {
			if (transform.rotation != Quaternion.identity) {
				Quaternion targetRot = Quaternion.Euler(transform.eulerAngles.x + targetHeight,
					transform.eulerAngles.y, transform.eulerAngles.z);

				if (targetRot != Quaternion.identity)
					mortarCore.transform.rotation = Quaternion.RotateTowards(mortarCore.transform.rotation, targetRot, elevationRate);
			}
		} catch { }

		//targetHeight += 10 * Time.deltaTime;
		//mortarCore.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x + 90, transform.rotation.eulerAngles.y, 0);
		//Pivot barrel up and down to hit a position.
	}

	#endregion

}
