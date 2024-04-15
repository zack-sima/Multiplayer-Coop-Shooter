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

	//disable this if not in a lobby
	[SerializeField] private RectTransform lobbyButtons;

	//TODO: replace this with more fancy stuff (currently just a single string that displays everything)
	[SerializeField] private TMP_Text lobbyTextDisplay;

	[SerializeField] private TMP_InputField playerNameInput, waveInput;

	[SerializeField] private TMP_Dropdown mapDropdown;

	#endregion

	#region Members

	//TODO: find a better way to access lobby players than putting them in a UI script!
	private List<LobbyPlayer> lobbyPlayers = new();
	public void AddLobbyPlayer(LobbyPlayer p) { if (!lobbyPlayers.Contains(p)) lobbyPlayers.Add(p); }
	public void RemoveLobbyPlayer(LobbyPlayer p) { if (lobbyPlayers.Contains(p)) lobbyPlayers.Remove(p); }
	public List<LobbyPlayer> GetLobbyPlayers() { return lobbyPlayers; }

	#endregion

	#region Functions

	//sends current player information (master client) to LobbyStatsSyncer script
	public void InitLocalSync() {
		MapDropdownChanged();
		PlayerNameInputChanged();
	}
	public void WaveInputChanged() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;

		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) {
			if (int.TryParse(waveInput.text, out int wave))
				LobbyStatsSyncer.instance.SetStartingWave(wave);
		}
	}
	public void MapDropdownChanged() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;

		if (LobbyPlayer.playerInstance.Runner.IsSharedModeMasterClient) {
			LobbyStatsSyncer.instance.SetMap(mapDropdown.options[mapDropdown.value].text);
		}
	}
	public void PlayerNameInputChanged() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;

		if (playerNameInput.text != "") {
			LobbyPlayer.playerInstance.SetPlayerName(playerNameInput.text);
		} else {
			LobbyPlayer.playerInstance.SetPlayerName("Player");
		}
	}
	public void SetLobbyUIActive(bool isActive) {
		if (!isActive) {
			lobbyTextDisplay.text = "Not currently in a lobby";
		} else {
			lobbyTextDisplay.text = "Loading lobby...";
		}
		lobbyButtons.gameObject.SetActive(isActive);
	}
	//TODO: modify the lobby controls script to give this script actual information instead of one string
	public void SetLobbyText(string text) {
		lobbyTextDisplay.text = text;
	}
	//NOTE: this calls the singleton lobby player assuming the player exists
	public void ToggleIsReady() {
		if (LobbyPlayer.playerInstance == null || LobbyStatsSyncer.instance == null) return;

		LobbyPlayer.playerInstance.ToggleIsReady();
	}

	private void Awake() {
		instance = this;
	}
	private void Start() {
		SetLobbyUIActive(false);
	}

	#endregion

}
