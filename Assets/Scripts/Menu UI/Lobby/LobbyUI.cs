using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script that just deals with lobby UI. TODO: actually integrate with good visuals, etc
/// </summary>

public class LobbyUI : MonoBehaviour {

	#region Statics & Consts

	public static LobbyUI instance;

	public static string GetPlayerName() {
		string name = PlayerPrefs.GetString("player_name");
		if (name == "") name = "Player";
		return name;
	}

	public const int MAX_PLAYERS = 5;

	#endregion

	#region References

	//////////// NEW ///////////

	//player scroller
	[SerializeField] private InvisibleScroller playerScroller;

	[SerializeField] private TMP_Text lobbyIdText;
	[SerializeField] private Button leaveLobbyButton;

	//3d player models
	[SerializeField] private List<LobbyPlayerDisplayer> playerDisplayers;

	//lobby failed UI
	[SerializeField] private RectTransform lobbyJoinFailedScreen, lobbyJoinFullScreen;

	////////////////////////////

	//NOTE: this is just a debug that displays everything right now
	[SerializeField] private TMP_Text lobbyTextDisplay;

	[SerializeField] private RectTransform lobbyLoadingUI, gameStartingUI;
	public bool GetLobbyOrGameLoading() {
		return lobbyLoadingUI.gameObject.activeInHierarchy || gameStartingUI.gameObject.activeInHierarchy;
	}

	#endregion

	#region Members

	private readonly List<LobbyPlayer> lobbyPlayers = new();
	public void AddLobbyPlayer(LobbyPlayer p) { if (!lobbyPlayers.Contains(p)) lobbyPlayers.Add(p); }
	public void RemoveLobbyPlayer(LobbyPlayer p) { if (lobbyPlayers.Contains(p)) lobbyPlayers.Remove(p); }
	public List<LobbyPlayer> GetLobbyPlayers() { return lobbyPlayers; }

	//player turret rotation (locally saved to PlayerPrefs too)
	private float playerTurretRotation = -20f;
	private float playerHullRotation = 20f;

	private float randSeed = 0f;

	#endregion

	#region Functions

	public void DisableLobbyFailedScreen() {
		lobbyJoinFailedScreen.gameObject.SetActive(false);
		lobbyJoinFullScreen.gameObject.SetActive(false);
	}

