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

	#region Statics

	public static GameStatsSyncer instance;

	#endregion

	#region Synced

	[Networked] //random seed for enemy spawning, determined once by first master client
	private int RandomSeed { get; set; } = 0;
	public int GetRandomSeed() { return RandomSeed; }

	[Networked, OnChangedRender(nameof(ScoreOrWaveChanged))]
	private int Score { get; set; } = 0;
	public void AddScore(int addition) { Score += addition; } //called by master client only

	[Networked, OnChangedRender(nameof(ScoreOrWaveChanged))]
	private int Wave { get; set; } = 0;
	public int GetWave() { return Wave; } //all spawners sync this and simulate same delays
	public void IncrementWave() { Wave++; } //called by master client only

	[Networked, OnChangedRender(nameof(GameOverChanged))]
	private bool GameOver { get; set; } = false;
	public bool GetGameOver() { return GameOver; }

	//local game over called
	private bool gameOverInvoked = false;

	#endregion

	#region Callbacks

	//everyone displays the same score
	private void ScoreOrWaveChanged() {
		UIController.instance.SetScoreAndWaveText(Score, Wave + 1);
	}
	private void GameOverChanged() {
		if (GameOver) {
			StartCoroutine(GameOverCoroutine());
		}
	}

	#endregion

	#region Functions

	public bool HasSyncAuthority() {
		return Runner.IsSharedModeMasterClient || Runner.IsSinglePlayer;
	}
	public override void FixedUpdateNetwork() {
		if (!HasSyncAuthority() || EntityController.player == null) return;

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
	//called when game over is detected as true
	private IEnumerator GameOverCoroutine() {
		UIController.instance.SetRespawnUIEnabled(false);
		UIController.instance.SetGameOverUIEnabled(true);

		for (float i = 10f - 0.00001f; i > 0f; i -= Time.deltaTime) {
			//UIController screen
			UIController.instance.SetGameOverTimerText($"Game over. Leaving in:\n{Mathf.CeilToInt(i)}");
			yield return null;
		}

		//leave game!
		ServerLinker.instance.StopGame();
	}
	public override void Spawned() {
		GameOverChanged();
		ScoreOrWaveChanged();

		if (HasSyncAuthority()) {
			Wave = Mathf.Max(PlayerPrefs.GetInt("debug_starting_wave") - 1, 0);
			RandomSeed = Random.Range(0, 1000000);
		}
	}
	private void Update() {
		if (GameOver && !gameOverInvoked) {
			StartCoroutine(GameOverCoroutine());
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
