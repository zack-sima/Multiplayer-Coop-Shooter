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
	public void SetMap(string mapName, bool isSolo, bool isComp, bool isPCP, int difficulty) {
		//soloMapScreen.gameObject.SetActive(false);
		//coopMapScreen.gameObject.SetActive(false);
		//compMapScreen.gameObject.SetActive(false);

		MenuManager.instance.SetSelectedMap(mapName);
		LobbyUI.instance.MapChanged();
		LobbyUI.instance.GameModeChanged();

		//NOTE: should always be 1, unless changed by debug
		MenuManager.instance.SetWave(1);

		MenuManager.instance.SetDifficulty(difficulty);

		MenuManager.instance.SetGameMode(isSolo ? MenuManager.GameMode.Singleplayer :
			(isComp ? (isPCP ? MenuManager.GameMode.PointCap : MenuManager.GameMode.Comp) : MenuManager.GameMode.Coop));

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

	private void Start() { }

	private void Update() { }

	#endregion

}
