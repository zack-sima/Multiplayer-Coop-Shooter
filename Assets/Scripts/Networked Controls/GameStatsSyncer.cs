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

	[Networked, OnChangedRender(nameof(ScoreChanged))]
	private int Score { get; set; } = 0;
	public void AddScore(int addition) { Score += addition; } //called by master client only

	[Networked] private int Wave { get; set; } = 0;
	public int GetWave() { return Wave; } //all spawners sync this and simulate same delays
	public void IncrementWave() { Wave++; } //called by master client only

	[Networked, OnChangedRender(nameof(GameOverChanged))]
	private bool GameOver { get; set; } = false;

	#endregion

	#region Callbacks

	//everyone displays the same score
	private void ScoreChanged() {
		UIController.instance.SetScoreText(Score);
	}

	private void GameOverChanged() {
	}

	#endregion

	#region Functions

	private void Update() {

	}
	private void Awake() {
		if (instance != null) {
			Debug.LogError("more than one instance!");
		}
		instance = this;
	}

	#endregion
}
