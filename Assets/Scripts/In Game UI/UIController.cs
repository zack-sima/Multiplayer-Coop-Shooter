using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Abilities;

#if UNITY_IOS || UNITY_ANDROID
using CandyCoded.HapticFeedback;
#endif

using TMPro;

public class UIController : MonoBehaviour {

	#region Statics & Consts

	public static UIController instance;
	private const bool OVERRIDE_MOBILE = false;

	public static bool GetIsMobile() {
#if UNITY_EDITOR
		if (OVERRIDE_MOBILE) return true;
#endif
		if (Application.isEditor) return false;
		return Application.isMobilePlatform;
	}

	#endregion

	#region References

	[SerializeField] private TMP_Text respawnTimerText, gameOverTimerText, moneyText, waveText;

	[SerializeField] private RectTransform respawnUI, gameOverUI, mobileUI, pcUI, loadingUI, optionsUI;

	[SerializeField] private List<GameObject> mobileAbilityButtons;

	[SerializeField] private List<GameObject> pcAbilityButtons;

	[SerializeField] private AudioSource soundtrack;

	#region Ability UI

	/// <summary>
	/// Turns off all ability Buttons for a clean slate.
	/// </summary>
	private void InitAbilityButtons() {
		//turns everything off.
		foreach (GameObject g in mobileAbilityButtons) {
			if (g.activeInHierarchy) g.SetActive(false);
		}
		foreach (GameObject g in pcAbilityButtons) {
			if (g.activeInHierarchy) g.SetActive(false);
		}
	}

	/// <summary>
	/// Called by each ability to update certain stuff on a callback basis.
	/// </summary>
	public GameObject GetAbilityButton(int index) {
		if (index! < (GetIsMobile() ? mobileAbilityButtons.Count : pcAbilityButtons.Count)) return null;
		return GetIsMobile() ? mobileAbilityButtons[index] : pcAbilityButtons[index];
	}

	/// <summary>
	/// Called by UI Buttons. (Configured in unity editor on a per button basis)
	/// </summary>
	public void AbilityButtonCallback(int buttonIndex) {
		Debug.Log("Button Called " + buttonIndex);
		PlayerInfo.instance.PushAbilityActivation(buttonIndex);
	}

	/// <summary>
	/// Called when ability list gets updated/changed.
	/// </summary>
	public void AbilitiesUpdated() {
		List<GameObject> buttons = GetIsMobile() ? mobileAbilityButtons : pcAbilityButtons;
		InitAbilityButtons(); // turn off all the buttons.
							  //Debug.Log("UI controller is reached" + PlayerInfo.instance.GetAbilityList().Count + " " + buttons.Count);
		for (int i = 0; i < PlayerInfo.instance.GetAbilityList().Count && i < buttons.Count; i++) {
			//PlayerInfo.instance.GetAbilityList()[i]. /* TODO: Callback for icon, color, shape, etc. */

			//Callback for updating the UI button fill amount.
			if (PlayerInfo.instance.GetAbilityList()[i].Item1 is IButtonRechargable) {
				//callback for the image.
				GameObject g = buttons[i].FindChild("OutlineProgress");
				if (g != null) {
					Image image = g.GetComponent<Image>();
					if (image != null)
						((IButtonRechargable)PlayerInfo.instance.GetAbilityList()[i].Item1).SetButtonOutlineProgressImage(image);
				}
			}
			//Debug.Log("Button is activated");
			buttons[i].SetActive(true);
		}
	}


	// private List<AbilityButton> abilityButtons;

	// private struct AbilityButton {
	// 	public GameObject realButton;
	// 	public IAbility ability;
	// }


	// /// <summary>
	// /// Maps each button to a ability and toggles visibility. Is a CALLBACK from Ability.Manager.AbilityUIManagerExtensions
	// /// </summary>
	// public void MapAbilityButtons() {
	// 	abilityButtons.Clear();

	// 	//Deactivate all the buttons.
	// 	foreach(GameObject g in GetIsMobile() ? mobileAbilityButtons : pcAbilityButtons) {
	// 		if (g.activeInHierarchy) g.SetActive(false);
	// 	} 
	// 	int i = 0;
	// 	foreach((IAbility a, bool b) in PlayerInfo.instance.GetAbilityList()) {
	// 		if ((GetIsMobile() ? mobileAbilityButtons.Count : pcAbilityButtons.Count) !< i) break;

	// 		AbilityButton button = new AbilityButton {
	// 			realButton = GetIsMobile() ? mobileAbilityButtons[i] : pcAbilityButtons[i],
	// 			ability = a
	// 		};

