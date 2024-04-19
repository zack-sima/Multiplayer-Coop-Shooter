using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lobby;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.UI.ProceduralImage;

/// <summary>
/// Script that just deals with lobby UI. TODO: actually integrate with good visuals, etc
/// </summary>

public class LobbyUI : MonoBehaviour {

	#region Statics

	public static LobbyUI instance;

	#endregion

	#region References

	[Header("Lobby Buttons")]
	[SerializeField] private Button quitButton;
	[SerializeField] private Button readyButton;

	//TODO: replace this with more fancy stuff (currently just a single string that displays everything)
	[Header("Display Info Fields")]
	[SerializeField] private TMP_Text lobbyTextDisplay;
	[SerializeField] private TMP_Text waveTextDisplay, mapTextDisplay;

	[Header("Loading UI's")] 
	[SerializeField] private RectTransform lobbyLoadingUI;
	[SerializeField] private RectTransform gameStartingUI;
	
	
	[Header("Lobby Screen")]
	[SerializeField] private Button createRoomButton;
	[SerializeField] private TMP_Text roomText;


	[Header("Join Lobby")] [SerializeField]
	private TMP_InputField joinLobbyInput;
	[SerializeField] private Button joinLobbyButton;
	
	[Header("Player Cards")]
	[SerializeField] private GameObject playersContainer;
	[SerializeField] private GameObject playerPrefab;

	[Header("Error Messages")]
	[SerializeField] private GameObject errorMessageContainer;
	[SerializeField] private GameObject errorMessagePrefab;
	
	#endregion

	#region Lobby Players
	/* Lobby Players Dictionary */
	//TODO: find a better way to access lobby players than putting them in a UI script!

	private Dictionary<LobbyPlayer, GameObject> lobbyPlayers = new();
	public Dictionary<LobbyPlayer, GameObject> GetLobbyPlayers() { return lobbyPlayers; }
	
	/* On Player Joining */

	private void SubscribeLobbyPlayerSpawn() {
		LobbyEventsHandler.OnPlayerSpawn += AddLobbyPlayer;
	}
	private void UnsubscribeLobbyPlayerSpawn() {
		LobbyEventsHandler.OnPlayerSpawn -= AddLobbyPlayer;
	}
	
	private void AddLobbyPlayer(LobbyPlayer p) {
		if (!lobbyPlayers.ContainsKey(p)) {
			lobbyPlayers.Add(p, Instantiate(playerPrefab, playersContainer.transform));
		}
	}

	/* On Player Quitting */
	private void SubscribeLobbyPlayerQuit() {
		LobbyEventsHandler.OnPlayerQuit += RemoveLobbyPlayer;
	}
	private void UnsubscribeLobbyPlayerQuit() {
		LobbyEventsHandler.OnPlayerQuit -= RemoveLobbyPlayer;
	}
	
	private void RemoveLobbyPlayer(LobbyPlayer p) {
		if (lobbyPlayers.ContainsKey(p)) {
			Destroy(lobbyPlayers[p]);
			lobbyPlayers.Remove(p);
		}
	}

	#endregion

	#region Lifecycle Start & End

		private void Awake() {
    		instance = this;
    	}
    	private void Start() {
    		SetLobbyUIActive(false);
		    
		    SubscribeOnJoinLobby();
		    SubscribeLobbyPlayerSpawn();
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
		// joinLobbyButton.enabled = !isActive;
		
		//NOTE: on room creation
		createRoomButton.gameObject.SetActive(!isActive);
		roomText.gameObject.SetActive(isActive);

		if (!isActive) readyButton.GetComponentInChildren<TMP_Text>().text = "Solo"; 
		else readyButton.GetComponentInChildren<TMP_Text>().text = "Ready";
		
		//NOTE: activate/deactivate lobby info & their subscriptions
		if (isActive) SubscribeWaveInfo(); else UnsubscribeWaveInfo();
		if (isActive) SubscribeMapInfo(); else UnsubscribeMapInfo();
		if (isActive) SubscribeLobbyId(); else UnsubscribeLobbyId();
		if (isActive) SubscribeOnPlayerUpdate(); else UnsubscribeOnPlayerUpdate();
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
	    }

