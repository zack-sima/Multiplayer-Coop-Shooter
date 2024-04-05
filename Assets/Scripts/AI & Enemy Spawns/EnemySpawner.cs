using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// This class handles spawning when the local player is the master client. Otherwise, it does nothing.
/// </summary>

public class EnemySpawner : SimulationBehaviour {

	#region Prefabs

	[SerializeField] private GameObject enemyPrefab;

	#endregion

	#region Members

	//a hook to FixedUpdateNetwork
	private bool spawnEnemyLater = false;

	#endregion

	#region Spawn Logic

	private IEnumerator SpawnCycle() {
		while (true) {
			while (EntityController.player == null) yield return null;
			yield return new WaitForSeconds(5);

			//NOTE: Only proceed 
			if (NetworkedEntity.playerInstance == null || !NetworkedEntity.playerInstance.
				Runner.IsSharedModeMasterClient && !NetworkedEntity.playerInstance.Runner.IsSinglePlayer) continue;

			spawnEnemyLater = true;
		}
	}
	private void SpawnEnemy() {
		Runner.Spawn(enemyPrefab, new Vector3(0, 1, 0), Quaternion.identity);
	}
	public override void FixedUpdateNetwork() {
		if (spawnEnemyLater) {
			spawnEnemyLater = false;
			SpawnEnemy();
		}
	}

	#endregion

	#region Initialization

	private void Start() {
		StartCoroutine(SpawnCycle());
	}

	#endregion

}
