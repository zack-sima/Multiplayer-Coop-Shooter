using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mortar : Turret {
    #region Statics & Consts

    private const float GRAVITY = .2f;

	#endregion

	#region References

    [SerializeField] private GameObject mortarCore;

	#endregion

	#region Members

    private float targetDistance = 5f;
    private float targetHeight = -45; // degrees
    private float elevationRate = 90f;

	#endregion

	#region Functions

    private static float CalculateLaunchAngle(float velocity, float distance) {
        if (distance > 10f) return -45f;
        return -(90f - (.5f * Mathf.Asin(10f * distance / Mathf.Pow(velocity, 2)) * 180 / Mathf.PI));
    }

    public void SetTargetLocation(Vector3 location) {
        targetDistance = Vector3.Distance(location, transform.position);
    }

    private void Awake() {

    }

	private new void Update() { 
        if (shootTimer > 0) shootTimer -= Time.deltaTime;

		if (useTargetRotation) {
			transform.rotation = Quaternion.RotateTowards(transform.rotation,
				Quaternion.Euler(0, targetRotation + 90, 0),
				Time.deltaTime * rotateSpeed * (inSlowMode ? 0.55f : 1f));
		}
        targetHeight = CalculateLaunchAngle(15f, targetDistance);
        mortarCore.transform.rotation = Quaternion.RotateTowards(mortarCore.transform.rotation, 
            Quaternion.Euler(transform.rotation.eulerAngles.x + targetHeight, transform.rotation.eulerAngles.y - 90, 0), elevationRate);
        //targetHeight += 10 * Time.deltaTime;
        //mortarCore.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x + 90, transform.rotation.eulerAngles.y, 0);
        //Pivot barrel up and down to hit a position.
    }

	#endregion
}
