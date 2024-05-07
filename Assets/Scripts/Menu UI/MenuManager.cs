using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class MenuManager : MonoBehaviour {

	#region Statics & Enums

	public static MenuManager instance;

	public enum GameMode { Singleplayer, Coop }

	#endregion

	#region References

	//mode display (button)
	[SerializeField] private TMP_Text modeDisplayTitle, modeDisplayDescription;
	[SerializeField] private Image modeDispalyIcon;

	//debug screen (TODO: add in all other required stuff!)
	[SerializeField] private RectTransform debugScreen, menuScreen;
	public void SetMenuScreen(bool active) { menuScreen.gameObject.SetActive(active); }

	//play button (change text!)
	[SerializeField] private TMP_Text playButtonText;
	public void SetPlayButtonText(string text) { playButtonText.text = text; }

	[SerializeField] private TMP_InputField waveInput, playerNameInput;
	public TMP_InputField GetWaveInput() { return waveInput; }
	public TMP_InputField GetPlayerNameInput() { return playerNameInput; }

	//map select
	[SerializeField] private TMP_Dropdown mapDropdown;
	public TMP_Dropdown GetMapDropdown() { return mapDropdown; }

	//TODO: move this to a separate map scene manager that tracks each map's properties, name, scene index, etc
	[SerializeField] private List<string> mapDropdownSceneNames;
	public List<string> GetMapSceneNames() { return mapDropdownSceneNames; }

	//NOTE: this dropdown is used for actual game but the dropdown itself is usually hidden
	[SerializeField] private TMP_Dropdown hullDropdown, turretDropdown;
	public void SetHullDropdown(string val) {
		for (int i = 0; i < hullDropdown.options.Count; i++) {
			if (hullDropdown.options[i].text == val) {
				hullDropdown.value = i;
				PlayerHullChanged();
				break;
			}
		}
	}
	public void SetTurretDropdown(string val) {
		for (int i = 0; i < turretDropdown.options.Count; i++) {
			if (turretDropdown.options[i].text == val) {
				turretDropdown.value = i;
				PlayerTurretChanged();
				break;
			}
		}
	}

	#endregion

	#region Members

	//NOTE: this value determines what game mode the play button goes to
	private GameMode currentGameMode;
	public GameMode GetCurrentGameMode() { return currentGameMode; }

	#endregion

	#region Functions

	//TODO: TEMPORARY SWAP GAMEMODE INSTEAD OF GAME PAGE
	public void ToggleGameMode() {
		if (currentGameMode == GameMode.Singleplayer) {
			SetGameMode(GameMode.Coop);
		} else {
			SetGameMode(GameMode.Singleplayer);
		}
	}

	//TODO: mode select sets this mode -- this determines whether the play button goes to singleplayer or not
	public void SetGameMode(GameMode mode) {
		PlayerPrefs.SetInt("game_mode", (int)mode);

		currentGameMode = mode;
		GameModeChanged();
	}
	public void GameModeChanged() {
		switch (currentGameMode) {
			case GameMode.Coop:
				modeDisplayTitle.text = "SURVIVAL";
				modeDisplayDescription.text = "COOP";

				LobbyUI.instance.InLobbyUpdated();
				break;
			case GameMode.Singleplayer:
				modeDisplayTitle.text = "SURVIVAL";
				modeDisplayDescription.text = "SOLO";

				if (ServerLinker.instance.GetIsInLobby()) {
					ServerLinker.instance.StopLobby();
					SceneManager.LoadScene(0);
					return;
				}
				SetPlayButtonText("PLAY");
				LobbyUI.instance.InLobbyUpdated();
				break;
		}
	}
	public void PlayerTurretChanged() {
		//TODO: replace dropdown value pulls with directly setting player prefs for turret/hull
		PlayerPrefs.SetInt("turret_index", turretDropdown.value);
		PlayerPrefs.SetString("turret_name", turretDropdown.options[turretDropdown.value].text);

		LobbyUI.instance.SetPlayerTurret();
	}
	public void PlayerHullChanged() {
		//TODO: replace dropdown value pulls with directly setting player prefs for turret/hull
		PlayerPrefs.SetInt("hull_index", hullDropdown.value);
		PlayerPrefs.SetString("hull_name", hullDropdown.options[hullDropdown.value].text);

		LobbyUI.instance.SetPlayerHull();
	}

	//NOTE: play button goes to where currentGameMode is set to (SP game, lobby, etc)
	public void PlayButtonClicked() {
		switch (currentGameMode) {
			case GameMode.Singleplayer:
				StartSingle();
				break;
			case GameMode.Coop:
				if (!ServerLinker.instance.GetIsInLobby()) {
					PlayerPrefs.SetString("room_id", LobbyUI.GenerateLobbyID());
					StartLobby(PlayerPrefs.GetString("room_id"), false);
				} else {
					LobbyUI.instance.ToggleIsReady();
				}
				break;
		}
	}
	public void StartLobby(string lobbyId, bool isJoining) {
		if (lobbyId == "") return;

		//NOTE: lobbies directly use room_id; games have _g appended to it to distinguish it from lobby rooms
		PlayerPrefs.SetString("room_id", lobbyId);
		ServerLinker.instance.StartLobby(lobbyId, isJoining);

		//NOTE: this calls the lobby UI loading screen
		LobbyUI.instance.SetLobbyLoading(true);

		//TODO: when adding more gamemodes make sure this is still correct!
		SetGameMode(GameMode.Coop);
	}
	/// <summary>
	/// Returns -1 if there is no scene with matching name
	/// </summary>
	public static int GetSceneIndexByName(string name) {
		for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
			string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
			string sceneName = Path.GetFileNameWithoutExtension(scenePath);
			if (sceneName == name) return i;
		}
		return -1;
	}
	//NOTE: only call this from the lobby!
	public void StartShared(string mapName) {
		InitGame();

		ServerLinker.instance.StopLobby();

		int mapIndex = GetSceneIndexByName(mapName);

		//TODO: scale by challenge mode, difficulty, etc
		PlayerPrefs.SetInt("game_start_delay", 10);

		//saved lobby room ID + "_g" goes to correct game room
		if (mapIndex != -1)
			ServerLinker.instance.StartShared(mapIndex, PlayerPrefs.GetString("room_id") + "_g");
	}
	public void StartSingle() {
		int sceneIndex = GetSceneIndexByName(mapDropdownSceneNames[mapDropdown.value]);

		//TODO: scale by challenge mode, difficulty, etc
		PlayerPrefs.SetInt("game_start_delay", 10);

		if (sceneIndex != -1) {
			InitGame();
			ServerLinker.instance.StartSinglePlayer(sceneIndex);
		}
	}
	//NOTE: this button should not be shown to the player in builds
	public void ToggleDebug() {
		debugScreen.gameObject.SetActive(!debugScreen.gameObject.activeInHierarchy);
	}
	private void InitGame() {
		PlayerTurretChanged();
		PlayerHullChanged();

		//TODO: remove links to dropdowns in the future/relegate functions to debug only, being replaced
		//  by actual real UI buttons, etc

		PlayerPrefs.SetInt("map_index", mapDropdown.value);

		PlayerPrefs.SetString("player_name", playerNameInput.text);

		if (int.TryParse(waveInput.text, out int wave) && wave > 0) {
			PlayerPrefs.SetInt("debug_starting_wave", wave);
		} else {
			PlayerPrefs.SetInt("debug_starting_wave", 0);
		}
	}
	//don't make networking call until input was finished changing
	bool waitingChangeNameInput = false;
	private IEnumerator WaitForNameInputExit() {
		waitingChangeNameInput = true;
		while (playerNameInput.isFocused) yield return null;
		waitingChangeNameInput = false;

		AccountDataSyncer.instance.ChangedUsername();
	}
	private void Awake() {
		instance = this;
	}
	private void Start() {
		Application.targetFrameRate = 90;
		Time.timeScale = 1f;

		currentGameMode = (GameMode)PlayerPrefs.GetInt("game_mode");

		turretDropdown.value = PlayerPrefs.GetInt("turret_index");
		hullDropdown.value = PlayerPrefs.GetInt("hull_index");

		mapDropdown.value = PlayerPrefs.GetInt("map_index");

		playerNameInput.text = PlayerPrefs.GetString("player_name");

		if (PlayerPrefs.GetInt("debug_starting_wave") > 0)
			waveInput.text = PlayerPrefs.GetInt("debug_starting_wave").ToString();

		GameModeChanged();
	}
	private void Update() {
		string inputText = playerNameInput.text;
		if (PlayerPrefs.GetString("player_name") != inputText) {
			PlayerPrefs.SetString("player_name", inputText);
			if (!waitingChangeNameInput) StartCoroutine(WaitForNameInputExit());
		}

		PlayerPrefs.SetInt("turret_index", turretDropdown.value);
		PlayerPrefs.SetString("turret_name", turretDropdown.options[turretDropdown.value].text);

		PlayerPrefs.SetInt("hull_index", hullDropdown.value);
		PlayerPrefs.SetString("hull_name", hullDropdown.options[hullDropdown.value].text);
	}

	#endregion
}
