using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkedPlayer : NetworkBehaviour {
	public static NetworkedPlayer instance;

	#region References

	[SerializeField] private CombatEntity player;
	public CombatEntity GetPlayerEntity() { return player; }

	#endregion

	#region Synced

	[Networked, OnChangedRender(nameof(PositionChanged))]
	private Vector3 Position { get; set; }

	[Networked, OnChangedRender(nameof(TurretRotationChanged))]
	private Quaternion TurretRotation { get; set; }

	#endregion

	#region Members

	//for non-local clients
	private Vector3 targetPosition = Vector3.zero;
	private Quaternion targetTurretRotation = Quaternion.identity;

	#endregion

	#region Callbacks

	private void PositionChanged() {
		targetPosition = Position;
	}
	private void TurretRotationChanged() {
		player.GetTurret().SetTargetTurretRotation(TurretRotation.eulerAngles.y);
	}

	#endregion

	#region Functions

	private void Start() {
		targetPosition = transform.position;
		targetTurretRotation = player.GetTurret().transform.rotation;
	}
	private void Update() {
		if (!HasStateAuthority) {
			if (Vector3.Distance(transform.position, targetPosition) < 5f) {
				transform.position = Vector3.MoveTowards(
					transform.position, targetPosition, player.GetHull().GetSpeed() * Time.deltaTime * 1.2f);
			} else {
				transform.position = targetPosition;
			}
		}
	}

	public override void Spawned() {
		if (HasStateAuthority) {
			instance = this;
			EntityController.player = player;
			transform.position = new Vector3(transform.position.x, 0, transform.position.z);
		}
	}
	public override void FixedUpdateNetwork() {
		if (HasStateAuthority) {
			//update position
			Position = transform.position;
			TurretRotation = player.GetTurret().transform.rotation;
		}
	}

	#endregion
}
