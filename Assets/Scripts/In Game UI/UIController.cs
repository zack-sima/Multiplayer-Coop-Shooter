using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour {

	#region Statics

	public static UIController instance;

	#endregion

	#region References

	[SerializeField]
	private TMP_Text respawnTimerText, gameOverTimerText, scoreText;

	[SerializeField]
	private RectTransform respawnUI, gameOverUI;

	#endregion

	#region Functions

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
