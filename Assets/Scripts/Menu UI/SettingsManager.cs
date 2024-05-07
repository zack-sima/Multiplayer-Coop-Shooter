using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour {

	public static SettingsManager instance;

	[SerializeField] private RectTransform settingsScreen;

	[SerializeField] private Image muteAudioImage;
	[SerializeField] private Sprite muteAudioSprite, unMuteAudioSprite;

	[SerializeField] private Image muteMusicImage;
	[SerializeField] private Sprite muteMusicSprite, unMuteMusicSprite;

	[SerializeField] private List<TMP_Text> qualityTexts;

	private int currentQualityIndex = 3;

	public void SetQuality(int quality) {
		QualitySettings.SetQualityLevel(quality);
		PlayerPrefs.SetInt("quality", quality + 1);

		for (int i = 0; i < qualityTexts.Count; i++) {
			if (qualityTexts[i] == null) continue;
			if (i == quality) {
				qualityTexts[i].color = Color.yellow;
			} else {
				qualityTexts[i].color = Color.white;
			}
		}
	}
	public void ToggleMuteAudio() {
		PlayerPrefs.SetInt("mute_audio", PlayerPrefs.GetInt("mute_audio") ^ 1);
		UpdateIcons();
	}
	public void ToggleMuteMusic() {
		PlayerPrefs.SetInt("mute_music", PlayerPrefs.GetInt("mute_music") ^ 1);
		UpdateIcons();
	}
	public void ShowSettings() {
		settingsScreen.gameObject.SetActive(true);
	}
	public void HideSettings() {
		settingsScreen.gameObject.SetActive(false);
	}
	private void UpdateIcons() {
		muteMusicImage.sprite = PlayerPrefs.GetInt("mute_music") == 1 ? muteMusicSprite : unMuteMusicSprite;
		muteAudioImage.sprite = PlayerPrefs.GetInt("mute_audio") == 1 ? muteAudioSprite : unMuteAudioSprite;
	}
	private void Awake() {
		instance = this;
	}
	private void Start() {
		UpdateIcons();

		if (PlayerPrefs.GetInt("quality") == 0) {
			PlayerPrefs.SetInt("quality", currentQualityIndex);
		} else {
			currentQualityIndex = PlayerPrefs.GetInt("quality") - 1;
		}
		SetQuality(currentQualityIndex);
	}
}
