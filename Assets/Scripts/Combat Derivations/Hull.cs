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

	public enum HullType { TankDrive, SpiderDrive }

	[SerializeField] private float speed;
	public float GetSpeed() { return speed; }

	[SerializeField] private float acceleration;

	[SerializeField] private HullType hullType;
	public HullType GetHullType() { return hullType; }

	[SerializeField] private int baseHealth = 5000;
	public int GetBaseHealth() { return baseHealth; }

	private Vector3 currentVelocity = Vector3.zero;
	private Vector3 lastDirection = Vector3.zero;

	private Vector3 physicsVelocity = Vector3.zero;

	#endregion

	#region Functions

	//NOTE: movement can also be performed by AINavigator for AI combat entities that have a NavMeshAgent
	//NOTE: inReverse is for the local player only so that they reverse properly on PC
	public void Move(Vector3 direction, int inReverse = -1) {
		if (rootTransform == null) return;

		float maxSpeed = animator.GetInReverse() == 1 ? speed * 0.85f : speed;

		currentVelocity = Vector3.MoveTowards(currentVelocity, maxSpeed * direction, acceleration * Time.deltaTime);

		//if -1 don't set inReverse
		if (inReverse != -1 && direction != Vector3.zero) {
			animator.SetInReverse(inReverse == 1);
			animator.SetTargetDirection(direction);
		}

		if (currentVelocity != Vector3.zero && direction == Vector3.zero) direction = lastDirection;

		if (optionalRigidbody == null) {
			rootTransform.Translate(currentVelocity * Time.deltaTime);
		} else {
			optionalRigidbody.velocity = physicsVelocity + currentVelocity;
		}

		if (direction != Vector3.zero) lastDirection = direction;

		physicsVelocity = Vector3.zero;
	}
	//for player squishing (call BEFORE move function, one for each!)
	public void SquishPhysics(List<CombatEntity> otherEntities, CombatEntity self) {
		if (self.GetNetworker().GetIsDead()) return;
		foreach (CombatEntity e in otherEntities) {
			if (e == null || e == self || e.GetNetworker().GetIsDead()) continue;
			TrySimulatePush(e.transform.position);
		}
	}
	public void SetAsAI() {
		animator.SetAsAI();
	}
	private void TrySimulatePush(Vector3 other) {
		float threshold = 1.8f;
		float distance = AIBrain.GroundDistance(transform.position, other);
		if (distance < threshold)
			physicsVelocity += (threshold - distance) * 5f * (transform.position - other).normalized;
		physicsVelocity.y = 0;
	}

	#endregion

}
