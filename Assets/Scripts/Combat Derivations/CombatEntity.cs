using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatEntity : Entity {
	//TODO: when spawning player/enemies, etc, player spawns a combat entity shell and spawns
	//specific hull/turret prefabs to populate it; separate all animators from shell structure

	#region References

	[SerializeField] private Hull hull; //for movement
	public Hull GetHull() { return hull; }

	[SerializeField] private Turret turret; //for combat
	public Turret GetTurret() { return turret; }

	[SerializeField] private Transform movementMarker; //movement indicator

	#endregion

	#region Members

	//when set rotation/maintain rotation hasn't been called
	private bool turretFollowsMovement = false;

	//set this back to time.time when rotation is called on mobile
	private float freezeTurretTimestamp = -10f;
	public bool GetTurretFollowsMovement() {
		return turretFollowsMovement && (Time.time - freezeTurretTimestamp > 0.1f);
	}

	//autoaim target
	private CombatEntity target = null;
	private float targetFindTimer = 0f;

	//unique bulletid
	private int bulletsFired = 0;

	//prevent double blow
	private bool blownUp = false;

	//keeps track of bullets spawned so when an RPC is called to destroy nonlocal
	private readonly Dictionary<int, GameObject> bullets = new();
	public void AddBullet(int bulletId, GameObject bulletRef) {
		bullets.TryAdd(bulletId, bulletRef);
	}
	public void RemoveBullet(int bulletId) {
		if (bullets.ContainsKey(bulletId)) {
			GameObject bullet = bullets[bulletId];
			bullets.Remove(bulletId);

			//NOTE: this must be after removing the bullet from the dictionary
			//because DestroyBulletEffect attempts to remove the bullet again (for non-local players)
			if (bullet != null) {
				bullet.GetComponent<Bullet>().DestroyBulletEffect();
			}
		}
	}

	#endregion

	#region Functions

	#region Mobile Controls

	public void MaintainTurretRotation() {
		freezeTurretTimestamp = Time.time;
	}
	//when set to true, whenever entity turret is not being set it will rotate with its movement
	public void SetTurretFollowsMovement(bool follow) {
		turretFollowsMovement = follow;
	}

	#endregion

	//non-local bullet just for decorative purposes
	public void NonLocalFireMainWeapon(int bulletId) {
		GameObject b = turret.NonLocalFireWeapon(this, GetTeam(), bulletId);

		if (b == null) return;

		AddBullet(bulletId, b);
	}
	//the one that actually creates a bullet that matters
	public void TryFireMainWeapon() {
		if (!turret.gameObject.activeInHierarchy) return;

		GameObject b = turret.TryFireMainWeapon(GetTeam(), bulletsFired, optionalSender: this);

		if (b == null) return;

		PreventHealing();
		AddBullet(bulletsFired, b);

		GetNetworker().RPC_FireWeapon(bulletsFired);

		bulletsFired++;
	}
	//blows up a radius (kinda fun to use lol)
	public void BlowUp() {
		if (blownUp) return;
		blownUp = true;

		float radius = turret.GetExplosionRadius();
		foreach (CombatEntity e in new List<CombatEntity>(EntityController.instance.GetCombatEntities())) {
			if (e == null || e == this || !e.GetNetworker().GetInitialized()) continue;

			float dist = Vector3.Distance(transform.position, e.gameObject.transform.position);

			try { //for some reason, GetIsDead() throws error in chain blow up sometimes
				if (e.GetNetworker().GetIsDead() || dist > radius) continue;
			} catch { continue; }

			e.GetNetworker().RPC_TakeDamage(e.GetNetworker().Object,
				turret.GetExplosionDamage() * Mathf.Min(radius - dist + radius / 2f, radius) / radius);
		}
	}
	public override void EntityRemoved() {
		base.EntityRemoved();

		if (turret.GetIsProximityExploder() && GetNetworker().HasSyncAuthority()) {
			try {
				BlowUp();
			} catch (System.Exception e) { Debug.LogWarning(e); }
		}
	}
	public override void RespawnEntity() {
		base.RespawnEntity();
		hull.gameObject.SetActive(true);
		hull.GetAnimator().Teleported();
		turret.gameObject.SetActive(true);
		turret.GetAnimator().ResetAnimations();
	}
	public override void DisableEntity() {
		base.DisableEntity();
		hull.gameObject.SetActive(false);
		turret.gameObject.SetActive(false);
	}
	public override void SetTeam(int newTeam) {
		base.SetTeam(newTeam);

		Material teamMat = GetTeamMaterials().GetTeamColor(newTeam);
		hull.GetAnimator().SetTeamMaterial(teamMat);
		turret.GetAnimator().SetTeamMaterial(teamMat);
	}
	public override void AddEntityToRegistry() {
		EntityController.instance.AddToCombatEntities(this);
	}
	public override void RemoveEntityFromRegistry() {
		EntityController.instance.RemoveFromCombatEntities(this);
	}

	protected override void Update() {
		base.Update();

		if (GetTurretFollowsMovement() && TryGetComponent(out Rigidbody rb)) {
			//try to auto-aim towards nearest target within screen before following hull

			if (targetFindTimer > 0) {
				targetFindTimer -= Time.deltaTime;
			} else {
				targetFindTimer = 0.25f;
				float closestDistance = 15f;
				target = null;
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
				turret.SetTargetTurretRotation(
					Mathf.Atan2(target.transform.position.x - transform.position.x,
					target.transform.position.z - transform.position.z) * Mathf.Rad2Deg
				);
			} else {
				if (rb.velocity != Vector3.zero)
					turret.SetTargetTurretRotation(Mathf.Atan2(
						rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg, slow: true);
			}
		}
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
	}

	#endregion

}