	//sends current player information (master client) to LobbyStatsSyncer script
	private IEnumerator WaitInitData() {
		yield return new WaitForSeconds(0.2f);

		MapChanged();
		GameModeChanged();

		MenuManager.instance.PlayerTurretChanged();
		MenuManager.instance.PlayerHullChanged();

		PlayerNameInputChanged();
	}
	public void WaveChanged(int newStartingWave) {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;

		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) {
			LobbyStatsSyncer.instance.SetStartingWave(newStartingWave);
		}
	}
	public void DifficultyChanged(int newDifficulty) {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;

		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) {
			LobbyStatsSyncer.instance.SetDifficulty(newDifficulty);
		}
	}
	public void MapChanged() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;

		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) {
			LobbyStatsSyncer.instance.SetMap(MenuManager.instance.GetSelectedMap());
		}
	}
	public void GameModeChanged() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;

		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) {
			LobbyStatsSyncer.instance.SetGameMode((int)MenuManager.instance.GetCurrentGameMode());
		}
	}
	public void PlayerNameInputChanged() {
		string text = MenuManager.instance.GetPlayerNameInput().text;
		if (text == "") text = "Player";

		playerDisplayers[0].SetPlayerNameText(text);

		//online lobby stuff
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;

		LobbyPlayer.playerInstance.SetPlayerName(text);

		string isReadyText = LobbyPlayer.playerInstance.GetIsReady() ? "(Ready)" : "(Not Ready)";
		playerDisplayers[0].SetPlayerNameText(text + "\n" + isReadyText);
	}
	//if the host changes the wave, change it on client too (still, only for testing)
	public void SetClientWave() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;
		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) return;

		MenuManager.instance.SetWave(LobbyStatsSyncer.instance.GetStartingWave());
	}
	public void SetClientDifficulty() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;
		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) return;

		MenuManager.instance.SetDifficulty(LobbyStatsSyncer.instance.GetDifficulty());
	}
	public void SetClientMap() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;
		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) return;

		MenuManager.instance.SetSelectedMap(LobbyStatsSyncer.instance.GetMap());
		SetClientGameMode();
	}
	public void SetClientGameMode() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;
		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) return;
		if ((MenuManager.GameMode)LobbyStatsSyncer.instance.GetGameMode() == MenuManager.GameMode.Singleplayer) return;

		MenuManager.instance.SetGameMode((MenuManager.GameMode)LobbyStatsSyncer.instance.GetGameMode());
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
		//NOTE: removed 0 & O's because they could be confusing to type, still has 34^6 combinations
		string chars = "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789";
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
		UnityEngine.SceneManagement.SceneManager.LoadScene(0);
	}
	//NOTE: this calls the singleton lobby player assuming the player exists
	public void ToggleIsReady() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;

		LobbyPlayer.playerInstance.ToggleIsReady();

		PlayerNameInputChanged();
	}
	public void SetLobbyLoading(bool loading) {
		lobbyLoadingUI.gameObject.SetActive(loading);
	}
	public void SetGameStarting() {
		gameStartingUI.gameObject.SetActive(true);
	}
	//assumed PlayerPrefs has been set elsewhere already
	public void SetPlayerTurret(bool isTemporary = false) {
		string newTurret = PlayerPrefs.GetString(isTemporary ? "temp_turret_name" : "turret_name");

		playerDisplayers[0].Initialize();
		playerDisplayers[0].SetTurret(newTurret);

		if (!isTemporary) {
			//call local player if possible to update network
			if (!ServerLinker.instance.GetIsInLobby() || LobbyPlayer.playerInstance == null ||
				LobbyStatsSyncer.instance == null) return;
			LobbyPlayer.playerInstance.SetTurretName(newTurret);
		}
	}
	public void SetPlayerHull(bool isTemporary = false) {
		string newHull = PlayerPrefs.GetString(isTemporary ? "temp_hull_name" : "hull_name");

		playerDisplayers[0].Initialize();
		playerDisplayers[0].SetHull(newHull);

		if (!isTemporary) {
			//call local player if possible to update network
			if (!ServerLinker.instance.GetIsInLobby() || LobbyPlayer.playerInstance == null ||
				LobbyStatsSyncer.instance == null) return;
			LobbyPlayer.playerInstance.SetHullName(newHull);
		}
	}
	private void Awake() {
		instance = this;

		if (PlayerPrefs.GetString("turret_name") == "") {
			string newTurret = "Autocannon";
			PlayerPrefs.SetString("turret_name", newTurret);
		}
		if (PlayerPrefs.GetString("hull_name") == "") {
			string newHull = "Spider";
			PlayerPrefs.SetString("hull_name", newHull);
		}
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

		playerDisplayers[0].Initialize();

		PlayerNameInputChanged();
		SetPlayerHull();
		SetPlayerTurret();

		randSeed = Random.Range(1f, 10f);
	}
	private void Update() {
		int index = 1;
		if (ServerLinker.instance.GetIsInLobby()) {
			foreach (LobbyPlayer p in lobbyPlayers) {
				//represent self locally (always as central player & never disabled)
				//if somehow there's >5 players don't display extra (they should be kicking themselves out anyway)
				if (p == null || p == LobbyPlayer.playerInstance || index >= MAX_PLAYERS) continue;

				try {
					//update the players' hulls & turrets
					if (!playerDisplayers[index].gameObject.activeInHierarchy) {
						if (!GarageManager.instance.GetIsInGarage() && !LevelSelectManager.instance.GetIsInLevelSelect())
							playerDisplayers[index].gameObject.SetActive(true);
					}
					if ((GarageManager.instance.GetIsInGarage() || LevelSelectManager.instance.GetIsInLevelSelect()) &&
						playerDisplayers[index].gameObject.activeInHierarchy) {
						playerDisplayers[index].gameObject.SetActive(false);
						continue;
					}

					playerDisplayers[index].Initialize();
					playerDisplayers[index].SetHull(p.GetHullName());
					playerDisplayers[index].SetTurret(p.GetTurretName());
					playerDisplayers[index].SetHullRotation(p.GetHullRotation());
					playerDisplayers[index].SetTurretRotation(p.GetHullRotation() - 20f + p.GetTurretRotation());

					string isReadyText = p.GetIsReady() ? "(Ready)" : "(Not Ready)";

					playerDisplayers[index].SetPlayerNameText(p.GetPlayerName() + "\n" + isReadyText);
					playerDisplayers[index].SetHostIcon(p.GetIsMasterClient());
				} catch (System.Exception e) {
					Debug.LogWarning(e);
				} finally {
					index++;
				}
			}
			//local player
			playerDisplayers[0].SetHostIcon(LobbyPlayer.playerInstance.GetIsMasterClient());

			LobbyPlayer.playerInstance.SetHullRotation(playerHullRotation);
			LobbyPlayer.playerInstance.SetTurretRotation(playerTurretRotation);
		} else {
			playerDisplayers[0].SetHostIcon(false);
		}

		//local player stuff, regardless of whether in lobby
		playerDisplayers[0].SetHullRotation(playerHullRotation);
		playerDisplayers[0].SetTurretRotation(playerHullRotation - 20f + playerTurretRotation);

		playerHullRotation -= playerScroller.GetMouseDelta().x / 3.5f;

		//bobbing effect
		float time = Time.time + randSeed;
		float frequency = 0.25f;
		float amplitude = 15f;

		// Combining sine and cosine to create a sharper effect
		float bobbingEffect = Mathf.Sin(time * frequency) * amplitude + Mathf.Cos(time * frequency * 0.5f) * (amplitude * 0.5f);

		// Applying the bobbing effect to the player turret rotation
		playerTurretRotation = bobbingEffect;


		//disable other players
		for (; index < MAX_PLAYERS; index++) {
			if (playerDisplayers[index].gameObject.activeInHierarchy)
				playerDisplayers[index].gameObject.SetActive(false);
		}
	}

	#endregion

}
