using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hull : MonoBehaviour {

	#region References

	//NOTE: if rigidbody doesn't exist, use transform for all movements
	private Rigidbody optionalRigidbody = null;
	public void SetRigidbody(Rigidbody rb) { optionalRigidbody = rb; }

	//move this root transform (which could have a rigidbody) but rotate locally
	private Transform rootTransform = null;
	public void SetRootTransform(Transform t) { rootTransform = t; }

	//where the turret is placed at, plus an optional turret offset set in turret settings
	[SerializeField] private Transform turretAnchor;
	public Vector3 GetTurretAnchorPosition() { return turretAnchor.position; }

	[SerializeField] private HullAnimatorBase animator;
	public HullAnimatorBase GetAnimator() { return animator; }

	#endregion

	#region Members

	[SerializeField] private float speed;
	public float GetSpeed() { return speed; }

	[SerializeField] private float acceleration;

	private float currentSpeed = 0f;
	private Vector3 lastDirection = Vector3.zero;

	#endregion

	#region Functions

	//NOTE: movement can also be performed by AINavigator for AI combat entities that have a NavMeshAgent
	public void Move(Vector3 direction) {
		if (rootTransform == null) return;

		if (currentSpeed < speed * direction.magnitude) {
			currentSpeed = Mathf.Min(currentSpeed + Time.deltaTime * acceleration, speed * direction.magnitude);
		}
		if (currentSpeed > speed * direction.magnitude) {
			currentSpeed = Mathf.Max(currentSpeed - Time.deltaTime * acceleration, speed * direction.magnitude);
		}
		if (currentSpeed != 0 && direction == Vector3.zero) direction = lastDirection;

		if (optionalRigidbody == null) {
			rootTransform.Translate(currentSpeed * direction.normalized);
		} else {
			optionalRigidbody.velocity = currentSpeed * direction.normalized;
		}

		if (direction != Vector3.zero) lastDirection = direction;
	}

	#endregion

}
