using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Rendering;
using Abilities.Internal;
using Abilities.Manager;

public class HumanCombatEntity : CombatEntity {

	#region References

	[SerializeField] private Transform movementMarker; //mobile movement indicator
	[SerializeField] private Transform aimMarker, lobMarker; //mobile aim indicator

	#endregion

	//autoaim target
	private CombatEntity target = null;
	private float targetFindTimer = 0f;

	#region Abilities

	private float totalDmg = 0;

	private List<IAbility> abilities = new();

	public override void IncrementDamageCharge(float dmgDone) { 
		totalDmg += dmgDone;
		Debug.Log(totalDmg);
	}

	

	//public float GetDamageCharge();

	private void UpdateAbilities() {
		abilities.ManageAbilities(this);
	}
	
	#endregion

	protected override void Update() {
		if (GetTurretFollowsMovement() && TryGetComponent(out Rigidbody rb)) {
			//try to auto-aim towards nearest target within screen before following hull
			if (targetFindTimer > 0) {
				targetFindTimer -= Time.deltaTime;
			} else {
				float maxDistance = 16f;

				//prioritize existing target (don't switch unless much closer)
				float closestDistance = target != null ?
					Vector3.Distance(target.transform.position, transform.position) - 5f : maxDistance;

				//if trigger is held down, don't re-autoaim
				if (HumanInputs.instance.GetPlayerShooting() && target != null)
					closestDistance = -1;

				foreach (CombatEntity ce in EntityController.instance.GetCombatEntities()) {
					if (!ce.GetNetworker().GetInitialized() || ce.GetTeam() == GetTeam() ||
						ce.GetNetworker().GetIsDead()) continue;
					float distance = Vector3.Distance(ce.transform.position, transform.position);
					if (distance < closestDistance) {
						closestDistance = distance;
						target = ce;
					}
				}
				targetFindTimer = 0;
			}
			if (target != null) {
				Vector3 targetPosition = target.transform.position;

				if (GetTurret() is Mortar && !HumanInputs.instance.GetIsMobileAiming()) {
					//TODO: mortar aim adjustment is shit (not used rn)
					//NOTE: rough estimate that it takes ~1s to land and don't overpredict -- only for bombs
					//if (target.GetTurret().GetIsProximityExploder())
					//	targetPosition += target.GetVelocity() * 1.2f;

					((Mortar)GetTurret()).SetDistance(Vector3.Distance(transform.position, target.transform.position));
				}
				GetTurret().SnapToTargetRotation(
					Mathf.Atan2(targetPosition.x - transform.position.x,
					targetPosition.z - transform.position.z) * Mathf.Rad2Deg, false
				);
			} else {
				//turret follows movement
				if (rb.velocity != Vector3.zero)
					GetTurret().SetTargetTurretRotation(Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg);
			}
		}
		//ammo display for local player
		if (GetIsPlayer() && GetNetworker().HasSyncAuthority()) {
			if (GetTurret().GetIsFullAuto()) {
				GetHealthCanvas().GetAmmoGrowBar().localScale = Vector2.zero;
				GetHealthCanvas().GetAmmoBar().localScale = new Vector2(
					(float)PlayerInfo.instance.GetAmmoLeft() / PlayerInfo.instance.GetMaxAmmo(), 1f);
			} else {
				//shows not full ammo ammo in gray
				GetHealthCanvas().GetAmmoGrowBar().localScale = new Vector2(
					(float)PlayerInfo.instance.GetAmmoLeft() / PlayerInfo.instance.GetMaxAmmo(), 1f);
				GetHealthCanvas().GetAmmoBar().localScale = new Vector2(
					(int)PlayerInfo.instance.GetAmmoLeft() / (float)PlayerInfo.instance.GetMaxAmmo(), 1f);
			}
		}
		//aim marker
		if (UIController.GetIsMobile()) {
			if (GetIsPlayer() && GetNetworker().HasSyncAuthority() &&
				HumanInputs.instance.GetIsMobileAiming()) {

				if (GetTurret() is Mortar) { //NOTE: lobbing
					lobMarker.gameObject.SetActive(true);
					aimMarker.gameObject.SetActive(false);

					((Mortar)GetTurret()).SetDistance(HumanInputs.instance.GetMobileLobDistance());
					lobMarker.transform.rotation = Quaternion.Euler(0, GetTurret().transform.eulerAngles.y, 0);
					lobMarker.transform.localScale = new Vector3(1f, 1f, HumanInputs.instance.GetMobileLobDistance() / 7f);
				} else {
					//aim raycast
					Vector3 direction = GetTurret().transform.forward;

					float aimDistance = 12f;

					RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up, direction, aimDistance);

					List<RaycastHit> hitsList = hits.ToList();
					hitsList.Sort((hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

					foreach (var hit in hitsList) {
						//hit something that's not a teammate or bullet
						if ((!hit.collider.TryGetComponent(out CombatEntity e) || e.GetTeam() != GetTeam()) &&
							hit.collider.GetComponent<Bullet>() == null) {
							aimDistance = hit.distance;
							break;
						}
					}
					lobMarker.gameObject.SetActive(false);
					aimMarker.gameObject.SetActive(true);
					aimMarker.GetChild(0).localScale = new Vector3((aimDistance - 1f) / 100f, 1f / 100f, 1f);
					aimMarker.eulerAngles = new Vector3(0, GetTurret().transform.eulerAngles.y - 90, 0);
				}
			} else {
				aimMarker.gameObject.SetActive(false);
				lobMarker.gameObject.SetActive(false);
			}
		} else {
			//PC
			if (GetTurret() is Mortar) {
				((Mortar)GetTurret()).SetDistance(Vector3.Distance(transform.position, HumanInputs.instance.GetMouseWorldPos()) + 1f);
			}
		}
		//movement marker
		if (movementMarker != null && UIController.GetIsMobile()) {
			if (GetIsPlayer() && GetNetworker().HasSyncAuthority()) {
				if (TryGetComponent(out Rigidbody rb2) && rb2.velocity != Vector3.zero &&
					GetNetworker().GetInitialized() && !GetNetworker().GetIsDead()) {
					movementMarker.localPosition = 0.35f * new Vector3(rb2.velocity.x, 0, rb2.velocity.z);
					movementMarker.gameObject.SetActive(true);
				} else {
					movementMarker.gameObject.SetActive(false);
				}
			} else if (movementMarker.gameObject.activeInHierarchy) {
				movementMarker.gameObject.SetActive(false);
			}
		}
		base.Update();
	}
}
