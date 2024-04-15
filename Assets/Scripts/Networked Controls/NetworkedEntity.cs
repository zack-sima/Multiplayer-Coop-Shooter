using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkedEntity : NetworkBehaviour {

	#region Const & Statics

	public static NetworkedEntity playerInstance;

	private const float RESPAWN_PROTECTION_TIME = 3f;

	#endregion

	#region References

	[SerializeField] private Entity mainEntity;
	public Entity GetEntity() { return mainEntity; }

	//if the entity is a combat entity, this refers to the same class
	private CombatEntity optionalCombatEntity = null;
	public CombatEntity GetCombatEntity() { return optionalCombatEntity; }

	#endregion

	#region Synced

	[Networked, OnChangedRender(nameof(PositionChanged))]
	private Vector3 Position { get; set; }

	[Networked, OnChangedRender(nameof(TurretRotationChanged))]
	private Quaternion TurretRotation { get; set; }

	[Networked, OnChangedRender(nameof(HealthBarChanged))]
	private float Health { get; set; } = 999;
	public float GetHealth() { return Health; }

	//Team: determines whether bullets pass by, health bar color, etc;
	[Networked, OnChangedRender(nameof(TeamChanged))]
	private int Team { get; set; } = -1;
	public int GetTeam() { try { return Team; } catch { return -1; } }

	[Networked, OnChangedRender(nameof(IsDeadChanged))]
	private bool IsDead { get; set; } = false;
	public bool GetIsDead() { try { return IsDead; } catch { return true; } }
	public bool GetIsDeadUncaught() { return IsDead; } //for game over don't return true when error

	[Networked, OnChangedRender(nameof(TurretChanged))]
	private string TurretName { get; set; } = "Autocannon";
	public void SetTurretName(string turretName) { TurretName = turretName; } //only by local

	#endregion

	#region Members

	[SerializeField] private bool isPlayer;
	public bool GetIsPlayer() { return isPlayer; }

	//for non-local clients
	private Vector3 targetPosition = Vector3.zero;
	private Quaternion targetTurretRotation = Quaternion.identity;

	//don't update unless initted in spawned
	private bool initialized = false;
	public bool GetInitialized() { return initialized; }

	private Rigidbody optionalRigidbody = null;

	//local ability toggles; TODO: scale ability level, etc
	private bool abilityHealOn = false;
	private bool abilityOverclockOn = false;

	//respawn protection (locally enforced); compares Time.time
	private float lastRespawnTimestamp = -10f;

	#endregion

	#region Callbacks

	#region Ability Effects

	//Note: these should be called on the local player only, and the coroutines called will assume this.

	public void AbilityHealCalled() {
		StartCoroutine(AbilityHealCoroutine());
	}
	public void AbilityOverclockCalled() {
		StartCoroutine(AbilityOverclockCoroutine());
	}
	private IEnumerator AbilityHealCoroutine() {
		abilityHealOn = true;
		yield return new WaitForSeconds(2f);
		abilityHealOn = false;
	}
	private IEnumerator AbilityOverclockCoroutine() {
		abilityOverclockOn = true;
		yield return new WaitForSeconds(2f);
		abilityOverclockOn = false;
	}

	#endregion

	private void PositionChanged() {
		targetPosition = Position;
	}
	private void TurretChanged() {
		if (optionalCombatEntity == null || !isPlayer) return;

		optionalCombatEntity.TurretChanged(TurretName);

		if (HasSyncAuthority() && isPlayer) {
			PlayerInfo.instance.TurretChanged(TurretName);
		}
	}
	//NOTE: only applies to CombatEntities
	private void TurretRotationChanged() {
		if (optionalCombatEntity == null) return;

		//<20˚ will be interpolated, otherwise snap
		if (Quaternion.Angle(TurretRotation, optionalCombatEntity.GetTurret().transform.rotation) > 20f) {
			optionalCombatEntity.GetTurret().SnapToTargetRotation(TurretRotation.eulerAngles.y);
		} else {
			optionalCombatEntity.GetTurret().SetTargetTurretRotation(TurretRotation.eulerAngles.y);
		}
	}
	private void HealthBarChanged() {
		mainEntity.UpdateHealthBar();
	}
	private void TeamChanged() {
		if (Team != -1) {
			mainEntity.SetTeam(Team);
			mainEntity.UpdateHealthBar();
		}
	}
	private void IsDeadChanged() {
		if (HasSyncAuthority()) return;

		if (!IsDead) mainEntity.RespawnEntity();
		else mainEntity.SetEntityToDead();
	}

	#endregion

	#region Functions

	//must be called by local player!
	public void EntityDied() {
		IsDead = true;
	}
	public void EntityRespawned() {
		IsDead = false;
		lastRespawnTimestamp = Time.time;
	}
	//either is player and has state authority or isn't player and is master client/SP
	public bool HasSyncAuthority() {
		try {
			return HasStateAuthority && isPlayer || !isPlayer && (Runner.IsSharedModeMasterClient || Runner.IsSinglePlayer);
		} catch { return false; }
	}
	[Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
	public void RPC_FireWeapon(int bulletId) {
		if (HasSyncAuthority() || optionalCombatEntity == null ||
			!optionalCombatEntity.GetTurret().gameObject.activeInHierarchy) return;
		optionalCombatEntity.GetTurret().NonLocalFireWeapon(
			optionalCombatEntity, mainEntity.GetTeam(), bulletId);
	}
	//tries to delete the bullet on non-local clients (nothing happens if already destroyed)
	[Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
	public void RPC_DestroyBullet(int bulletId) {
		if (HasSyncAuthority() || optionalCombatEntity == null) return;
		optionalCombatEntity.RemoveBullet(bulletId);
	}
	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	public void RPC_TakeDamage(NetworkObject target, float damage) {
		//NOTE: RPC is called on all objects the player owns (bots, players, etc if master client)
		// target needs to be checked to make sure THIS object is the actual one being targeted
		if (target != Object) return;

		//within respawn protection
		if (Time.time - lastRespawnTimestamp < RESPAWN_PROTECTION_TIME) return;

		Health = Mathf.Max(0f, Health - damage);
		mainEntity.LostHealth();
	}
	public void DeleteEntity() {
		Runner.Despawn(Object);
	}
	public override void Despawned(NetworkRunner runner, bool hasState) {
		mainEntity.EntityRemoved();
		mainEntity.RemoveEntityFromRegistry();
	}
	public override void Spawned() {
		if (HasSyncAuthority()) {
			if (isPlayer) {
				playerInstance = this;
				EntityController.player = optionalCombatEntity;

				TurretName = PlayerInfo.instance.GetLocalPlayerTurretName();

				GetComponent<AudioListener>().enabled = true;

				transform.position = new Vector3(0, 0, 0);

				//TODO: add team selection for pvp
				Team = 0;
			} else {
				Team = 1;
			}
			Health = optionalCombatEntity.GetMaxHealth();
			transform.position = new Vector3(transform.position.x, 0, transform.position.z);
		} else {
			PositionChanged();
			TurretRotationChanged();
			SyncNonLocal();
		}
		TeamChanged();
		StartCoroutine(StartupDelay());
		StartCoroutine(WaitInitialize());
	}
	//sometimes there's a bug where even if initialization is done networking doesn't allow calls
	private IEnumerator WaitInitialize() {
		yield return new WaitForEndOfFrame();
		initialized = true;
		TurretChanged();
	}
	//wait a few frames for networking to fully sync
	private IEnumerator StartupDelay() {
		yield return new WaitForSeconds(0.1f);
		if (!IsDead) mainEntity.RespawnEntity();
		TurretChanged();
	}
	public override void FixedUpdateNetwork() {
		if (!initialized) return;

		if (HasSyncAuthority()) {
			//update position
			Position = transform.position;
			TurretRotation = optionalCombatEntity.GetTurret().transform.rotation;
		}
	}
	private void SyncNonLocal() {
		if (Vector3.Distance(transform.position, targetPosition) < 3f) {
			if (optionalRigidbody != null) optionalRigidbody.velocity = Vector3.zero;
			transform.position = Vector3.MoveTowards(
				transform.position, targetPosition,
				optionalCombatEntity.GetHull().GetSpeed() * Time.deltaTime * 1.2f
			);
		} else {
			transform.position = targetPosition;
		}
	}
	private void Awake() {
		if (mainEntity is CombatEntity entity) {
			optionalCombatEntity = entity;
		}
		TryGetComponent(out optionalRigidbody);
	}
	private void Update() {
		if (!initialized) return;

		if (HasSyncAuthority()) {
			//local entity
			if (isPlayer) {
				//overclock ability
				if (abilityOverclockOn) {
					PlayerInfo.instance.ReloadFaster();
					optionalCombatEntity.GetTurret().ReloadFaster();
				}
				//healing ability, TODO: scale by ability stats instead
				if (abilityHealOn) {
					Health = Mathf.Min(mainEntity.GetMaxHealth(),
						Health + Time.deltaTime * mainEntity.GetMaxHealth() / 5f);
					mainEntity.UpdateHealthBar();
				}
				//natural healing
				if (Time.time - mainEntity.GetLastDamageTimestamp() > 2.5f) {
					Health = Mathf.Min(mainEntity.GetMaxHealth(),
						Health + Time.deltaTime * mainEntity.GetMaxHealth() / 12f);
					mainEntity.UpdateHealthBar();
				}
			}
		} else {
			//non-local entity
			SyncNonLocal();
		}
	}

	#endregion
}
