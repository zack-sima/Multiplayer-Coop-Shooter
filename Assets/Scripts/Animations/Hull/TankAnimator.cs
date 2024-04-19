using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankAnimator : HullAnimatorBase {

	#region References

	[SerializeField] private Transform meshParent;

	#endregion

	#region Members

	private float targetRotation = 0f;
	private Vector3 originalPosition = Vector3.zero;

	#endregion

	#region Functions

	protected override void Start() {
		base.Start();

		originalPosition = meshParent.localPosition;
	}

	protected override void Update() {
		base.Update();

		if (GetVelocity() != Vector3.zero)
			targetRotation = Mathf.Atan2(GetVelocity().x, GetVelocity().z) * Mathf.Rad2Deg;

		transform.rotation = Quaternion.RotateTowards(
			transform.rotation, Quaternion.Euler(0, targetRotation, 0), 250f * Time.deltaTime);

		meshParent.localPosition = originalPosition +
			0.01f * GetVelocity().magnitude * Random.insideUnitSphere;
	}

	#endregion

}
