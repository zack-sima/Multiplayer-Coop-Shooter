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
		if (OVERRIDE_MOBILE) return true;
		if (Application.isEditor) return false;
		return Application.isMobilePlatform;
	}

	#endregion

	#region References

	[SerializeField]
	private TMP_Text respawnTimerText, gameOverTimerText, scoreText;

	[SerializeField]
	private RectTransform respawnUI, gameOverUI, mobileUI;

	#endregion

	#region Functions

	public void SetMobileUIEnabled(bool enabled) {
		mobileUI.gameObject.SetActive(enabled);
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
	}

	#endregion
}
