using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Abilities;
using Handlers;


#if UNITY_IOS || UNITY_ANDROID
using CandyCoded.HapticFeedback;
#endif

using TMPro;

public class UIController : MonoBehaviour {

	#region Nested

	[System.Serializable]
	public class IconBundle {
		public string id; // ID is found in the Active CSV file or as nameof(CSVId.---)
		public Sprite icon;
		public Color color;
	}

	#endregion

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
	[Header("UI Elements")]

	[SerializeField] private TMP_Text respawnTimerText;
	[SerializeField] private TMP_Text gameOverTimerText, moneyText, waveText;
	[SerializeField] private RectTransform respawnUI, gameOverUI, mobileUI, pcUI, loadingUI, optionsUI;

	[Header("Abilities")]
	[SerializeField] private List<GameObject> mobileAbilityButtons;
	[SerializeField] private List<GameObject> pcAbilityButtons;
	[SerializeField] private Sprite defaultAbilityButtonIcon;
	[SerializeField] private List<IconBundle> abilityButtonIcons; //TODO: Name this better perhaps?

	[Header("Misc")]
	[SerializeField] private AudioSource soundtrack;
	[SerializeField] private GameObject debugUIPrefab;

	#endregion

	#region Members

	//if just closed options, don't allow fire
	private float closedOptionsTimestamp = 0f;
	private bool hasInitButtons = false;

	#endregion

	#region Functions

	public Sprite GetAbilityIcon(string id) {
		foreach (IconBundle i in abilityButtonIcons) {
			if (i.id == id) return i.icon;
		}
		return null;
	}
	public Color GetAbilityColor(string id) {
		foreach (IconBundle i in abilityButtonIcons) {
			if (i.id == id) return i.color;
		}
		return Color.white;
	}

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
		int i = 0;
		foreach ((IAbility a, bool b) in NetworkedEntity.playerInstance.GetAbilityList()) { // init the button icons.
			if (a is IActivatable) {
				if (i < buttons.Count) {
					buttons[i].SetActive(true);
					GameObject o = buttons[i].transform.GetChild(1).gameObject;
					GameObject g = buttons[i].transform.GetChild(2).gameObject;

					g.GetComponent<Image>().sprite = GetAbilityIcon(a.GetId());
					g.GetComponent<Image>().color = GetAbilityColor(a.GetId());
					o.GetComponent<Image>().color = GetAbilityColor(a.GetId());

					++i;
				} else break;
			}
		}
		hasInitButtons = true;
	}

	/// <summary>
	/// Updates the cooldowns on the ability buttons.
	/// </summary>
	private void UpdateAbilityButtons() { // TODO: Implement this in a better way zack??
		int index = 0;
		List<GameObject> buttons = GetIsMobile() ? mobileAbilityButtons : pcAbilityButtons;
		foreach ((IAbility i, bool b) in NetworkedEntity.playerInstance.GetAbilityList()) {
			if (i is ICooldownable a) {
				if (index < buttons.Count) {
					float percentage = a.GetCooldownPercentage();
					GameObject g = buttons[index].FindChild("OutlineProgress");
					if (g != null) {
						Image image = g.GetComponent<Image>();
						if (image != null) {
							image.fillAmount = percentage;
						}
					}
				} else return;
				++index;
			}

		}
	}

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
		if (hasInitButtons) UpdateAbilityButtons();
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
