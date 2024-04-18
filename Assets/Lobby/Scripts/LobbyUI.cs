using System;
using System.Collections;
using System.Collections.Generic;
using Lobby;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

/// <summary>
/// Script that just deals with lobby UI. TODO: actually integrate with good visuals, etc
/// </summary>

public class LobbyUI : MonoBehaviour {

	#region Statics

	public static LobbyUI instance;

	#endregion

	#region References

	//disable this if not in a lobby
	[SerializeField] private Button quitButton, readyButton;

	//TODO: replace this with more fancy stuff (currently just a single string that displays everything)
	[SerializeField] private TMP_Text lobbyTextDisplay, waveTextDisplay, mapTextDisplay;
	
	[SerializeField] private RectTransform lobbyLoadingUI, gameStartingUI;

	[SerializeField] private Button joinLobbyButton;

	[SerializeField] private GameObject playersContainer, playerPrefab;

	#endregion

	#region Members

	//TODO: find a better way to access lobby players than putting them in a UI script!
	private Dictionary<LobbyPlayer, GameObject> lobbyPlayers = new();

	public void AddLobbyPlayer(LobbyPlayer p) {
		if (!lobbyPlayers.ContainsKey(p)) {
			lobbyPlayers.Add(p, Instantiate(playerPrefab, playersContainer.transform));
		}
	}
	
	private void SubscribeLobbyPlayerSpawn() {
		LobbyEventsHandler.OnPlayerSpawn += AddLobbyPlayer;
	}
	private void UnsubscribeLobbyPlayerSpawn() {
		LobbyEventsHandler.OnPlayerSpawn -= AddLobbyPlayer;
	}

	public void RemoveLobbyPlayer(LobbyPlayer p) {
		if (lobbyPlayers.ContainsKey(p)) {
			Destroy(lobbyPlayers[p]);
			lobbyPlayers.Remove(p);
		}
	}

	private void SubscribeLobbyPlayerQuit() {
		LobbyEventsHandler.OnPlayerQuit += RemoveLobbyPlayer;
	}
	private void UnsubscribeLobbyPlayerQuit() {
		LobbyEventsHandler.OnPlayerQuit -= RemoveLobbyPlayer;
	}
	
	public Dictionary<LobbyPlayer, GameObject> GetLobbyPlayers() { return lobbyPlayers; }

	#endregion

	#region Lifecycle Start & End

		private void Awake() {
    		instance = this;
    	}
    	private void Start() {
    		SetLobbyUIActive(false);
    		
		    SubscribeOnJoinLobby();
		    SubscribeLobbyPlayerSpawn();
		    SubscribeOnPlayerUpdate();
		    SubscribeLobbyPlayerQuit();
	    }

	    private void OnDestroy() {
		    UnsubscribeFromAllEvents();
	    }

	    private void UnsubscribeFromAllEvents() {
		    UnsubscribeOnJoinLobby();
		    UnsubscribeLobbyPlayerSpawn();
		    UnsubscribeLobbyPlayerQuit();
		    UnsubscribeAllInfoListeners();
	    }
	#endregion
	
	#region Functions

	#region OnJoinLobby

	private void OnJoinLobby() {
		InitLocalSync();
		SetLobbyUIActive(true);
	}

	private void SubscribeOnJoinLobby() {
		LobbyEventsHandler.OnJoinLobby += OnJoinLobby;
	}

	private void UnsubscribeOnJoinLobby() {
		LobbyEventsHandler.OnJoinLobby -= OnJoinLobby;
	}

	#region Apprentice Methods

	//sends current player information (master client) to LobbyStatsSyncer script
	public void InitLocalSync() {
		StartCoroutine(WaitInitData());
	}
	
	private IEnumerator WaitInitData() {
		yield return new WaitForSeconds(0.2f);

		WaveInputChanged();
		MapDropdownChanged();
		PlayerNameInputChanged();
	}
	
	public void SetLobbyUIActive(bool isActive) {
		if (!isActive) {
			lobbyTextDisplay.text = "Not currently in a lobby";
		} else {
			lobbyTextDisplay.text = "Loading lobby...";
		}
		quitButton.gameObject.SetActive(isActive);
		readyButton.enabled = isActive;
		joinLobbyButton.enabled = !isActive;
		
		//NOTE: activate/deactivate lobby info & their subscriptions
		if (isActive) SubscribeWaveInfo(); else UnsubscribeWaveInfo();
		if (isActive) SubscribeMapInfo(); else UnsubscribeMapInfo();
		if (isActive) SubscribeLobbyId(); else UnsubscribeLobbyId();
	}

	#endregion

	#endregion

	#region TMPro Callbacks

