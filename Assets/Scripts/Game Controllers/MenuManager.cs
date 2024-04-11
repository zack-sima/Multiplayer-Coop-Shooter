using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour {

	#region References

	[SerializeField] private TMP_InputField roomInput;

	//map
	[SerializeField] private TMP_Dropdown mapDropdown;
	[SerializeField] private List<int> mapDropdownSceneIndices;

	//turret
	[SerializeField] private TMP_Dropdown turretDropdown;
	[SerializeField] private List<string> turretDropdownNames;

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
		PlayerPrefs.SetString("turret_name", turretDropdownNames[turretDropdown.value]);
	}
	void Start() {
		Application.targetFrameRate = 90;
	}

	#endregion
}