	// 		button.realButton.SetActive(true);
	// 		//TODO: Update icon.
	// 		abilityButtons.Add(button);
	// 		i++;
	// 	}
	// }

	#endregion

	#endregion

	#region Members

	//if just closed options, don't allow fire
	private float closedOptionsTimestamp = 0f;

	#endregion

	#region Functions

	/// <summary>
	/// Haptic feedback
	/// </summary>
	/// <param name="nudgeMode">0 for light, 1 for medium, 2 for heavy</param>
	public static void NudgePhone(int nudgeMode) {
#if UNITY_IOS || UNITY_ANDROID
		if (nudgeMode == 0) {
			HapticFeedback.LightFeedback();
		} else if (nudgeMode == 1) {
			HapticFeedback.MediumFeedback();
		} else {
			HapticFeedback.HeavyFeedback();
		}
#endif
	}
	public bool InOptions() {
		if (Time.time - closedOptionsTimestamp < 0.07f) return true;
		return optionsUI.gameObject.activeInHierarchy || UpgradesCatalog.instance.UpgradeUIOn();
	}
	public void ResumeGame() {
		closedOptionsTimestamp = Time.time;
		optionsUI.gameObject.SetActive(false);
		Time.timeScale = 1f;
	}
	public void ToggleOptions() {
		closedOptionsTimestamp = Time.time;
		optionsUI.gameObject.SetActive(!optionsUI.gameObject.activeInHierarchy);

		if (optionsUI.gameObject.activeInHierarchy) PauseGame();
		else ResumeGame();
	}
	public void LeaveGame() {
		Time.timeScale = 1f;

		SetLoadingTrue();

		if (ServerLinker.instance == null) {
			UnityEngine.SceneManagement.SceneManager.LoadScene(0);
		} else {
			ServerLinker.instance.StopGame();
		}
	}
	public void SetMobileUIEnabled(bool enabled) {
		mobileUI.gameObject.SetActive(enabled);
	}
	public void SetPCUIEnabled(bool enabled) {
		pcUI.gameObject.SetActive(enabled);
	}
	public void SetRespawnTimerText(string text) {
		respawnTimerText.text = text;
	}
	public void SetGameOverTimerText(string text) {
		gameOverTimerText.text = text;
	}
	public void SetRespawnUIEnabled(bool enabled) {
		respawnUI.gameObject.SetActive(enabled);
	}
	public void SetGameOverUIEnabled(bool enabled) {
		gameOverUI.gameObject.SetActive(enabled);
	}
	public void SetPointCapScores(int[] scores) {
		if (!PlayerInfo.GetIsPointCap()) return;

		waveText.text = $"<color=#5555FF>{scores[0]}<color=white> vs <color=#FF5555>{scores[1]}";
	}
	public void SetTeamScores(int[] scores) {
		if (!PlayerInfo.GetIsPVP()) return;

		waveText.text = $"<color=#5555FF>{scores[0]}<color=white> vs <color=#FF5555>{scores[1]}";
	}
	public void SetMoneyText(int money) {
		moneyText.text = $"${money}";
	}
	public void SetWaveText(int wave) {
		if (PlayerInfo.GetIsPVP()) return;

		waveText.text = $"Wave {wave}";

		if (EnemySpawner.instance == null) return;

		int initTimer = Mathf.CeilToInt(EnemySpawner.instance.GetInitTimer());
		int timer = Mathf.CeilToInt(EnemySpawner.instance.GetSpawnTimer());

		if (initTimer > 0) {
			waveText.text = $"Wave {wave} ({initTimer})";
		} else if (timer != 0 && timer < 10) waveText.text = $"Wave {wave} ({timer})";
	}
	public void SetLoadingTrue() { loadingUI.gameObject.SetActive(true); }
	private void Awake() {
		instance = this;
		SetLoadingTrue();
		StartCoroutine(WaitStartPlayer());

		if (PlayerPrefs.GetInt("mute_music") == 0) soundtrack.Play();
	}
	private void Update() {
		if (EnemySpawner.instance != null) {
			SetWaveText(GameStatsSyncer.instance.GetWave() + 1);
		}
	}
	public void PauseGame() {
		if (NetworkedEntity.playerInstance != null &&
			!NetworkedEntity.playerInstance.Runner.IsSinglePlayer) return;
		Time.timeScale = 0.000000000001f;
	}
	private IEnumerator WaitStartPlayer() {
		while (NetworkedEntity.playerInstance == null) yield return null;
		loadingUI.gameObject.SetActive(false);
	}

	#endregion
}