		public void WaveInputChanged() {
    		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;
    
    		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) {
    			if (int.TryParse(MenuManager.instance.GetWaveInput().text, out int wave)) {
    				LobbyStatsSyncer.instance.SetStartingWave(wave);
    			}
    		}
    	}
    	public void MapDropdownChanged() {
    		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;
    
    		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) {
    			LobbyStatsSyncer.instance.SetMap(MenuManager.instance.GetMapDropdown().options[
    				MenuManager.instance.GetMapDropdown().value].text);
    		}
    	}
    	public void PlayerNameInputChanged() {
    		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;
    
    		if (MenuManager.instance.GetPlayerNameInput().text != "") {
    			LobbyPlayer.playerInstance.SetPlayerName(MenuManager.instance.GetPlayerNameInput().text);
    		} else {
    			LobbyPlayer.playerInstance.SetPlayerName("Player");
    		}

		    LobbyEventsHandler.RaisePlayerUpdate(LobbyPlayer.playerInstance);
	    }

	#endregion
	
	//NOTE: this calls the singleton lobby player assuming the player exists
	public void ToggleIsReady() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;

		LobbyPlayer.playerInstance.ToggleIsReady();
		LobbyEventsHandler.RaisePlayerUpdate(LobbyPlayer.playerInstance);
	}
	public void SetLobbyLoading(bool loading) {
		lobbyLoadingUI.gameObject.SetActive(loading);
		StartCoroutine(LobbyLoadingTimeout(lobbyLoadingUI.gameObject));
	}
	public void SetGameStarting() {
		gameStartingUI.gameObject.SetActive(true);
		StartCoroutine(LobbyLoadingTimeout(gameStartingUI.gameObject));
	}
	//if something fishy happens with the lobby loader, don't leave the UI on forever
	private IEnumerator LobbyLoadingTimeout(GameObject ui) {
		for (float t = 0; t < 10f; t += Time.deltaTime) {
			yield return null;
			if (!ui.activeInHierarchy) {
				Debug.Log("lobby load hidden");
				yield break;
			}
		}
		ui.SetActive(true);
	}

	#endregion


	#region Button Methods

	public void ActivateScreen(GameObject screen) {
		screen.SetActive(true);
	}

	public void DeactivateScreen(GameObject screen) {
		screen.SetActive(false);
	}

	#endregion

	#region Lobby Info

	private void UnsubscribeAllInfoListeners() {
		UnsubscribeWaveInfo();
		UnsubscribeMapInfo();
		UnsubscribeLobbyId();
		UnsubscribeOnPlayerUpdate();
	}

	private void SubscribeWaveInfo() {
		if (MenuManager.instance.GetWaveInput()) MenuManager.instance.GetWaveInput().enabled = true;
		LobbyEventsHandler.OnWaveChanged += UpdateWaveInfo;
	}
	
	private void UnsubscribeWaveInfo() {
		if (MenuManager.instance.GetWaveInput()) MenuManager.instance.GetWaveInput().enabled = false;
		LobbyEventsHandler.OnWaveChanged -= UpdateWaveInfo;
	}
	
	private void UpdateWaveInfo(int waveNum) {
		waveTextDisplay.text = $"Starting Wave: {waveNum.ToString()}";
		
		//NOTE: so that wave is still the same if master client switches
		if (!LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient)
			MenuManager.instance.GetWaveInput().text = waveNum.ToString();
	}
	
	
	private void SubscribeMapInfo() {
		if (MenuManager.instance.GetWaveInput()) MenuManager.instance.GetWaveInput().enabled = true;
		LobbyEventsHandler.OnMapChanged += UpdateMapInfo;
	}
	
	private void UnsubscribeMapInfo() {
		if (MenuManager.instance.GetWaveInput()) MenuManager.instance.GetWaveInput().enabled = false;
		LobbyEventsHandler.OnMapChanged -= UpdateMapInfo;
	}
	
	private void UpdateMapInfo(string map) {
		mapTextDisplay.text = $"Selected Map: {map}";
		
		//NOTE: so that wave is still the same if master client switches
		if (!LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient)
			for (int i = 0; i < MenuManager.instance.GetMapDropdown().options.Count; i++) {
				if (MenuManager.instance.GetMapDropdown().options[i].text == map) 
					MenuManager.instance.GetMapDropdown().value = i;
			}
	}
	
	private void SubscribeLobbyId() {
		LobbyEventsHandler.OnLobbyIdChanged += UpdateLobbyIdInfo;
	}
	
	private void UnsubscribeLobbyId() {
		LobbyEventsHandler.OnLobbyIdChanged -= UpdateLobbyIdInfo;
	}
	
	private void UpdateLobbyIdInfo(string id) {
		lobbyTextDisplay.text = $"Lobby Id: {id}";

		//NOTE: so that wave is still the same if master client switches
		if (!LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient)
			MenuManager.instance.GetRoomInput().text = id;
	}

	private void SubscribeOnPlayerUpdate() {
		LobbyEventsHandler.OnPlayerUpdate += OnPlayerUpdate;
	}
	
	private void UnsubscribeOnPlayerUpdate() {
		LobbyEventsHandler.OnPlayerUpdate -= OnPlayerUpdate;
	}
	
	private void OnPlayerUpdate(LobbyPlayer player) {
		SetPlayerName(lobbyPlayers[player], player.GetPlayerName());
		SetPlayerStatus(lobbyPlayers[player], player.GetIsReady());
		SetPlayerIsHost(lobbyPlayers[player], player.GetIsMasterClient());
		SetPlayerIsStateAuthority(lobbyPlayers[player], player == LobbyPlayer.playerInstance);
	}
	
	#endregion

	#region Player Cards

	private void SetPlayerName(GameObject playerCard, string name) {
		playerCard.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = name;
	}
	
	private void SetPlayerStatus(GameObject playerCard, bool status) {
		if (status) {
			playerCard.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = "Ready";
			playerCard.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().color = Color.green;
		}
		else {
			playerCard.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = "Not Ready";
			playerCard.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().color = Color.red;
		}
	}
	
	private void SetPlayerIsHost(GameObject playerCard, bool isHost) {
		playerCard.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(isHost);
	}
	
	private void SetPlayerIsStateAuthority(GameObject playerCard, bool isAuthority) {
		playerCard.transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(isAuthority);
	}

	#endregion
}
