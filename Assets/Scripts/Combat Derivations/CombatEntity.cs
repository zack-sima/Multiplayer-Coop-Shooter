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

	#endregion

	#region Members

	//when set rotation/maintain rotation hasn't been called
	private bool turretFollowsMovement = false;

	//set this back to time.time when rotation is called on mobile
	private float freezeTurretTimestamp = -10f;
	public bool GetTurretFollowsMovement() {
		return turretFollowsMovement && (Time.time - freezeTurretTimestamp > 0.5f);
	}

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

	//spawned in when instantiated; when a player switches turrets after enable/disable here
	List<PlayerInfo.TurretInfo> spawnedTurrets = new();

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

	//callback from networker
	public void TurretChanged(string newTurretName) {
		foreach (PlayerInfo.TurretInfo t in spawnedTurrets) {
			if (t.turretName == newTurretName) {
				t.turret.gameObject.SetActive(true);
				turret = t.turret;
			} else {
				t.turret.gameObject.SetActive(false);
			}
		}
		turret.GetAnimator().SetTeamMaterial(GetTeamMaterials().GetTeamColor(GetTeam()));

		if (GetIsPlayer()) {
			if (GetNetworker().HasSyncAuthority()) {
				GetHealthCanvas().UpdateAmmoTickerCount(turret.GetIsFullAuto() ? 0 : turret.GetMaxAmmo() - 1);
			} else {
				SetHealthCanvasToFallback();
			}
		}
	}

	//non-local bullet just for decorative purposes
	public void NonLocalFireMainWeapon(int bulletId) {
		List<GameObject> b = turret.NonLocalFireWeapon(this, GetTeam(), bulletId);

		if (b == null || b.Count == 0) return;

		foreach (GameObject g in b) AddBullet(bulletId, g);
	}
	//the one that actually creates a bullet that matters
	public void TryFireMainWeapon() {
		if (!turret.gameObject.activeInHierarchy) return;
		if (GetIsPlayer() && PlayerInfo.instance.GetAmmoLeft() < 1) return;

		List<GameObject> b = turret.TryFireMainWeapon(GetTeam(), bulletsFired, optionalSender: this);

		if (b == null || b.Count == 0) return;

		if (GetIsPlayer()) PlayerInfo.instance.ConsumeAmmo();

		PreventHealing();

		foreach (GameObject g in b) AddBullet(bulletsFired, g);

		GetNetworker().RPC_FireWeapon(bulletsFired);

		bulletsFired++;
	}
	//blows up a radius (kinda fun to use lol)
	public void BlowUp() {
		if (blownUp) return;
		blownUp = true;

		DamageHandler.DealExplosiveDamage(transform.position, turret.GetExplosionRadius(),
			turret.GetExplosionDamage(), canDamageTeam: true, self: this);
	}
	//only called by player functions
	public void SetName(string name) {
		GetHealthCanvas().GetNameGhostText().gameObject.SetActive(true);
		GetHealthCanvas().GetNameGhostText().text = name;
		GetHealthCanvas().GetNameText().text = name;
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
	protected override void Start() {
		if (!GetIsPlayer()) {
			base.Start();
			return;
		}
		//spawn in all the required turrets
		List<PlayerInfo.TurretInfo> turrets = PlayerInfo.instance.GetTurrets();

		//clear current turret
		if (turret != null) {
			Destroy(turret.gameObject);
		}
		foreach (PlayerInfo.TurretInfo t in turrets) {
			GameObject g = Instantiate(t.turret.gameObject, transform);
			g.transform.SetLocalPositionAndRotation(t.localPositionOffset, Quaternion.identity);
			g.SetActive(false);

			PlayerInfo.TurretInfo localInfo = new() {
				turret = g.GetComponent<Turret>(), //NOTE: actually a reference, not a prefab 
				turretName = t.turretName
			};
			spawnedTurrets.Add(localInfo);
		}
		//fallback to first turret if none actually given
		turret = spawnedTurrets[0].turret;
		turret.gameObject.SetActive(true);

		base.Start();
	}
	protected override void Update() {
		base.Update();
	}

	#endregion

}
