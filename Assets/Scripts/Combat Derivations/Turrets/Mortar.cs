using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Video;

public class Mortar : Turret {
    #region Statics & Consts

    private const float GRAVITY = .2f;

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
        if (asinVal < -1f || asinVal > 1f) return -45f;

        return  -(90f - (.5f * Mathf.Asin(asinVal) * 180 / Mathf.PI));
    }

    public void SetDistance(float d) {
        distance = d;
    }

	protected override void Update() { 
        base.Update();

        targetHeight = CalculateLaunchAngle(15f, distance);

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