	    public void UpperRoomCodeInput() {
		    joinLobbyInput.text = joinLobbyInput.text.ToUpper();
	    }

	#endregion
	
	//NOTE: this calls the singleton lobby player assuming the player exists
	public void ToggleIsReady() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;
		
		LobbyPlayer.playerInstance.ToggleIsReady();
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

	#region Player Cards

	private void SetPlayerName(GameObject playerCard, string name) {
		playerCard.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = name;
	}
	
	private void SetPlayerStatus(GameObject playerCard, bool status) {
		if (status) {
			playerCard.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = "Ready";
			playerCard.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().color = Color.green;
			readyButton.GetComponent<Image>().color = Color.green;
		}
		else {
			playerCard.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = "Not Ready";
			playerCard.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().color = Color.red;
			readyButton.GetComponent<Image>().color = Color.red;
		}
	}
	
	private void SetPlayerIsHost(GameObject playerCard, bool isHost) {
		playerCard.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(isHost);
	}
	
	private void SetPlayerIsStateAuthority(GameObject playerCard, bool isAuthority) {
		playerCard.transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(isAuthority);
	}

	#endregion
	#endregion
	
	#region Button Methods
    
    	public void ActivateScreen(GameObject screen) {
    		screen.SetActive(true);
    	}
    
    	public void DeactivateScreen(GameObject screen) {
    		screen.SetActive(false);
    	}

	    public void CreateRoom(TMP_Text text) {
		    //NOTE: generates random 8 char code
		    System.Random random = new System.Random();
		    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		    string roomCode = new string(Enumerable.Repeat(chars, 8)
			    .Select(s => s[random.Next(s.Length)]).ToArray());
		    
		    //NOTE: lobbies directly use room_id; games have _g appended to it to distinguish it from lobby rooms
		    PlayerPrefs.SetString("room_id", roomCode);
		    ServerLinker.instance.StartLobby(roomCode);

		    //NOTE: this calls the lobby UI loading screen
		    SetLobbyLoading(true);
		    
		    //NOTE: turn button into text
		    text.text = $"Room:\n{roomCode}";
	    }
	    
	    public void JoinLobby() {
		    if (joinLobbyInput.text == "") return;
		    if (LobbyPlayer.playerInstance) return;
		    
		    //NOTE: lobbies directly use room_id; games have _g appended to it to distinguish it from lobby rooms
		    PlayerPrefs.SetString("room_id", joinLobbyInput.text);
		    ServerLinker.instance.StartLobby(joinLobbyInput.text);

		    roomText.text = $"Room:\n{joinLobbyInput.text}";
		    
		    //NOTE: this calls the lobby UI loading screen
		    SetLobbyLoading(true);

		    StartCoroutine(WaitCheckLobbyEmpty());
	    }

	    private IEnumerator WaitCheckLobbyEmpty() {
		    yield return new WaitUntil(() => LobbyPlayer.playerInstance != null);
            		    
		    if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) {
			    ThrowErrorMessage("Invalid Lobby Code!");
			    QuitLobby();
			    
		    }
	    }
	    
	    public void QuitLobby() {
		    // LobbyEventsHandler.RaisePlayerUpdate(LobbyPlayer.playerInstance);
		    ServerLinker.instance.StopLobby();
		    SetLobbyUIActive(false);

		    Destroy(ServerLinker.instance.gameObject);
		    
		    SceneManager.LoadScene(ServerLinker.LOBBY_SCENE);
	    }
    
    	#endregion


	    #region Debugging

	    private void ThrowErrorMessage(string message) {
		    GameObject newMessage = Instantiate(errorMessagePrefab, errorMessageContainer.transform);
		    newMessage.GetComponentInChildren<TMP_Text>().text = message;

		    StartCoroutine(WaitToDestroyMessage(newMessage));
	    }

	    private IEnumerator WaitToDestroyMessage(GameObject message) {
		    yield return new WaitForSeconds(2f);

		    message.GetComponent<SpriteRenderer>().DOFade(0, 1f);

		    yield return new WaitForSeconds(1f);
		    
		    Destroy(message);
	    }

	    #endregion
}
