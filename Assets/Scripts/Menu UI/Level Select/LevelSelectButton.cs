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

	private void Start() {
		if (!displayProg) {
			progDisplayText.gameObject.SetActive(false);
			progDisplayIcon.gameObject.SetActive(false);
		} else {
			int wave = PersistentDict.GetInt("wave_record_" + mapName + "_" + (int)mode);
			progDisplayText.text = $"RECORD:\n<color={ChooseColorByWave(wave)}>WAVE {wave}";
		}
	}
	//TODO: make it better
	private string ChooseColorByWave(int wave) {
		// Define the start and end hue values
		float startHue = 20f / 360f;
		float endHue = 125f / 360f;

		// Interpolate the hue based on the wave number
		float hue = Mathf.Lerp(startHue, endHue, (wave - 1) / 99f);

		// Convert HSV to RGB
		Color color = Color.HSVToRGB(hue, 0.63f, 1f);

		// Convert the color to a hex string
		return "#" + ColorUtility.ToHtmlStringRGB(color);
	}
	public void SelectMap() {
		LevelSelectManager.instance.SetMap(mapName, mode, difficulty);
	}
}
