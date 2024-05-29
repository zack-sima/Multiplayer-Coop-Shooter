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

	public enum GameMode { Singleplayer, Coop, Comp, PointCap }

	#endregion

	#region References

	//mode display (button)
	[SerializeField] private TMP_Text modeDisplayTitle, modeDisplayDescription;
	[SerializeField] private Image modeDispalyIcon;

	//debug screen
	[SerializeField] private RectTransform debugScreen, menuScreen;
	public void SetMenuScreen(bool active) { menuScreen.gameObject.SetActive(active); }

	//play button (change text!)
	[SerializeField] private TMP_Text playButtonText;
	public void SetPlayButtonText(string text) { playButtonText.text = text; }

	[SerializeField] private TMP_InputField playerNameInput;
	public TMP_InputField GetPlayerNameInput() { return playerNameInput; }

	[SerializeField] private TMP_InputField waveInput;

	[SerializeField] private GameObject garageMap, outdoorsMap;

	#endregion

	#region Members

	//NOTE: this value determines what game mode the play button goes to
	private GameMode currentGameMode;
	public GameMode GetCurrentGameMode() { return currentGameMode; }

	//NOTE: use this as actual map
	private string selectedMapName = "Military_1";
	public string GetSelectedMap() { return selectedMapName; }
	public void SetSelectedMap(string newMap) {
		if (newMap != "") selectedMapName = newMap;
		PlayerPrefs.SetString("game_map_name", selectedMapName);
	}

	//maps stuff
	[SerializeField] private List<string> mapSceneNames;
	public List<string> GetMapSceneNames() { return mapSceneNames; }

	//what is actually shown to the player
	[SerializeField] private List<string> mapDisplayNames;

	private Dictionary<string, string> mapNamesToDisplayDict = new();
	public string GetDisplayedMapName(string internalMapName) {
		if (!mapNamesToDisplayDict.ContainsKey(internalMapName)) return internalMapName;
		return mapNamesToDisplayDict[internalMapName];
	}

	//NOTE: player's chosen hull/turret here (replaced dropdowns)
	private string currentHull = "Tank", currentTurret = "Autocannon";
	public string GetPlayerHull() { return currentHull; }
	public string GetPlayerTurret() { return currentTurret; }

	public void SetHull(string val, bool isTemporary = false) {
		currentHull = val;
		PlayerHullChanged(isTemporary);
	}
	public void SetTurret(string val, bool isTemporary = false) {
		currentTurret = val;
		PlayerTurretChanged(isTemporary);
	}

	//NOTE: also set by level select (rapid mode = higher starting waves; TODO: modes, difficulty, etc)
	private int currentWave = 0;
	public int GetWave() { return currentWave; }
	public void SetWave(int wave) {
		currentWave = wave;

		if (LobbyStatsSyncer.instance != null && LobbyStatsSyncer.instance.Runner.IsSharedModeMasterClient) {
			LobbyUI.instance.WaveChanged(wave);
		}
	}

	//NOTE: 0 = standard, 1 = hardened, 2 = special difficulty, tbd
	private int currentCoopDifficulty = 0;
	public int GetDifficulty() { return currentCoopDifficulty; }
	public void SetDifficulty(int difficulty) {
		currentCoopDifficulty = difficulty;
		PlayerPrefs.SetInt("game_start_difficulty", currentCoopDifficulty);

		if (LobbyStatsSyncer.instance != null && LobbyStatsSyncer.instance.Runner.IsSharedModeMasterClient) {
			LobbyUI.instance.DifficultyChanged(difficulty);
		}
	}

	//for waiting coroutine
	bool waitingChangeNameInput = false;

	//0 = garage, 1 = repairs
	int lastClosedId = 0;

	#endregion

	#region Functions

	public void SetLastClosed(int id) {
		lastClosedId = id;
	}
	public int GetLastClosedId() {
		return lastClosedId;
	}
	public void SetGameMode(GameMode mode) {
		PlayerPrefs.SetInt("game_mode", (int)mode);
		PlayerPrefs.SetString("last_map", selectedMapName);

		currentGameMode = mode;
		GameModeChanged();
	}
	public void GameModeChanged() {
		modeDisplayDescription.text = GetDisplayedMapName(selectedMapName);

		switch (currentGameMode) {
			case GameMode.Comp:
				modeDisplayTitle.text = "PvP";

				LobbyUI.instance.InLobbyUpdated();

				break;
			case GameMode.PointCap:
				modeDisplayTitle.text = "POINT CAP";

				LobbyUI.instance.InLobbyUpdated();

				break;
			case GameMode.Coop:
				modeDisplayTitle.text = "CO-OP";

				if (currentCoopDifficulty / 10 == 1) modeDisplayTitle.text += " (HARD)";
				if (currentCoopDifficulty / 10 == 2) modeDisplayTitle.text += " (RAPID)";

				LobbyUI.instance.InLobbyUpdated();

				break;
			case GameMode.Singleplayer:
				modeDisplayTitle.text = "SOLO";

				if (currentCoopDifficulty / 10 == 1) modeDisplayTitle.text += " (HARD)";
				if (currentCoopDifficulty / 10 == 2) modeDisplayTitle.text += " (RAPID)";

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
	public void PlayerTurretChanged(bool isTemporary = false) {
		PlayerPrefs.SetString(isTemporary ? "temp_turret_name" : "turret_name", currentTurret);

		LobbyUI.instance.SetPlayerTurret(isTemporary);
	}
	public void PlayerHullChanged(bool isTemporary = false) {
		PlayerPrefs.SetString(isTemporary ? "temp_hull_name" : "hull_name", currentHull);

		LobbyUI.instance.SetPlayerHull(isTemporary);
	}

	//NOTE: play button goes to where currentGameMode is set to (SP game, lobby, etc)
	public void PlayButtonClicked() {
		PlayerPrefs.SetInt("game_mode", (int)currentGameMode);

		switch (currentGameMode) {
			case GameMode.Singleplayer:
				StartSingle();
				break;
			case GameMode.PointCap:
			case GameMode.Comp:
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

		SetGameMode((GameMode)PlayerPrefs.GetInt("game_mode"));
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
	public int GetCashByDifficulty(int difficulty) {
		difficulty /= 10;

		return difficulty switch {
			0 => 250,
			1 => 500,
			2 => 2500,
			_ => 250,
		};
	}
	public int GetStartDelayByDifficulty(int difficulty) {
		difficulty /= 10;

		return difficulty switch {
			0 => 5,
			1 => 5,
			2 => 15,
			_ => 5
		};
	}
	//NOTE: only call this from the lobby!
	public void StartShared(string mapName) {
		InitGame();

		ServerLinker.instance.StopLobby();

		int mapIndex = GetSceneIndexByName(mapName);

		//TODO: scale by challenge mode, difficulty, etc
		PlayerPrefs.SetInt("game_start_delay", GetStartDelayByDifficulty(currentCoopDifficulty));
		PlayerPrefs.SetInt("game_start_cash", GetCashByDifficulty(currentCoopDifficulty));
		PlayerPrefs.SetInt("game_start_wave", currentWave);
		PlayerPrefs.SetInt("game_start_difficulty", currentCoopDifficulty);
		PlayerPrefs.SetString("game_map_name", mapName);

		//saved lobby room ID + "_g" goes to correct game room
		if (mapIndex != -1)
			ServerLinker.instance.StartShared(mapIndex, PlayerPrefs.GetString("room_id") + "_g");
	}
	public void StartSingle() {
		int sceneIndex = GetSceneIndexByName(selectedMapName);

#if UNITY_EDITOR
		if (waveInput.text != "" && int.TryParse(waveInput.text, out int wave)) {
			currentWave = wave;
		}
#endif
		//TODO: scale by challenge mode, difficulty, etc
		print(GetCashByDifficulty(currentCoopDifficulty));
		PlayerPrefs.SetInt("game_start_delay", 5);
		PlayerPrefs.SetInt("game_start_cash", GetCashByDifficulty(currentCoopDifficulty));
		PlayerPrefs.SetInt("game_start_wave", currentWave);
		PlayerPrefs.SetInt("game_start_difficulty", currentCoopDifficulty);
		PlayerPrefs.SetString("game_map_name", selectedMapName);

		if (sceneIndex != -1) {
			LobbyUI.instance.SetGameStarting();

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

		PlayerPrefs.SetString("last_map", selectedMapName);

		PlayerPrefs.SetString("player_name", playerNameInput.text);
	}
	//don't make networking call until input was finished changing
	private IEnumerator WaitForNameInputExit() {
		waitingChangeNameInput = true;
		while (playerNameInput.isFocused) yield return null;
		waitingChangeNameInput = false;

		AccountDataSyncer.instance.ChangedUsername();
	}
	private void Awake() {
		instance = this;

		if (PlayerPrefs.GetString("turret_name") != "")
			currentTurret = PlayerPrefs.GetString("turret_name");

		if (PlayerPrefs.GetString("hull_name") != "")
			currentHull = PlayerPrefs.GetString("hull_name");

		if (TestingServerLinker.instance != null) {
			Destroy(TestingServerLinker.instance.gameObject);
		}
	}
	private void Start() {
		QualitySettings.SetQualityLevel(4);

		Time.timeScale = 1f;

		for (int i = 0; i < mapSceneNames.Count; i++) {
			mapNamesToDisplayDict.Add(mapSceneNames[i], mapDisplayNames[i]);
		}

		currentGameMode = (GameMode)PlayerPrefs.GetInt("game_mode");

		SetDifficulty(PlayerPrefs.GetInt("game_start_difficulty"));
		SetWave(PlayerPrefs.GetInt("game_start_wave"));
		SetSelectedMap(PlayerPrefs.GetString("last_map"));

		playerNameInput.text = PlayerPrefs.GetString("player_name");

		PlayerTurretChanged();
		PlayerHullChanged();

		GameModeChanged();
	}
	private void Update() {
		string inputText = playerNameInput.text;
		if (PlayerPrefs.GetString("player_name") != inputText) {
			PlayerPrefs.SetString("player_name", inputText);
			if (!waitingChangeNameInput) StartCoroutine(WaitForNameInputExit());
		}
		//if (outdoorsMap.activeInHierarchy != (PlayerPrefs.GetInt("use_outdoor") == 1)) {
		//	outdoorsMap.SetActive(PlayerPrefs.GetInt("use_outdoor") == 1);
		//	garageMap.SetActive(PlayerPrefs.GetInt("use_outdoor") != 1);
		//}

#if UNITY_EDITOR
		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.D))
			ToggleDebug();
#endif
	}

	#endregion
}
