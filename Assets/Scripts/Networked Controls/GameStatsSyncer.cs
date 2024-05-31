using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// This script is the primary way to sync the game's server data, like team score, current wave, etc.
/// It is controlled by whoever happens to be the master client (where data is synced between all clients
/// through the [Networked] tag.
/// </summary>

public class GameStatsSyncer : NetworkBehaviour {

	#region Statics & Consts

	public static GameStatsSyncer instance;

	#endregion

	#region Synced

	[Networked] //random seed for enemy spawning, deterministic by map
	private int RandomSeed { get; set; } = 0;
	public int GetRandomSeed() { return RandomSeed; }

	[Networked, OnChangedRender(nameof(ScoreChanged))]
	private int Score { get; set; } = 0;
	public void AddScore(int addition) { Score += addition; } //called by master client only

	[Networked, OnChangedRender(nameof(WaveChanged))]
	private int Wave { get; set; } = 0;
	public int GetWave() { return Wave; } //all spawners sync this and simulate same delays
	public void IncrementWave() { Wave++; } //called by master client only

	[Networked, Capacity(2), OnChangedRender(nameof(TeamScoresChanged))]
	NetworkArray<int> TeamScores { get; }
	public void AddTeamScore(int team, int score) { TeamScores.Set(team, TeamScores.Get(team) + score); }

	[Networked, Capacity(8)] //point capture; arbitrary number for points needed
	NetworkArray<int> CapturePointOwners { get; }

	[Networked, Capacity(8)]
	NetworkArray<float> CapturePointValues { get; }

	//NOTE: point capture scores
	[Networked, Capacity(2), OnChangedRender(nameof(TeamPointCapScoresChanged))]
	NetworkArray<int> CapturePointScores { get; } = MakeInitializer(new int[] { 100, 100 });

	[Networked] //comp only
	private int WinningTeam { get; set; } = -1;
	public int GetWinningTeam() { return WinningTeam; }

	[Networked, OnChangedRender(nameof(GameOverChanged))]
	private bool GameOver { get; set; } = false;
	public bool GetGameOver() { return GameOver; }

	//NOTE: updated every second
	[Networked, OnChangedRender(nameof(TimeChanged))]
	private float ServerTime { get; set; } = 0;
	public float GetServerTime() { return ServerTime; }

	//NOTE: local time; call this to sync map objects
	private float localTime = 0;
	public float GetLocalTime() { return localTime; }

	//local game over called
	private bool gameOverInvoked = false;

	#endregion

	#region Callbacks

	private void TeamScoresChanged() {
		if (PlayerInfo.GetIsPointCap()) {
			UpgradesCatalog.instance.ScoreChanged(TeamScores.Get(NetworkedEntity.playerInstance.GetTeam()));
		} else if (PlayerInfo.GetIsPVP()) {
			UIController.instance.SetTeamScores(TeamScores.ToArray());
		}
	}
	private void TeamPointCapScoresChanged() {
		UIController.instance.SetPointCapScores(CapturePointScores.ToArray());
	}
	private void ScoreChanged() {
		if (PlayerInfo.GetIsPointCap()) return;
		UpgradesCatalog.instance.ScoreChanged(Score);
	}
	//everyone displays the same score
	private void WaveChanged() {
		UIController.instance.SetWaveText(Wave + 1);

		if (PlayerInfo.GetIsPVP() || GameOver) return;

		int currentRecord = PersistentDict.GetInt("wave_record_" + PlayerPrefs.GetString("game_map_name") +
			"_" + PlayerPrefs.GetInt("game_mode"));

		if (Wave + 1 > currentRecord) {
			PersistentDict.SetInt("wave_record_" + PlayerPrefs.GetString("game_map_name") +
				"_" + PlayerPrefs.GetInt("game_mode"), Wave + 1);
		}


	}
	private void GameOverChanged() {
		if (GameOver) {
			StartCoroutine(GameOverCoroutine());
		}
	}
	private void TimeChanged() {
		if (HasSyncAuthority()) return;

		if ((int)localTime != (int)ServerTime) {
			localTime = ServerTime;
		}
	}

