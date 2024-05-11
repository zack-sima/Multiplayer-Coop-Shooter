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

	//basic coop enemies
	[SerializeField] private List<GameObject> enemyPrefabs;
	[SerializeField] private List<int> enemyCosts; //credit costs

	//for every 5th wave
	[SerializeField] private List<GameObject> enemyBossPrefabs;
	[SerializeField] private List<int> enemyBossCosts;

	//PVP bots
	[SerializeField] private List<GameObject> PVPBotPrefabs;

	#endregion

	#region Synced

	//for master client (if host migrated, this makes sure spawning is rougly the same)
	[Networked]
	private float SpawnTimer { get; set; } = 0;
	public float GetSpawnTimer() { return SpawnTimer; }

	//for before the game starts, how long players get to prepare
	[Networked]
	private float InitTimer { get; set; } = 0;
	public float GetInitTimer() { return InitTimer; }

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
	private List<GameObject> DetermineEnemiesForInfiniteWave(int waveNum) {
		//$10 is the standard cost of a normal enemy
		int totalSpawnCredits = 10 * ((int)Mathf.Pow(waveNum, 1.5f) + 5);

		//bosses
		bool isBossWave = (waveNum + 1) % 5 == 0 && waveNum >= 9;
		List<int> currEnemyCosts = isBossWave ? enemyBossCosts : enemyCosts;
		List<GameObject> currEnemyPrefabs = isBossWave ? enemyBossPrefabs : enemyPrefabs;

		List<GameObject> enemySpawns = new();
		System.Random rand = new(GameStatsSyncer.instance.GetRandomSeed() + waveNum);

		int minCost = currEnemyCosts.Min();
		int spawned = 0;
		bool onBoss = isBossWave;
		while (totalSpawnCredits >= minCost) {
			int spawnChoice = rand.Next() % currEnemyCosts.Count;

			//find another enemy (don't allow expensive enemies if they take up >25% of total cost)
			if (currEnemyCosts[spawnChoice] > totalSpawnCredits || !isBossWave &&
				currEnemyCosts[spawnChoice] > minCost && currEnemyCosts[spawnChoice] * 4f > totalSpawnCredits) continue;

			totalSpawnCredits -= currEnemyCosts[spawnChoice];
			enemySpawns.Add(currEnemyPrefabs[spawnChoice]);

			spawned++;

			//spawn non-bosses after spawning n number of bosses
			if (onBoss && spawned >= (int)(Mathf.Pow(waveNum, 1.5f) / 35f)) {
				Debug.LogWarning($"switching; currency = {totalSpawnCredits}");
				currEnemyCosts = enemyCosts;
				currEnemyPrefabs = enemyPrefabs;
				minCost = currEnemyCosts.Min();
				onBoss = false;
			}
		}
		return enemySpawns;
	}
	private IEnumerator SpawnCycle() {

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
			while (InitTimer >= 0) {
				yield return new WaitForEndOfFrame();
				InitTimer -= Time.deltaTime;
			}
			if (SpawnIndex == 0) {
				//if spawn timer goes down also comes back
				yield return StartCoroutine(WaitUntilEnemiesDead());

				//upgrades if there is a timer
				if (SpawnTimer > 0) {

					//force it back to 10
					if (SpawnTimer > 10) SpawnTimer = 10;

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
			List<GameObject> enemies = DetermineEnemiesForInfiniteWave(currWave);

			//NOTE: SpawnIndex usually is 0, but if host was migrated this would be a nonzero number
			for (int i = SpawnIndex; i < enemies.Count; i++) {
				if (GameStatsSyncer.instance.GetGameOver()) break;

				spawnEnemyLater = true;

				spawnEnemyLaterPrefab = enemies[i];

				SpawnIndex++;
				yield return new WaitForSeconds(8f / (currWave + 5f));
			}

			//end of wave delay
			SpawnIndex = 0;
			SpawnTimer = 10 + currWave / 2f;
			if ((currWave + 1) % 5 == 0) SpawnTimer += 10;
		}
	}
	private IEnumerator WaitUntilEnemiesDead() {
		//waits until all enemies are dead
		bool enemiesAlive;
		do {
			yield return new WaitForEndOfFrame();

			SpawnTimer -= Time.deltaTime;

			enemiesAlive = false;
			try {
				enemiesAlive = !EnemiesAreDead();
			} catch { }
		} while (enemiesAlive && SpawnTimer > 10f);
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
	public bool HasSyncAuthority() {
		return Runner.IsSharedModeMasterClient || Runner.IsSinglePlayer;
	}

	private void SpawnPVPBot(int team) {
		NetworkedEntity spawned = Runner.Spawn(PVPBotPrefabs[Random.Range(0, PVPBotPrefabs.Count)],
			MapController.instance.GetTeamSpawnpoint(team) +
			new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f)),
			Quaternion.identity).GetComponent<NetworkedEntity>();

		spawned.SetNonPlayerTeam(team);
	}
	private IEnumerator SpawnPVPBots(int blueBots, int redBots) {
		yield return new WaitForSeconds(0.1f);

		for (int i = 0; i < blueBots; i++) {
			SpawnPVPBot(0);
			yield return new WaitForSeconds(0.1f);
		}
		for (int i = 0; i < redBots; i++) {
			SpawnPVPBot(1);
			yield return new WaitForSeconds(0.1f);
		}
	}

	#endregion

	#region Initialization

	public override void Spawned() {
		if (!HasSyncAuthority()) return;

		InitTimer = PlayerPrefs.GetInt("game_start_delay");
	}
	private void Awake() {
		instance = this;
	}
	private void Start() {
		if (PlayerInfo.GetIsPVP()) {
			StartCoroutine(SpawnPVPBots(2, 2));
			return;
		}

		StartCoroutine(SpawnCycle());
	}

	#endregion

}
