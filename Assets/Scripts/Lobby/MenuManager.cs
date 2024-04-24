using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour {

	#region Statics & Enums

	public static MenuManager instance;

	public enum GameMode { Singleplayer, Coop }

	#endregion

	#region References

	//mode display (button)
	[SerializeField] private TMP_Text modeDisplayTitle, modeDisplayDescription;
	[SerializeField] private Image modeDispalyIcon;

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
	[SerializeField] private List<int> mapDropdownSceneIndices;

	//hull/turret
	[SerializeField] private TMP_Dropdown hullDropdown, turretDropdown;

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
					return;
				}
				SetPlayButtonText("PLAY");
				LobbyUI.instance.InLobbyUpdated();
				break;
		}
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
	}
	//NOTE: only call this from the lobby!
	public void StartShared(string mapName) {
		InitGame();

		ServerLinker.instance.StopLobby();

		int mapIndex = 1;

		//TODO: change this scuffed map name matching with dropdowns
		for (int i = 0; i < mapDropdown.options.Count; i++) {
			if (mapDropdown.options[i].text == mapName) {
				mapIndex = mapDropdownSceneIndices[i];
				break;
			}
		}

		//saved lobby room ID + "_g" goes to correct game room
		ServerLinker.instance.StartShared(mapIndex, PlayerPrefs.GetString("room_id") + "_g");
	}
	public void StartSingle() {
		InitGame();
		ServerLinker.instance.StartSinglePlayer(mapDropdownSceneIndices[mapDropdown.value]);
	}
	private void InitGame() {
		PlayerPrefs.SetInt("turret_index", turretDropdown.value);
		PlayerPrefs.SetString("turret_name", turretDropdown.options[turretDropdown.value].text);

		PlayerPrefs.SetInt("hull_index", hullDropdown.value);
		PlayerPrefs.SetString("hull_name", hullDropdown.options[hullDropdown.value].text);

		PlayerPrefs.SetString("player_name", playerNameInput.text);

		if (int.TryParse(waveInput.text, out int wave) && wave > 0) {
			PlayerPrefs.SetInt("debug_starting_wave", wave);
		} else {
			PlayerPrefs.SetInt("debug_starting_wave", 0);
		}
	}
	private void Awake() {
		instance = this;
	}
	private void Start() {
		Application.targetFrameRate = 90;

		currentGameMode = (GameMode)PlayerPrefs.GetInt("game_mode");

		turretDropdown.value = PlayerPrefs.GetInt("turret_index");
		hullDropdown.value = PlayerPrefs.GetInt("hull_index");

		playerNameInput.text = PlayerPrefs.GetString("player_name");

		if (PlayerPrefs.GetInt("debug_starting_wave") > 0)
			waveInput.text = PlayerPrefs.GetInt("debug_starting_wave").ToString();

		GameModeChanged();
	}
	private void Update() {
		PlayerPrefs.SetString("player_name", playerNameInput.text);

		PlayerPrefs.SetInt("turret_index", turretDropdown.value);
		PlayerPrefs.SetString("turret_name", turretDropdown.options[turretDropdown.value].text);

		PlayerPrefs.SetInt("hull_index", hullDropdown.value);
		PlayerPrefs.SetString("hull_name", hullDropdown.options[hullDropdown.value].text);
	}

	#endregion
}
