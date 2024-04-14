using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour {

	#region References

	[SerializeField] private TMP_InputField roomInput, waveInput;

	//map
	[SerializeField] private TMP_Dropdown mapDropdown;
	[SerializeField] private List<int> mapDropdownSceneIndices;

	//turret
	[SerializeField] private TMP_Dropdown turretDropdown;

	#endregion

	#region Functions

	public void StartShared() {
		InitGame();
		ServerLinker.instance.StartShared(mapDropdownSceneIndices[mapDropdown.value], roomInput.text);
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
	void Start() {
		Application.targetFrameRate = 90;

		turretDropdown.value = PlayerPrefs.GetInt("turret_index");
	}

	#endregion
}
