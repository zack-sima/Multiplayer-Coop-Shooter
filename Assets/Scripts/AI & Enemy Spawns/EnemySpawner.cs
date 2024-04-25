using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

/// <summary>
/// This class handles spawning when the local player is the master client. Otherwise, it does nothing.
/// </summary>

public class EnemySpawner : NetworkBehaviour {

	#region Statics

	public static EnemySpawner instance;

	#endregion

	#region Prefabs

	[SerializeField] private List<GameObject> enemyPrefabs;
	[SerializeField] private List<int> enemyCosts; //credit costs

	#endregion

	#region Synced

	[Networked] //for master client (if host migrated, this makes sure spawning is rougly the same)
	private float SpawnTimer { get; set; } = 0;
	[Networked]
	private int SpawnIndex { get; set; } = 0;

	#endregion

	#region Members

	//a hook to FixedUpdateNetwork
	private bool spawnEnemyLater = false;
	private GameObject spawnEnemyLaterPrefab = null;

	#endregion

	#region Spawn Logic

	//NOTE: this returns a shuffled list (seeded by wave) of enemy prefab indices
	private List<int> DetermineEnemiesForInfiniteWave(int waveNum) {
		//$10 is the standard cost of a normal enemy
		int totalSpawnCredits = 10 * ((int)Mathf.Pow(waveNum, 1.5f) + 5);

		List<int> enemySpawns = new();
		System.Random rand = new(GameStatsSyncer.instance.GetRandomSeed() + waveNum);

		int minCost = enemyCosts.Min();
		while (totalSpawnCredits >= minCost) {
			int spawnChoice = rand.Next() % enemyCosts.Count;

			//find another enemy (don't allow expensive enemies if they take up >25% of total cost)
			if (enemyCosts[spawnChoice] > totalSpawnCredits || enemyCosts[spawnChoice] > minCost &&
				enemyCosts[spawnChoice] * 4f > totalSpawnCredits) continue;

			totalSpawnCredits -= enemyCosts[spawnChoice];
			enemySpawns.Add(spawnChoice);
		}
		return enemySpawns;
	}
	private IEnumerator SpawnCycle() {
		yield return new WaitForSeconds(5f);

		//NOTE: infinite mode
		while (true) {
			while (EntityController.player == null || GameStatsSyncer.instance == null ||
				GameStatsSyncer.instance.GetGameOver())
				yield return null;

			//NOTE: Only proceed 
			if (NetworkedEntity.playerInstance == null || !NetworkedEntity.playerInstance.
				Runner.IsSharedModeMasterClient && !NetworkedEntity.playerInstance.Runner.IsSinglePlayer) {
				yield return new WaitForSeconds(1f);
				continue;
			}

			while (SpawnTimer > 0) {
				SpawnTimer -= Time.deltaTime;

				if (SpawnTimer <= 0) GameStatsSyncer.instance.IncrementWave();

				//if everything was killed only wait 10s max
				if (EnemiesAreDead() && SpawnTimer > 10f) SpawnTimer = 10f;

				yield return new WaitForEndOfFrame();
			}

			int currWave = GameStatsSyncer.instance.GetWave();
			Debug.Log($"Spawning wave {currWave + 1}");

			//spawning logic
			List<int> enemies = DetermineEnemiesForInfiniteWave(currWave);

			//NOTE: SpawnIndex usually is 0, but if host was migrated this would be a nonzero number
			for (int i = SpawnIndex; i < enemies.Count; i++) {
				if (GameStatsSyncer.instance.GetGameOver()) break;

				spawnEnemyLater = true;

				spawnEnemyLaterPrefab = enemyPrefabs[enemies[i]];

				SpawnIndex++;
				yield return new WaitForSeconds(8f / (currWave + 5f));
			}

			//end of wave delay
			SpawnIndex = 0;
			SpawnTimer = currWave * 2f + 20f;
		}
	}
	//TODO: should only be for the final wave in non-infinite mode
	private IEnumerator WaitUntilEnemiesDead() {
		//waits until all enemies are dead
		bool enemiesAlive;
		bool waited = false;
		do {
			yield return null;
			enemiesAlive = false;
			try {
				foreach (CombatEntity e in EntityController.instance.GetCombatEntities()) {
					if (e == null || e.GetIsPlayer()) continue;
					enemiesAlive = true;
					waited = true;
					break;
				}
			} catch { }
		} while (enemiesAlive);

		//TODO: change this code to adapt to level conditions
		//if waited, means enemies were actually spawned
		if (waited) {
			//give players some time after killing all enemies
			yield return new WaitForSeconds(10f);
			GameStatsSyncer.instance.IncrementWave();
		}
	}
	private bool EnemiesAreDead() {
		try {
			foreach (CombatEntity e in EntityController.instance.GetCombatEntities()) {
				if (e == null || e.GetIsPlayer()) continue;
				return true;
			}
			return false;
		} catch {
			return false;
		}
	}
	private void SpawnEnemy(GameObject spawnPrefab) {
		try {
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
			} while (iterations < 20 && spawnpointDistance < 18f);

			Runner.Spawn(spawnPrefab, new Vector3(point.x, 1, point.z), Quaternion.identity);
		} catch (System.Exception e) { Debug.LogWarning(e); }
	}
	
	public override void FixedUpdateNetwork() {
		if (spawnEnemyLater) {
			spawnEnemyLater = false;
			if (spawnEnemyLaterPrefab != null) {
				SpawnEnemy(spawnEnemyLaterPrefab);
			}
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
