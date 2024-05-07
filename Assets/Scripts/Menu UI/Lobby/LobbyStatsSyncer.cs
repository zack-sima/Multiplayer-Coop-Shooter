using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class LobbyStatsSyncer : NetworkBehaviour {

	#region Statics

	public static LobbyStatsSyncer instance;

	#endregion

	#region Synced

	[Networked, OnChangedRender(nameof(MapChanged))]
	private string MapName { get; set; } = "[Map]";
	public string GetMap() { return MapName; }
	public void SetMap(string map) { MapName = map; }

	[Networked, OnChangedRender(nameof(WaveChanged))]
	private int StartingWave { get; set; } = 1;
	public int GetStartingWave() { return StartingWave; }
	public void SetStartingWave(int wave) { StartingWave = wave; }

	[Networked, OnChangedRender(nameof(GameStartedChanged))]
	private bool GameStarted { get; set; } = false;
	public void SetGameStarted() { GameStarted = true; } //called by master client only

	#endregion

	#region Callbacks

	private void MapChanged() {
		LobbyUI.instance.SetClientMap();
	}
	private void WaveChanged() {
		LobbyUI.instance.SetClientWave();
	}
	private void GameStartedChanged() {
		if (!GameStarted) return;

		//force all clients (master or not) to go to game scene once this is ever set to true
		MenuManager.instance.StartShared(MapName);
		LobbyUI.instance.SetGameStarting();
	}

	#endregion

	#region Functions

	public override void Spawned() {
		instance = this;

		MapChanged();
		WaveChanged();
		GameStartedChanged();
	}
	private void Update() {
	}

	#endregion

}
