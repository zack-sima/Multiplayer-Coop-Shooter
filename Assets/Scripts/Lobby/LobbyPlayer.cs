using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class LobbyPlayer : NetworkBehaviour {

	#region Const & Statics

	public static LobbyPlayer playerInstance;

	#endregion

	#region Synced

	//TODO: add more parameters you want other players to see, like skins, selected turret, perks, etc

	[Networked]
	private bool IsReady { get; set; } = false;
	public bool GetIsReady() { return IsReady; }

	[Networked]
	private string PlayerName { get; set; } = "Player";
	public string GetPlayerName() { return PlayerName; }

	#endregion

	#region Callbacks

	#endregion

	#region Functions

	//NOTE: the following setters must be called by local playerInstance!
	public void ToggleIsReady() {
		IsReady = !IsReady;
	}
	public void SetPlayerName(string name) {
		PlayerName = name;
	}

	//Photon built-in function called when object is spawned in
	public override void Spawned() {
		if (HasStateAuthority) {
			playerInstance = this;
			LobbyUI.instance.InitLocalSync();
			LobbyUI.instance.SetLobbyUIActive(true);
		}

		//TODO: this adds player reference to the LobbyUI script, which is a little bit scuffed.
		//  See TODO in Lobby about re-formatting this
		LobbyUI.instance.AddLobbyPlayer(this);
	}
	public override void Despawned(NetworkRunner runner, bool hasState) {
		//TODO: same thing as the spawned function
		LobbyUI.instance.RemoveLobbyPlayer(this);
	}
	private void Update() {
		if (LobbyStatsSyncer.instance == null) return;

		//update local UI; TODO: change this so it doesn't just send a big chunk of string text to the LobbyUI
		if (HasStateAuthority) {
			string lobbyDisplayText = "";

			//NOTE: room ID is stored locally through PlayerPrefs
			lobbyDisplayText += $"Lobby ID: {PlayerPrefs.GetString("room_id")}\n";
			lobbyDisplayText += $"Selected Map: {LobbyStatsSyncer.instance.GetMap()}\n";
			lobbyDisplayText += $"Starting Wave: {LobbyStatsSyncer.instance.GetStartingWave()}\n";

			//TODO: give LobbyUI all synced information here to actually display it
			foreach (LobbyPlayer p in LobbyUI.instance.GetLobbyPlayers()) {
				string readyText = p.GetIsReady() ? "Ready" : "Not Ready";
				string hostText = p.Runner.IsSharedModeMasterClient ? " (Host)" : "";
				lobbyDisplayText += $"\n{p.GetPlayerName()}{hostText}: {readyText}";
			}
			LobbyUI.instance.SetLobbyText(lobbyDisplayText);
		}

		//master client decisions
		if (Runner.IsSharedModeMasterClient) {
			//start game if all players are ready
			bool allPlayersReady = true;
			foreach (LobbyPlayer p in LobbyUI.instance.GetLobbyPlayers()) {
				if (!p.GetIsReady()) {
					allPlayersReady = false;
					break;
				}
			}
			//start game!
			if (allPlayersReady) {
				Debug.Log("All players ready; starting game...");
				LobbyStatsSyncer.instance.SetGameStarted();
			}
		}
	}

	#endregion

}
