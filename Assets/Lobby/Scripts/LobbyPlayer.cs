using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Lobby;

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

	public void SetIsReady(bool isReady) {
		IsReady = isReady;
	}
	
	public void SetPlayerName(string name) {
		PlayerName = name;
		LobbyEventsHandler.RaisePlayerUpdate(this);
	}

	#region Photon Lifecycle

		//Photon built-in function called when object is spawned in
    	public override void Spawned() {
    		if (HasStateAuthority) {
    			playerInstance = this;
    			LobbyEventsHandler.InvokePlayerJoinLobby();
    			
    			// LobbyUI.instance.InitLocalSync();
    			// LobbyUI.instance.SetLobbyUIActive(true);
    
    			LobbyEventsHandler.RaiseLobbyIdChanged(PlayerPrefs.GetString("room_id"));
    
    			StartCoroutine(CheckForUIUpdate());
    			
    			if (Runner.IsSharedModeMasterClient) {
    				IsMasterClient = true;
				    StartCoroutine(CheckForAllPlayersReady());
			    }
    		}
    
    		//TODO: this adds player reference to the LobbyUI script, which is a little bit scuffed.
    		//  See TODO in Lobby about re-formatting this
    		// LobbyUI.instance.AddLobbyPlayer(this);
		    LobbyEventsHandler.RaisePlayerSpawn(this);
		    LobbyEventsHandler.RaisePlayerUpdate(this);
	    }
    	public override void Despawned(NetworkRunner runner, bool hasState) {
    		//TODO: same thing as the spawned function
    		// LobbyUI.instance.RemoveLobbyPlayer(this);
		    LobbyEventsHandler.RaisePlayerQuit(this);
    		
    		StopCoroutine(CheckForUIUpdate());
		    Debug.Log("Stopped coroutine"); 

		    if (IsMasterClient) {
			    StopCoroutine(CheckForAllPlayersReady());
		    }
    	}

	#endregion
	
	// private void Update() {
	// 	if (LobbyStatsSyncer.instance == null) return;
	// 	if (!HasStateAuthority) return; //local player
	//
	// 	//update local UI; TODO: change this so it doesn't just send a big chunk of string text to the LobbyUI
	// 	string lobbyDisplayText = "";
	//
	// 	//NOTE: room ID is stored locally through PlayerPrefs
	// 	// LobbyEventsHandler.RaiseLobbyIdChanged($"Lobby ID: {PlayerPrefs.GetString("room_id")}");
	// 	
	// 	LobbyEventsHandler.RaiseMapChanged($"Selected Map: {LobbyStatsSyncer.instance.GetMap()}");
	// 	LobbyEventsHandler.RaiseWaveChanged(LobbyStatsSyncer.instance.GetStartingWave());
	// 	
	// 	//TODO: give LobbyUI all synced information here to actually display it
	// 	foreach (LobbyPlayer p in LobbyUI.instance.GetLobbyPlayers()) {
	// 		string readyText = p.GetIsReady() ? "Ready" : "Not Ready";
	// 		string hostText = p.GetIsMasterClient() ? " (Host)" : "";
	// 		lobbyDisplayText += $"\n{p.GetPlayerName()}{hostText}: {readyText}";
	// 	}
	// 	LobbyUI.instance.SetLobbyText(lobbyDisplayText);
	//
	// 	//master client decisions
	// 	if (Runner.IsSharedModeMasterClient) {
	// 		if (!IsMasterClient) IsMasterClient = true;
	//
	// 		//start game if all players are ready
	// 		bool allPlayersReady = true;
	// 		foreach (LobbyPlayer p in LobbyUI.instance.GetLobbyPlayers()) {
	// 			if (!p.GetIsReady()) {
	// 				allPlayersReady = false;
	// 				break;
	// 			}
	// 		}
	// 		//start game!
	// 		if (allPlayersReady) {
	// 			Debug.Log("All players ready; starting game...");
	// 			LobbyStatsSyncer.instance.SetGameStarted();
	// 		}
	// 	}
	// }

	#endregion

	#region Periodic Checks

		private IEnumerator CheckForUIUpdate() {
			Debug.Log("Started coroutine");
    		while (true) {
    			if (!HasStateAuthority) break;
			    
			    // wait for LobbyStatsSyncer to set to an instance
			    while (LobbyStatsSyncer.instance == null) {
				    for (int i = 0; i < 10; i++) {
					    if (LobbyStatsSyncer.instance != null) break;
					    yield return new WaitForSeconds(1f);
				    }
					
				    // breaks if it takes longer than 10 seconds
				    if (LobbyStatsSyncer.instance == null) {
					    Debug.LogWarning("LobbyStatsSyncer is null!");
					    break;
				    }
			    }
			    if (LobbyStatsSyncer.instance == null) break;
    			
    			UpdateLobbyDisplay();
			    
    			yield return new WaitForSeconds(1f);
    		}
		    Debug.Log("Broke from coroutine");
    	}

	    private IEnumerator CheckForAllPlayersReady() {
		    while (true) {
			    if (!HasStateAuthority || !IsMasterClient) break;
			    
			    // wait for LobbyStatsSyncer to set to an instance
			    while (LobbyStatsSyncer.instance == null) {
				    for (int i = 0; i < 10; i++) {
					    if (LobbyStatsSyncer.instance != null) break;
					    yield return new WaitForSeconds(1f);
				    }
					
				    // breaks if it takes longer than 10 seconds
				    if (LobbyStatsSyncer.instance == null) {
					    Debug.LogWarning("LobbyStatsSyncer is null!");
					    break;
				    }
			    }
			    if (LobbyStatsSyncer.instance == null) break;
    			
			    CheckAllPlayersReady();
			    
			    yield return new WaitForSeconds(0.2f);
		    }
	    }

	    #region Apprentice Methods

	    private void UpdateLobbyDisplay() {
		    Debug.Log("Synced UI Changes");
		    LobbyEventsHandler.RaiseMapChanged(LobbyStatsSyncer.instance.GetMap());
		    LobbyEventsHandler.RaiseWaveChanged(LobbyStatsSyncer.instance.GetStartingWave());
	    }

	    private void CheckAllPlayersReady() {
		    //master client decisions
		    if (Runner.IsSharedModeMasterClient) {
			    if (!IsMasterClient) IsMasterClient = true;

			    //start game if all players are ready
			    bool allPlayersReady = true;
			    foreach (LobbyPlayer p in LobbyUI.instance.GetLobbyPlayers().Keys) {
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

	#endregion
	
}
