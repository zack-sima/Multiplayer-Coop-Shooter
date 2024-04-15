using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour {

	#region Statics

	public static MenuManager instance;

	#endregion

	#region References

	[SerializeField] private TMP_InputField roomInput, waveInput;

	//map
	[SerializeField] private TMP_Dropdown mapDropdown;
	[SerializeField] private List<int> mapDropdownSceneIndices;

	//turret
	[SerializeField] private TMP_Dropdown turretDropdown;

	#endregion

	#region Functions

	public void StartLobby() {
		//NOTE: lobbies directly use room_id; games have _g appended to it to distinguish it from lobby rooms
		PlayerPrefs.SetString("room_id", roomInput.text);
		ServerLinker.instance.StartLobby(roomInput.text);
	}
	//NOTE: only call this from the lobby!
	//TODO for UI: move InitGame stuff to a singleton manager that is called when lobby decides to start game
	public void StartShared() {
		InitGame();

		ServerLinker.instance.StopLobby();

		//saved lobby room ID + "_g" goes to correct game room
		ServerLinker.instance.StartShared(mapDropdownSceneIndices[mapDropdown.value],
			PlayerPrefs.GetString("room_id") + "_g");
	}
	public void StartSingle() {
		InitGame();
		ServerLinker.instance.StartSinglePlayer(mapDropdownSceneIndices[mapDropdown.value]);
	}
	private void InitGame() {
		PlayerPrefs.SetInt("turret_index", turretDropdown.value);
		PlayerPrefs.SetString("turret_name", turretDropdown.options[turretDropdown.value].text);

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
	}

	#endregion
}
