using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverAnimator : HullAnimatorBase {

	#region References

	[SerializeField] private List<Transform> blades;

	#endregion

	#region Members

	private float targetRotation = 0f;

	//if in menu scene, don't do weird stuff
	private bool moved = false;

	#endregion

	#region Functions

	protected override void Start() {
		base.Start();
	}

	protected override void Update() {
		base.Update();

		foreach (Transform t in blades) {
			t.Rotate(0f, 0f, Time.deltaTime * 500f);
		}

		if (GetVelocity() != Vector3.zero) {
			moved = true;

			Vector3 velocity = GetVelocity();

			if (GetTargetDirection() != Vector3.zero) velocity = GetTargetDirection();

			targetRotation = Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg;
		}

		if (!moved) return;

		//reversed set separatedly for PC clients
		bool reversed;
		if (GetInReverse() == -1) {
			reversed = Quaternion.Angle(transform.rotation, Quaternion.Euler(0, targetRotation, 0)) > 105f;
		} else {
			reversed = GetInReverse() == 1;
		}
		//bots should always drive forward
		if (isBot) reversed = false;

		float velocityMult = Time.deltaTime * Mathf.Min(1f, GetVelocity().magnitude * 0.25f);
		if (!UIController.GetIsMobile()) velocityMult = 1;

		if (reversed) {
			transform.rotation = Quaternion.RotateTowards(
				transform.rotation, Quaternion.Euler(0, targetRotation + 180, 0), 250f * velocityMult);
		} else {
			transform.rotation = Quaternion.RotateTowards(
				transform.rotation, Quaternion.Euler(0, targetRotation, 0), 250f * velocityMult);
		}
	}

	#endregion

}
