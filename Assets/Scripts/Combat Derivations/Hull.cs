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

	private Vector3 currentVelocity = Vector3.zero;
	private Vector3 lastDirection = Vector3.zero;

	#endregion

	#region Functions

	//NOTE: movement can also be performed by AINavigator for AI combat entities that have a NavMeshAgent
	//NOTE: inReverse is for the local player only so that they reverse properly on PC
	public void Move(Vector3 direction, int inReverse = -1) {
		if (rootTransform == null) return;

		currentVelocity = Vector3.MoveTowards(currentVelocity, speed * direction, acceleration * Time.deltaTime);

		//if -1 don't set inReverse
		if (inReverse != -1 && direction != Vector3.zero) {
			animator.SetInReverse(inReverse == 1);
			animator.SetTargetDirection(direction);
		}

		if (currentVelocity != Vector3.zero && direction == Vector3.zero) direction = lastDirection;

		if (optionalRigidbody == null) {
			rootTransform.Translate(currentVelocity * Time.deltaTime);
		} else {
			optionalRigidbody.velocity = currentVelocity;
		}

		if (direction != Vector3.zero) lastDirection = direction;
	}

	#endregion

}
