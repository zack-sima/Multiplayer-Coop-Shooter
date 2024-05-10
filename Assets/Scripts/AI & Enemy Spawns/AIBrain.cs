using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

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

	//if is PVP bot, execute a different set of behaviors (at least don't always shoot)
	[SerializeField] private bool isPVPBot;

	//for bot intermittent shooting
	private bool inPausePhase = false;
	private float botPhaseTimer = 1f;

	//turret rotates towards target and shoots if there is a line of sight in range
	private CombatEntity target = null;
	private bool canShootTarget = false;

	#endregion

	#region Functions

	public static float GroundDistance(Vector3 a, Vector3 b) {
		return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
	}
	private void EnemyDecisionTick() {
		if (entity.GetNetworker().GetIsDead()) return;

		if (target != null && target.GetNetworker().GetIsDead()) {
			canShootTarget = false;
			target = null;
		}

		///TODO: something is wrong here about enemy init search! PRINT OUT LIST OF AVAILABLE ENTITIES

		//wait for NavMeshAgent to initialize
		if (!navigator.GetIsNavigable()) return;

		//try finding target
		//if (target == null) {
		float closestDistance = 999;
		foreach (CombatEntity ce in EntityController.instance.GetCombatEntities()) {
			if (!ce.GetNetworker().GetInitialized() ||
				ce.GetTeam() == entity.GetTeam() || ce.GetNetworker().GetIsDead()) continue;
			float distance = Vector3.Distance(ce.transform.position, transform.position);
			if (distance < closestDistance) {
				closestDistance = distance;
				target = ce;
			}
		}
		//}
		//TODO: enemies should "lose interest" if a direct line of sight can't be established;
		//  when no target exists, enemy should wander in a random location (when not moving,
		//  it should try to sample a random position within the map for ~10 times until it founds one)
		//TODO: enemies should not engage in shooting unless the target rotation is within 10˚ of current rotation
		if (target != null) {
			navigator.SetTarget(target.transform.position);

			//try raycasting to target
			canShootTarget = false;
			bool veryCloseToTarget = GroundDistance(transform.position, target.transform.position) < 3f;

			if (entity.GetTurret() is Mortar) {
				//mortar can shoot over obstacles
				canShootTarget = GroundDistance(transform.position, target.transform.position) < 13.5f;
			} else {
				//line of sight to target check; raycast all prevents other AI from blocking line of sight
				Vector3 directionToPlayer = target.transform.position - transform.position;
				directionToPlayer.y = 0;

				RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up,
					directionToPlayer.normalized, maxRange);

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
			if (entity.GetTurret().GetIsProximityExploder()) canShootTarget = false;
			navigator.SetStopped(veryCloseToTarget || canShootTarget);
		} else {
			if (PlayerInfo.GetIsPVP()) {
				//go to other side if nothing to do
				navigator.SetTarget(MapController.instance.GetTeamSpawnpoint((entity.GetTeam() + 1) % 2));
			} else {
				navigator.SetStopped(true);
			}
		}
	}
	private IEnumerator Tick() {
		while (true) {
			//not client master
			if (!entity.GetNetworker().HasSyncAuthority()) {
				navigator.SetActive(false);
				yield return new WaitForSeconds(1f);
				continue;
			} else navigator.SetActive(true);

			try {
				EnemyDecisionTick();
			} catch (System.Exception e) { Debug.LogWarning(e); }

			yield return new WaitForSeconds(0.35f);
		}
	}
	private void Update() {
		if (!entity.GetNetworker().HasSyncAuthority()) return;
		if (entity.GetNetworker().GetIsDead()) { return; }
		if (target == null) return;

		if (entity.GetTurret().GetIsRotatable()) {
			entity.GetTurret().SetTargetTurretRotation(
				Mathf.Atan2(target.transform.position.x - transform.position.x,
				target.transform.position.z - transform.position.z) * Mathf.Rad2Deg
			);
		}

		navigator.SetActive(true);

		if (entity.GetTurret() is Mortar)
			((Mortar)entity.GetTurret()).SetDistance(GroundDistance(transform.position, target.transform.position));

		if (!entity.GetTurret().GetIsProximityExploder() && canShootTarget) {
			//regulate shooting
			if (isPVPBot) {
				botPhaseTimer -= Time.deltaTime;
				if (botPhaseTimer <= 0f) {
					botPhaseTimer = inPausePhase ? Random.Range(1f, 1.5f) : Random.Range(0.5f, 0.7f);
					inPausePhase = !inPausePhase;
				}
				if (!inPausePhase) entity.TryFireMainWeapon();
			} else {
				entity.TryFireMainWeapon();
			}
		} else if (entity.GetTurret().GetIsProximityExploder() &&
			GroundDistance(target.transform.position, transform.position) < 2.5f) {

			//same sender as target
			entity.GetNetworker().RPC_TakeDamage(entity.GetNetworker().Object,
				entity.GetNetworker().Object, entity.GetMaxHealth(), 0);
		} else if (entity.GetTurret().GetIsProximityExploder() && //close enough to walk straight
			GroundDistance(target.transform.position, transform.position) < 5f) {
			navigator.SetActive(false);

			Vector3 newPos = Vector3.MoveTowards(transform.position, target.transform.position,
				navigator.GetSpeed() * Time.deltaTime);

			transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
		}
	}
	private void Start() {
		navigator.SetRotatable(false);
		navigator.SetSpeed(5f);

		StartCoroutine(Tick());
	}

	#endregion

}