	#endregion

	#region Prefabs

	[SerializeField] private GameObject damageTextPrefab;

	#endregion

	#region Members

	private Dictionary<string, DamageText> spawnedDamageTexts = new();

	#endregion

	#region Functions

	public bool HasSyncAuthority() {
		return Runner.IsSharedModeMasterClient || Runner.IsSinglePlayer;
	}
	public override void FixedUpdateNetwork() {
		//NOTE: PVP should have separate handler for score and win/loss, etc
		if (!HasSyncAuthority() || EntityController.player == null) return;

		Debug.LogWarning("gameupdate");

		//PvP follows other rulesets; TODO: expand for more than TDM
		if (PlayerInfo.GetIsPVP()) {
			if (PlayerInfo.GetIsPointCap()) {
				if (!GameOver) {
					if (CapturePointScores[0] <= 0) {
						GameOver = true;
						WinningTeam = 1;
					} else if (CapturePointScores[1] <= 0) {
						GameOver = true;
						WinningTeam = 0;
					}
				}
			} else {
				if (!GameOver) {
					if (TeamScores[0] >= 40) {
						GameOver = true;
						WinningTeam = 0;
					} else if (TeamScores[1] >= 40) {
						GameOver = true;
						WinningTeam = 1;
					}
				}
			}
			return;
		}

		//check if all players are alive; if not, end game
		bool someoneAlive = false;
		bool someoneDead = false;
		foreach (CombatEntity e in EntityController.instance.GetCombatEntities()) {
			if (e == null || !e.GetIsPlayer() || !e.GetNetworker().GetInitialized()) continue;
			try {
				if (!e.GetNetworker().GetIsDeadUncaught()) {
					someoneAlive = true;
					break;
				} else {
					someoneDead = true;
				}
			} catch (System.Exception ex) { Debug.LogWarning(ex); }
		}
		//it's joever (make sure players were actually initialized and at least one is dead)
		if (!someoneAlive && someoneDead) {
			GameOver = true;
		}
	}
	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	public void RPC_TakeDamageEffect(Vector3 position, NetworkObject target, float damage, string senderTargetId) {
		SpawnTakeDamageText(position, target, Mathf.CeilToInt(damage), senderTargetId);
	}
	//the damage number text that pops up when an entity is hit
	public void SpawnTakeDamageText(Vector3 position, NetworkObject target, int damage, string senderTargetId) {
		if (!PlayerInfo.GetIsPVP() && target.gameObject == NetworkedEntity.playerInstance.gameObject) return;

		try {
			bool noRand = false;
			Vector3 randOffset = new Vector3(Random.Range(-0.5f, 0.5f),
				Random.Range(3.5f, 4.5f), Random.Range(-0.5f, 0.5f));

			if (spawnedDamageTexts.ContainsKey(senderTargetId)) {
				if (spawnedDamageTexts[senderTargetId] != null && spawnedDamageTexts[senderTargetId].CanStack()) {
					damage += spawnedDamageTexts[senderTargetId].GetDamage();
					randOffset = spawnedDamageTexts[senderTargetId].GetRandOffset();
					noRand = true;
					Destroy(spawnedDamageTexts[senderTargetId].gameObject);
				}
			}

			GameObject item = Instantiate(damageTextPrefab, position + randOffset,
				Camera.main.transform.rotation);

			if (!spawnedDamageTexts.TryAdd(senderTargetId, item.GetComponent<DamageText>())) {
				spawnedDamageTexts[senderTargetId] = item.GetComponent<DamageText>();
			}

			float damageScale = Mathf.Clamp(damage / 750f + 0.5f, 1f, 2f);

			item.GetComponent<DamageText>().SetDamageText(damage, target.gameObject == NetworkedEntity.playerInstance.gameObject ?
				new Color(1f, 0.3f, 0.3f) : Color.white, damageScale, noRand, randOffset);

		} catch (System.Exception e) {
			Debug.LogWarning(e);
		}
	}
	//called when game over is detected as true
	private IEnumerator GameOverCoroutine() {

		Debug
			.Log("Game over!");
		UIController.instance.SetRespawnUIEnabled(false);
		UIController.instance.SetGameOverUIEnabled(true);

		gameOverInvoked = true;

		for (float i = 10f; i > 0f; i -= Time.deltaTime) {
			//UIController screen
			if (PlayerInfo.GetIsPVP()) {
				string text = WinningTeam == NetworkedEntity.playerInstance.GetTeam() ?
					"Victory!" : "Defeat.";
				UIController.instance.SetGameOverTimerText($"{text} Leaving in:\n{Mathf.CeilToInt(i)}");
			} else {
				UIController.instance.SetGameOverTimerText($"Game over. Leaving in:\n{Mathf.CeilToInt(i)}");
			}
			yield return null;
		}

		//leave game!
		ServerLinker.instance.StopGame();
	}
	public override void Spawned() {
		GameOverChanged();
		WaveChanged();
		TeamScoresChanged();
		TeamPointCapScoresChanged();

		UpgradesCatalog.instance.MoneyChanged();

		if (HasSyncAuthority()) {
			Wave = Mathf.Max(PlayerPrefs.GetInt("game_start_wave") - 1, 0);
			RandomSeed = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
		}
	}
	private void Update() {
		//every new second time is synced
		if (HasSyncAuthority() && (int)Time.time > (int)(Time.time - Time.deltaTime)) {
			ServerTime = localTime;
		}
		localTime += Time.deltaTime;

		//point capture
		if (PlayerInfo.GetIsPointCap()) {
			if (HasSyncAuthority()) {
				int i = 0;

				//negative means blue loses points, positive means red loses points
				int pointBalance = 0;

				bool isFullSecond = ((int)Time.time) - (int)(Time.time - Time.deltaTime) != 0;

				foreach (CapturePoint p in MapController.instance.GetCapturePoints()) {
					if (!p.gameObject.activeInHierarchy) continue;

					p.UpdateCaptureProgress(EntityController.instance.GetCombatEntities());

					if (p.GetCaptureMode() == CapturePoint.Mode.Points && p.GetPointOwnerTeam() >= 0) {
						//reduce other team's points
						if (isFullSecond) {
							pointBalance += p.GetPointOwnerTeam() == 0 ? p.GetPointsPerSecond() : -p.GetPointsPerSecond();
						}
					}

					int val = p.GetPoints();
					if (val > 0) {
						if (p.GetCaptureMode() == CapturePoint.Mode.Money) {
							//give team money through score
							TeamScores.Set(p.GetPointOwnerTeam(), TeamScores.Get(p.GetPointOwnerTeam()) + val);
						}
					}
					CapturePointValues.Set(i, p.GetCaptureProgress());
					CapturePointOwners.Set(i, p.GetPointOwnerTeam());
					i++;
				}
				if (pointBalance != 0) {
					int index = pointBalance < 0 ? 0 : 1;
					CapturePointScores.Set(index, Mathf.Max(CapturePointScores.Get(index) - Mathf.Abs(pointBalance), 0));
				}
			} else {
				int i = 0;
				foreach (CapturePoint p in MapController.instance.GetCapturePoints()) {
					if (!p.gameObject.activeInHierarchy) continue;

					p.SetClientCaptureProgress(CapturePointValues.Get(i), CapturePointOwners.Get(i));
					i++;
				}
			}
		}
	}
	private void Awake() {
		if (instance != null) {
			Debug.LogError("more than one instance!");
		}
		instance = this;
	}

	#endregion
}
