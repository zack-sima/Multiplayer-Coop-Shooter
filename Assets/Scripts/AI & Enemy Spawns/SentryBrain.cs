using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SentryBrain : MonoBehaviour {

	[SerializeField] private CombatEntity entity;
	[SerializeField] private float range = 10;

	private CombatEntity target = null;
	bool canShootTarget = false;

	private void SentryDecisionTick() {
		if (target != null && target.GetNetworker().GetIsDead()) {
			target = null;
		}

		//try finding target
		float closestDistance = range;
		foreach (CombatEntity ce in EntityController.instance.GetCombatEntities()) {
			if (!ce.GetNetworker().GetInitialized() ||
				ce.GetTeam() == entity.GetTeam() || ce.GetNetworker().GetIsDead()) continue;
			float distance = AIBrain.GroundDistance(ce.transform.position, transform.position);
			if (distance < closestDistance) {
				closestDistance = distance;
				target = ce;
			}
		}

		//line of sight to target check; raycast all prevents other AI from blocking line of sight
		canShootTarget = false;

		Vector3 directionToPlayer = target.transform.position - transform.position;
		directionToPlayer.y = 0;

		RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up,
			directionToPlayer.normalized, range);

		List<RaycastHit> hitsList = hits.ToList();
		hitsList.Sort((hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

		foreach (var hit in hitsList) {
			if (hit.collider.gameObject == target.gameObject) {
				canShootTarget = true;
				break;
			} else if (hit.collider.GetComponent<CombatEntity>() == null &&
				hit.collider.GetComponent<Bullet>() == null) {

				//If hit is not a combat entity, break
				break;
			}
		}
	}
	private IEnumerator Tick() {
		while (true) {
			//not client master
			if (!entity.GetNetworker().HasSyncAuthority()) {
				yield return new WaitForSeconds(1f);
				continue;
			}
			try {
				SentryDecisionTick();
			} catch (System.Exception e) { Debug.LogWarning(e); }

			yield return new WaitForSeconds(0.35f);
		}
	}

	void Start() {
		StartCoroutine(Tick());
	}
	void Update() {
		if (target == null) return;

		Vector3 targetPosition = target.transform.position;
		entity.GetTurret().SetTargetTurretRotation(
			Mathf.Atan2(targetPosition.x - transform.position.x,
			targetPosition.z - transform.position.z) * Mathf.Rad2Deg
		);
		if (canShootTarget) {
			entity.TryFireMainWeapon();
		}
	}
}
