using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script that just deals with lobby UI. TODO: actually integrate with good visuals, etc
/// </summary>

public class LobbyUI : MonoBehaviour {

	#region Statics

	public static LobbyUI instance;

	#endregion

	#region References

	//////////// NEW ///////////

	[SerializeField] private TMP_Text lobbyIdText;
	[SerializeField] private Button leaveLobbyButton;

	//lobby failed UI
	[SerializeField] private RectTransform lobbyJoinFailedScreen, lobbyJoinFullScreen;

	////////////////////////////

	//TODO: replace this with more fancy stuff (currently just a single string that displays everything)
	[SerializeField] private TMP_Text lobbyTextDisplay;

	[SerializeField] private RectTransform lobbyLoadingUI, gameStartingUI;

	#endregion

	#region Members

	private readonly List<LobbyPlayer> lobbyPlayers = new();
	public void AddLobbyPlayer(LobbyPlayer p) { if (!lobbyPlayers.Contains(p)) lobbyPlayers.Add(p); }
	public void RemoveLobbyPlayer(LobbyPlayer p) { if (lobbyPlayers.Contains(p)) lobbyPlayers.Remove(p); }
	public List<LobbyPlayer> GetLobbyPlayers() { return lobbyPlayers; }

	#endregion

	#region Functions

	public void DisableLobbyFailedScreen() {
		lobbyJoinFailedScreen.gameObject.SetActive(false);
		lobbyJoinFullScreen.gameObject.SetActive(false);
	}

	//sends current player information (master client) to LobbyStatsSyncer script
	private IEnumerator WaitInitData() {
		yield return new WaitForSeconds(0.2f);

		//TODO: make sure debug modes are controlled by toggle after setting up modes
		MapDropdownChanged();
		WaveInputChanged();

		PlayerNameInputChanged();
	}
	public void WaveInputChanged() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;

		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) {
			if (int.TryParse(MenuManager.instance.GetWaveInput().text, out int wave))
				LobbyStatsSyncer.instance.SetStartingWave(wave);
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
	//if the host changes the wave, change it on client too (still, only for testing)
	public void SetClientWaveInput() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;
		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) return;

		MenuManager.instance.GetWaveInput().text = LobbyStatsSyncer.instance.GetStartingWave().ToString();
		MenuManager.instance.GetWaveInput().interactable = false;
	}
	public void SetClientMapDropdown() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;
		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) return;

		List<TMP_Dropdown.OptionData> options = MenuManager.instance.GetMapDropdown().options;
		for (int i = 0; i < options.Count; i++) {
			if (options[i].text == LobbyStatsSyncer.instance.GetMap()) {
				MenuManager.instance.GetMapDropdown().value = i;
				MenuManager.instance.GetMapDropdown().RefreshShownValue();
				break;
			}
		}
		MenuManager.instance.GetMapDropdown().interactable = false;
	}
	public void InLobbyUpdated() {
		if (MenuManager.instance.GetCurrentGameMode() == MenuManager.GameMode.Singleplayer) {
			MenuManager.instance.SetPlayButtonText("PLAY");
		} else {
			if (ServerLinker.instance.GetIsInLobby()) {
				MenuManager.instance.SetPlayButtonText("SET READY");
			} else {
				MenuManager.instance.SetPlayButtonText("CREATE\nLOBBY");
			}
		}
	}
	//NOTE: procedurally generates a lobby ID
	public static string GenerateLobbyID() {
		string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		System.Random random = new((int)(Time.time * 10000));
		char[] id = new char[6];

		// Generate each character randomly
		for (int i = 0; i < id.Length; i++) {
			id[i] = chars[random.Next(chars.Length)];
		}

		return new string(id);
	}
	//NOTE: this is the function callback from ServerLinker when a lobby is successfully joined.
	public void JoinedLobby() {
		StartCoroutine(WaitInitData());

		leaveLobbyButton.gameObject.SetActive(true);
		lobbyIdText.text = $"ID: #{PlayerPrefs.GetString("room_id")}";

		FriendsManager.instance.CloseFriendsTab();
		MenuManager.instance.SetPlayButtonText("SET READY");
	}
	//TODO: modify the lobby controls script to give this script actual information instead of one string
	public void SetLobbyText(string text) {
		lobbyTextDisplay.text = text;
	}
	public void QuitLobby() {
		ServerLinker.instance.StopLobby();

		Destroy(ServerLinker.instance.gameObject);
		UnityEngine.SceneManagement.SceneManager.LoadScene(ServerLinker.LOBBY_SCENE);
	}
	//NOTE: this calls the singleton lobby player assuming the player exists
	public void ToggleIsReady() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;

		LobbyPlayer.playerInstance.ToggleIsReady();
	}
	public void SetLobbyLoading(bool loading) {
		lobbyLoadingUI.gameObject.SetActive(loading);
	}
	public void SetGameStarting() {
		gameStartingUI.gameObject.SetActive(true);
	}

	private void Awake() {
		instance = this;
	}
	private void Start() {
		if (PlayerPrefs.GetInt("joining_lobby_failed") == 1) {
			PlayerPrefs.SetInt("joining_lobby_failed", 0);
			lobbyJoinFailedScreen.gameObject.SetActive(true);
		}
		if (PlayerPrefs.GetInt("joining_lobby_full") == 1) {
			PlayerPrefs.SetInt("joining_lobby_full", 0);
			lobbyJoinFullScreen.gameObject.SetActive(true);
		}
	}

	#endregion

}
