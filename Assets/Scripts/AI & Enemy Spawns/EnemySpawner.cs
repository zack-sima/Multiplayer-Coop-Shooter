using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// This class handles spawning when the local player is the master client. Otherwise, it does nothing.
/// </summary>

public class EnemySpawner : NetworkBehaviour {

	#region Statics

	public static EnemySpawner instance;

	#endregion

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
			while (EntityController.player == null || GameStatsSyncer.instance == null)
				yield return null;

			//NOTE: Only proceed 
			if (NetworkedEntity.playerInstance == null || !NetworkedEntity.playerInstance.
				Runner.IsSharedModeMasterClient && !NetworkedEntity.playerInstance.Runner.IsSinglePlayer) {
				yield return new WaitForSeconds(1f);
				continue;
			}

			//waits until all enemies are dead
			bool enemiesAlive;
			do {
				yield return new WaitForSeconds(0.5f);
				enemiesAlive = false;
				foreach (CombatEntity e in EntityController.instance.GetCombatEntities()) {
					if (e == null || e.GetIsPlayer()) continue;
					enemiesAlive = true;
					break;
				}
			} while (enemiesAlive);

			//give players some time after killing all enemies
			yield return new WaitForSeconds(10f);

			int currWave = GameStatsSyncer.instance.GetWave();
			Debug.Log($"Spawning wave {currWave + 1}");

			//spawning logic
			int spawnCount = (int)Mathf.Pow(currWave, 1.5f) + 5;

			for (int i = 0; i < spawnCount; i++) {
				spawnEnemyLater = true;
				yield return new WaitForSeconds(2f / (currWave + 5f) * 5f);
			}
			GameStatsSyncer.instance.IncrementWave();
		}
	}
	private void SpawnEnemy() {
		//find a point far away enough from players
		int iterations = 0; //prevent infinite loop

		//keep finding a random spawnpoint until it is far away enough from everyone
		float spawnpointDistance;
		Vector3 point;
		do {
			spawnpointDistance = 999;
			int rand = Random.Range(0, MapController.instance.GetSpawnpointParent().childCount);
			point = MapController.instance.GetSpawnpointParent().GetChild(rand).position;
			foreach (CombatEntity e in EntityController.instance.GetCombatEntities()) {
				if (e == null || !e.GetIsPlayer()) continue;

				float distance = Vector3.Distance(e.transform.position, point);
				if (spawnpointDistance > distance)
					spawnpointDistance = distance;
			}
			iterations++;
		} while (iterations < 10 && spawnpointDistance < 15f);

		Runner.Spawn(enemyPrefab, new Vector3(point.x, 1, point.z), Quaternion.identity);
	}
	public override void FixedUpdateNetwork() {
		if (spawnEnemyLater) {
			spawnEnemyLater = false;
			SpawnEnemy();
		}
	}

	#endregion

	#region Initialization

	private void Awake() {
		instance = this;
	}
	private void Start() {
		StartCoroutine(SpawnCycle());
	}

	#endregion

}
