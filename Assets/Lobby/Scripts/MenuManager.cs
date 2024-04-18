using System.Collections;
using System.Collections.Generic;
using Lobby;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour {

	#region Statics

	public static MenuManager instance;

	#endregion

	#region References

	[SerializeField] private TMP_InputField roomInput, waveInput, playerNameInput;
	public TMP_InputField GetRoomInput() { return roomInput; }
	public TMP_InputField GetWaveInput() { return waveInput; }
	public TMP_InputField GetPlayerNameInput() { return playerNameInput; }

	//map select
	[SerializeField] private TMP_Dropdown mapDropdown;
	public TMP_Dropdown GetMapDropdown() { return mapDropdown; }

	//TODO: move this to a separate map scene manager that tracks each map's properties, name, scene index, etc
	[SerializeField] private List<int> mapDropdownSceneIndices;

	//turret
	[SerializeField] private TMP_Dropdown turretDropdown;

	#endregion

	#region Functions

	public void StartLobby() {
		if (roomInput.text == "") return;

		//NOTE: lobbies directly use room_id; games have _g appended to it to distinguish it from lobby rooms
		PlayerPrefs.SetString("room_id", roomInput.text);
		ServerLinker.instance.StartLobby(roomInput.text);

		//NOTE: this calls the lobby UI loading screen
		LobbyUI.instance.SetLobbyLoading(true);
	}
	
	public void QuitLobby() {
		// LobbyEventsHandler.RaisePlayerUpdate(LobbyPlayer.playerInstance);
		ServerLinker.instance.StopLobby();
		LobbyUI.instance.SetLobbyUIActive(false);

		Destroy(ServerLinker.instance.gameObject);
		UnityEngine.SceneManagement.SceneManager.LoadScene(ServerLinker.LOBBY_SCENE);
	}
	
	//NOTE: only call this from the lobby!
	//TODO for UI: move InitGame stuff to a singleton manager that is called when lobby decides to start game
	//TODO: NOTE: all players' PlayerPrefs needs to be updated with the right map & wave settings from lobby
	//because the lobby host won't necessarily be the master client
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
		if (LobbyPlayer.playerInstance || LobbyStatsSyncer.instance) return;
		
		InitGame();
		ServerLinker.instance.StartSinglePlayer(mapDropdownSceneIndices[mapDropdown.value]);
	}
	private void InitGame() {
		PlayerPrefs.SetInt("turret_index", turretDropdown.value);
		PlayerPrefs.SetString("turret_name", turretDropdown.options[turretDropdown.value].text);
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
	void Start() {
		Application.targetFrameRate = 90;

		turretDropdown.value = PlayerPrefs.GetInt("turret_index");
		playerNameInput.text = PlayerPrefs.GetString("player_name");

		if (PlayerPrefs.GetInt("debug_starting_wave") > 0)
			waveInput.text = PlayerPrefs.GetInt("debug_starting_wave").ToString();
	}

	#endregion
}
