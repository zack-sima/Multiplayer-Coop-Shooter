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

	//for master client (if host migrated, this makes sure spawning is rougly the same)
	[Networked]
	private float SpawnTimer { get; set; } = 0;
	public float GetSpawnTimer() { return SpawnTimer; }

	[Networked]
	private bool WaveEnded { get; set; } = false;
	public bool GetWaveEnded() { return WaveEnded; }

	[Networked]
	private int SpawnIndex { get; set; } = 0;

	[Networked, OnChangedRender(nameof(WaveChanged))]
	private int WaveCallback { get; set; } = 0;

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

			if (NetworkedEntity.playerInstance == null || !NetworkedEntity.playerInstance.
				Runner.IsSharedModeMasterClient && !NetworkedEntity.playerInstance.Runner.IsSinglePlayer) {
				yield return new WaitForSeconds(1f);
				continue;
			}

			if (SpawnIndex == 0) {
				yield return StartCoroutine(WaitUntilEnemiesDead());

				//upgrades if there is a timer
				if (SpawnTimer > 0) {
					WaveEnded = true;
					WaveCallback++;
				}
			}
			while (SpawnTimer > 0) {
				SpawnTimer -= Time.deltaTime;

				if (SpawnTimer <= 0) GameStatsSyncer.instance.IncrementWave();

				yield return new WaitForEndOfFrame();
			}
			WaveEnded = false;

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
			SpawnTimer = currWave < 10 ? 10 : 15;
		}
	}
	private IEnumerator WaitUntilEnemiesDead() {
		//waits until all enemies are dead
		bool enemiesAlive;
		do {
			yield return null;
			enemiesAlive = false;
			try {
				enemiesAlive = !EnemiesAreDead();
			} catch { }
		} while (enemiesAlive);
	}
	private bool EnemiesAreDead() {
		try {
			foreach (CombatEntity e in EntityController.instance.GetCombatEntities()) {
				if (e == null || e.GetIsPlayer()) continue;
				return false;
			}
			return true;
		} catch {
			return false;
		}
	}

	//called so that all players get the shop scene
	private void WaveChanged() {
		//TODO: fix before using
		return;
		UpgradesCatalog.instance.ShowPossibleUpgrades();
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
