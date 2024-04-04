using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkedEntity : NetworkBehaviour {
	public static NetworkedEntity playerInstance;

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
	public int GetTeam() { return Team; }

	#endregion

	#region Members

	[SerializeField] private bool isPlayer;

	//for non-local clients
	private Vector3 targetPosition = Vector3.zero;
	private Quaternion targetTurretRotation = Quaternion.identity;

	#endregion

	#region Callbacks

	private void PositionChanged() {
		targetPosition = Position;
	}
	//NOTE: only applies to CombatEntities
	private void TurretRotationChanged() {
		if (optionalCombatEntity == null) return;
		optionalCombatEntity.GetTurret().SetTargetTurretRotation(TurretRotation.eulerAngles.y);
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

	#endregion

	#region Functions

	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	public void RPC_FireWeapon(int bulletId) {
		if (HasStateAuthority || optionalCombatEntity == null) return;
		optionalCombatEntity.GetTurret().NonLocalFireWeapon(
			optionalCombatEntity, mainEntity.GetTeam(), bulletId);
	}
	//tries to delete the bullet on non-local clients (nothing happens if already destroyed)
	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	public void RPC_DestroyBullet(int bulletId) {
		if (HasStateAuthority || optionalCombatEntity == null) return;
		optionalCombatEntity.RemoveBullet(bulletId);
	}
	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	public void RPC_TakeDamage(float damage) {
		Health = Mathf.Max(0f, Health - damage);
		mainEntity.LostHealth();
	}
	public void DeleteEntity() {
		Runner.Despawn(Object);
	}
	public override void Despawned(NetworkRunner runner, bool hasState) {
		mainEntity.RemoveEntityFromRegistry();
	}
	public override void Spawned() {
		if (HasStateAuthority) {
			if (isPlayer) {
				playerInstance = this;
				EntityController.player = optionalCombatEntity;

				//TODO: add team selection
				Team = (new List<PlayerRef>(Runner.ActivePlayers).Count + 1) % 2;
			}
			transform.position = new Vector3(transform.position.x, 0, transform.position.z);
		}
		TeamChanged();
	}
	public override void FixedUpdateNetwork() {
		if (HasStateAuthority && isPlayer) {
			//update position
			Position = transform.position;
			TurretRotation = optionalCombatEntity.GetTurret().transform.rotation;
		}
	}
	private void Awake() {
		if (isPlayer) {
			optionalCombatEntity = (CombatEntity)mainEntity;
		}
	}
	private void Start() {
		if (optionalCombatEntity == null) return;

		targetPosition = transform.position;
		targetTurretRotation = optionalCombatEntity.GetTurret().transform.rotation;
	}
	private void Update() {
		if (!HasStateAuthority) {
			//non-local player
			if (Vector3.Distance(transform.position, targetPosition) < 5f) {
				transform.position = Vector3.MoveTowards(
					transform.position, targetPosition,
					optionalCombatEntity.GetHull().GetSpeed() * Time.deltaTime * 1.2f
				);
			} else {
				transform.position = targetPosition;
			}
		} else {
			//local player
			if (isPlayer && Health < mainEntity.GetMaxHealth() &&
				Time.time - mainEntity.GetLastDamageTimestamp() > 3.5f) {
				Health = Mathf.Min(mainEntity.GetMaxHealth(),
					Health + Time.deltaTime * mainEntity.GetMaxHealth() / 12f);
				mainEntity.UpdateHealthBar();
			}
		}
	}

	#endregion
}
