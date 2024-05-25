using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectManager : MonoBehaviour {

	#region Statics & Consts

	public static LevelSelectManager instance;

	#endregion

	#region References

	[SerializeField] private RectTransform soloMapScreen, coopMapScreen, compMapScreen;

	[SerializeField] private GameObject mainPlayerDisplay;
	[SerializeField] private RectTransform levelSelectScreen;

	[SerializeField] private TMP_Text trophiesDisplay, coopWavesDisplay, soloWavesDisplay;

	#endregion

	#region Members



	#endregion

	#region Functions

	public bool GetIsInLevelSelect() { return levelSelectScreen.gameObject.activeInHierarchy; }

	//0 = solo, 1 = coop, 2 = comp
	public void ChooseMode(int mode) {
		switch (mode) {
			case 0:
				soloMapScreen.gameObject.SetActive(true);
				break;
			case 1:
				coopMapScreen.gameObject.SetActive(true);
				break;
			case 2:
				compMapScreen.gameObject.SetActive(true);
				break;
		}
	}
	//NOTE: may eventually want to customize with the ability to remotely swap out map selections
	public void SetMap(string mapName, MenuManager.GameMode mode, int difficulty) {
		//soloMapScreen.gameObject.SetActive(false);
		//coopMapScreen.gameObject.SetActive(false);
		//compMapScreen.gameObject.SetActive(false);

		MenuManager.instance.SetSelectedMap(mapName);
		LobbyUI.instance.MapChanged();
		LobbyUI.instance.GameModeChanged();

		//NOTE: should always be 1, unless changed by debug
		MenuManager.instance.SetWave(1);

		MenuManager.instance.SetDifficulty(difficulty);

		MenuManager.instance.SetGameMode(mode);

		CloseLevelSelect();
	}
	public void OpenLevelSelect() {
		if (LobbyStatsSyncer.instance != null && !LobbyStatsSyncer.instance.Runner.IsSharedModeMasterClient)
			return;

		levelSelectScreen.gameObject.SetActive(true);
		MenuManager.instance.SetMenuScreen(false);
		mainPlayerDisplay.SetActive(false);
	}
	public void CloseLevelSelect() {
		levelSelectScreen.gameObject.SetActive(false);
		MenuManager.instance.SetMenuScreen(true);
		mainPlayerDisplay.SetActive(true);
	}

	private void Awake() {
		instance = this;
	}

	private void Start() {
		trophiesDisplay.text = PersistentDict.GetInt("trophies").ToString();

		int soloWaves = 0;
		int coopWaves = 0;
		foreach (LevelSelectButton b in Resources.FindObjectsOfTypeAll<LevelSelectButton>()) {
			print(b.name);
			if (b.GetMode() == MenuManager.GameMode.Singleplayer) {
				soloWaves += b.GetWaveRecord();
			} else if (b.GetMode() == MenuManager.GameMode.Coop) {
				coopWaves += b.GetWaveRecord();
			}
		}
		soloWavesDisplay.text = $"TOTAL:\n<color={LevelSelectButton.ChooseColorByWave(soloWaves, 999)}>" +
			soloWaves.ToString() + " WAVES";
		coopWavesDisplay.text = $"TOTAL:\n<color={LevelSelectButton.ChooseColorByWave(coopWaves, 999)}>" +
			coopWaves.ToString() + " WAVES";
	}

	private void Update() { }

	#endregion

}
