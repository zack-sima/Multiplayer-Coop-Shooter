using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankAnimator : HullAnimatorBase {

	#region References

	[SerializeField] private Transform meshParent;
	[SerializeField] private TankTracksAnimator leftTracksAnimator, rightTracksAnimator;

	#endregion

	#region Members

	private float targetRotation = 0f;

	#endregion

	#region Functions

	protected override void Start() {
		base.Start();
	}

	protected override void Update() {
		base.Update();

		if (GetVelocity() != Vector3.zero) {
			Vector3 velocity = GetVelocity();

			if (GetTargetDirection() != Vector3.zero) velocity = GetTargetDirection();

			targetRotation = Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg;
		}

		//reversed set separatedly for PC clients
		bool reversed;
		if (GetInReverse() == -1) {
			reversed = Quaternion.Angle(transform.rotation, Quaternion.Euler(0, targetRotation, 0)) > 90;
		} else {
			reversed = GetInReverse() == 1;
		}
		if (reversed) {
			transform.rotation = Quaternion.RotateTowards(
				transform.rotation, Quaternion.Euler(0, targetRotation + 180, 0), 250f * Time.deltaTime);

			leftTracksAnimator.SetSpeed(-GetVelocity().magnitude);
			rightTracksAnimator.SetSpeed(-GetVelocity().magnitude);
		} else {
			transform.rotation = Quaternion.RotateTowards(
				transform.rotation, Quaternion.Euler(0, targetRotation, 0), 250f * Time.deltaTime);

			leftTracksAnimator.SetSpeed(GetVelocity().magnitude);
			rightTracksAnimator.SetSpeed(GetVelocity().magnitude);
		}
	}

	#endregion

}
