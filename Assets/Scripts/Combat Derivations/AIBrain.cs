using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Autonomously decides on an AI enemy's course of action
/// </summary>

public class AIBrain : MonoBehaviour {

	#region References

	[SerializeField] private Collider hitbox; //own hitbox, turn off when doing raycast
	[SerializeField] private AINavigator navigator;
	[SerializeField] private CombatEntity entity;

	#endregion

	#region Members

	[SerializeField] private float maxRange;

	//turret rotates towards target and shoots if there is a line of sight in range
	private CombatEntity target = null;
	private bool canShootTarget = false;

	#endregion

	#region Functions

	private float GroundDistance(Vector3 a, Vector3 b) {
		return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
	}

	private IEnumerator Tick() {
		while (true) {
			//TODO: enemies should "lose interest" if a direct line of sight can't be established;
			//  when no target exists, enemy should wander in a random location (when not moving,
			//  it should try to sample a random position within the map for ~10 times until it founds one)
			//TODO: enemies should not engage in shooting unless the target rotation is within 10˚ of current rotation
			//TODO: check that in networking, AI has authority to trace enemies, etc
			if (target != null) {
				navigator.SetTarget(target.transform.position);

				//try raycasting to target
				canShootTarget = false;
				bool veryCloseToTarget = GroundDistance(transform.position, target.transform.position) < 3f;

				//line of sight to target check; TODO: prevent other AI from blocking line of sight
				Vector3 directionToPlayer = target.transform.position - transform.position;

				hitbox.enabled = false;
				//TODO: disable all hitboxes of behaviours of the same team
				if (Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hit, maxRange)) {
					if (hit.collider.gameObject == target.gameObject) {
						canShootTarget = true;
					}
				}
				hitbox.enabled = true;

				navigator.SetStopped(veryCloseToTarget || canShootTarget);
			} else {
				navigator.SetStopped(true);
			}

			yield return new WaitForSeconds(0.25f);
		}
	}
	private void Update() {
		if (!entity.GetNetworker().Runner.IsSharedModeMasterClient) return;

		if (target == null) {
			target = EntityController.player;
		} else {
			entity.GetTurret().SetTargetTurretRotation(
				Mathf.Atan2(target.transform.position.x - transform.position.x,
				target.transform.position.z - transform.position.z) * Mathf.Rad2Deg
			);
		}
		if (canShootTarget) {
			entity.TryFireMainWeapon();
		}
	}
	private void Start() {
		navigator.SetRotatable(false);
		navigator.SetSpeed(5f);

		StartCoroutine(Tick());
	}

	#endregion

}
