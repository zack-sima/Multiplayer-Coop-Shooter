using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectButton : MonoBehaviour {

	[SerializeField] private string mapName;
	[SerializeField] private MenuManager.GameMode mode;
	[SerializeField] private int difficulty;

	//TODO: actually show trophies/completion, etc
	[SerializeField] private bool displayProg;
	[SerializeField] private TMP_Text progDisplayText;
	[SerializeField] private RectTransform progDisplayIcon;

	public MenuManager.GameMode GetMode() {
		return mode;
	}
	public int GetWaveRecord() {
		return PersistentDict.GetInt("wave_record_" + mapName + "_" + (int)mode);
	}

	private void Start() {
		if (!displayProg) {
			progDisplayText.gameObject.SetActive(false);
			progDisplayIcon.gameObject.SetActive(false);
		} else {
			int wave = GetWaveRecord();
			progDisplayText.text = $"RECORD:\n<color={ChooseColorByWave(wave)}>WAVE {wave}";
		}
	}
	//TODO: make it better
	public static string ChooseColorByWave(int wave, int maxWaves = 99) {
		// Define the start and end hue values
		float startHue = 20f / 360f;
		float endHue = 125f / 360f;

		// Interpolate the hue based on the wave number
		float hue = Mathf.Lerp(startHue, endHue, (wave - 1) / (float)maxWaves);

		// Convert HSV to RGB
		Color color = Color.HSVToRGB(hue, 0.63f, 1f);

		// Convert the color to a hex string
		return "#" + ColorUtility.ToHtmlStringRGB(color);
	}
	public void SelectMap() {
		LevelSelectManager.instance.SetMap(mapName, mode, difficulty);
	}
}
