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

	[Networked]
	private string PlayerHullName { get; set; } = "";
	public string GetHullName() { return PlayerHullName; }
	public void SetHullName(string hull) { PlayerHullName = hull; }

	[Networked]
	private float PlayerHullRotation { get; set; } = 20f;
	public float GetHullRotation() { return PlayerHullRotation; }
	public void SetHullRotation(float rotation) { PlayerHullRotation = rotation; }

	[Networked]
	private float PlayerTurretRotation { get; set; } = -20f;
	public float GetTurretRotation() { return PlayerTurretRotation; }
	public void SetTurretRotation(float rotation) { PlayerTurretRotation = rotation; }

	[Networked]
	private string PlayerTurretName { get; set; } = "";
	public string GetTurretName() { return PlayerTurretName; }
	public void SetTurretName(string turret) { PlayerTurretName = turret; }

	[Networked]
	private bool IsMasterClient { get; set; } = false;
	public bool GetIsMasterClient() { return IsMasterClient; }

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

			//quit if is master client when player is trying to join
			if (PlayerPrefs.GetInt("is_lobby_client") == 1 && Runner.IsSharedModeMasterClient) {
				PlayerPrefs.SetInt("joining_lobby_failed", 1);
				ServerLinker.instance.StopLobby();
				UnityEngine.SceneManagement.SceneManager.LoadScene(0);
				return;
			}
			//quit if exceeded players
			if (new List<PlayerRef>(Runner.ActivePlayers).Count > 4) {
				PlayerPrefs.SetInt("joining_lobby_full", 1);
				ServerLinker.instance.StopLobby();
				UnityEngine.SceneManagement.SceneManager.LoadScene(0);
				return;
			}
			LobbyUI.instance.JoinedLobby();
			ServerLinker.instance.SetIsInLobby(true);

			if (Runner.IsSharedModeMasterClient) {
				IsMasterClient = true;
			}
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
		if (!HasStateAuthority) return; //local player

		//update local UI; TODO: change this so it doesn't just send a big chunk of string text to the LobbyUI
		string lobbyDisplayText = "";

		//NOTE: room ID is stored locally through PlayerPrefs
		lobbyDisplayText += $"Lobby ID: {PlayerPrefs.GetString("room_id")}\n";
		lobbyDisplayText += $"Selected Map: {LobbyStatsSyncer.instance.GetMap()}\n";
		lobbyDisplayText += $"Starting Wave: {LobbyStatsSyncer.instance.GetStartingWave()}\n";

		//TODO: give LobbyUI all synced information here to actually display it
		foreach (LobbyPlayer p in LobbyUI.instance.GetLobbyPlayers()) {
			string readyText = p.GetIsReady() ? "Ready" : "Not Ready";
			string hostText = p.GetIsMasterClient() ? " (Host)" : "";
			lobbyDisplayText += $"\n{p.GetPlayerName()}{hostText}: {readyText}";
		}
		LobbyUI.instance.SetLobbyText(lobbyDisplayText);

		//master client decisions
		if (Runner.IsSharedModeMasterClient) {
			if (!IsMasterClient) IsMasterClient = true;

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
