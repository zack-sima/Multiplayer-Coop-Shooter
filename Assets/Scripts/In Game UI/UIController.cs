using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour {

	#region Statics & Consts

	public static UIController instance;
	private const bool OVERRIDE_MOBILE = true;

	public static bool GetIsMobile() {
#if UNITY_EDITOR
		if (OVERRIDE_MOBILE) return false;
#endif
		if (Application.isEditor) return false;
		return Application.isMobilePlatform;
	}

	#endregion

	#region References

	[SerializeField]
	private TMP_Text respawnTimerText, gameOverTimerText, scoreText;

	[SerializeField]
	private RectTransform respawnUI, gameOverUI, mobileUI, pcUI, loadingUI, optionsUI;

	#endregion

	#region Members

	//if just closed options, don't allow fire
	private float closedOptionsTimestamp = 0f;

	#endregion

	#region Functions

	public bool InOptions() {
		if (Time.time - closedOptionsTimestamp < 0.07f) return true;
		return optionsUI.gameObject.activeInHierarchy;
	}
	public void ResumeGame() {
		closedOptionsTimestamp = Time.time;
		optionsUI.gameObject.SetActive(false);
	}
	public void ToggleOptions() {
		closedOptionsTimestamp = Time.time;
		optionsUI.gameObject.SetActive(!optionsUI.gameObject.activeInHierarchy);
	}
	public void LeaveGame() {
		ServerLinker.instance.StopGame();
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
	public void SetScoreAndWaveText(int score, int wave) {
		scoreText.text = $"<color=#dddddd>Score: <color=#aaeeaa>{score}</color>\n" +
			$"<color=#dddddd>Wave: <color=#eeeeaa>{wave}";
	}
	private void Awake() {
		instance = this;
		loadingUI.gameObject.SetActive(true);
		StartCoroutine(WaitStartPlayer());
	}
	private IEnumerator WaitStartPlayer() {
		while (NetworkedEntity.playerInstance == null) yield return null;
		loadingUI.gameObject.SetActive(false);
	}

	#endregion
}
