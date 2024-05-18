using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class SettingsManager : MonoBehaviour {

	public static SettingsManager instance;

	public static void SetFPS(int frameRateSettings) {
		switch (frameRateSettings) {
			case -2:
				Application.targetFrameRate = 30;
				break;
			case -1:
				Application.targetFrameRate = 45;
				break;
			case 0:
				Application.targetFrameRate = 60;
				break;
			case 1:
				Application.targetFrameRate = 90;
				break;
			case 2:
				Application.targetFrameRate = 120;
				break;
		}
	}

	[SerializeField] private RectTransform settingsScreen;

	[SerializeField] private Image muteAudioImage;
	[SerializeField] private Sprite muteAudioSprite, unMuteAudioSprite;

	[SerializeField] private Image muteMusicImage;
	[SerializeField] private Sprite muteMusicSprite, unMuteMusicSprite;

	[SerializeField] private List<TMP_Text> qualityTexts, fpsTexts, lightingTexts;

	private int currentQualityIndex = 2;
	private int currentLightingIndex = 0;
	private int currentFPSIndex = 0;

	public void ToggleMap() {
		PlayerPrefs.SetInt("use_outdoor", PlayerPrefs.GetInt("use_outdoor") ^ 1);
	}
	public void SetQuality(int quality) {
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
	public void SetLighting(int lighting) {
		PlayerPrefs.SetInt("light_brightness", lighting);

		for (int i = 0; i < lightingTexts.Count; i++) {
			if (lightingTexts[i] == null) continue;
			if (i == lighting + 1) {
				lightingTexts[i].color = Color.yellow;
			} else {
				lightingTexts[i].color = Color.white;
			}
		}
	}
	public void SetFramerate(int framerate) {
		PlayerPrefs.SetInt("fps", framerate);
		SetFPS(framerate);

		for (int i = 0; i < fpsTexts.Count; i++) {
			if (fpsTexts[i] == null) continue;
			if (i == framerate + 2) {
				fpsTexts[i].color = Color.yellow;
			} else {
				fpsTexts[i].color = Color.white;
			}
		}
	}
	public void ToggleDebugMenu() {
		DebugUIManager.instance.ToggleDebugMenu();
		HideSettings();
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

		currentLightingIndex = PlayerPrefs.GetInt("light_brightness");
		currentFPSIndex = PlayerPrefs.GetInt("fps");

		SetFramerate(currentFPSIndex);
		SetLighting(currentLightingIndex);
		SetQuality(currentQualityIndex);
	}
}
