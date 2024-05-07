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

	[SerializeField] private GameObject mainPlayerDisplay;
	[SerializeField] private RectTransform levelSelectScreen;
	[SerializeField] private ScrollRect levelScroller;

	[SerializeField] private List<TMP_Text> modeTexts;
	[SerializeField] private List<float> modeNormalizedThresholds;

	#endregion

	#region Members

	#endregion

	#region Functions

	public bool GetIsInLevelSelect() { return levelSelectScreen.gameObject.activeInHierarchy; }

	//TODO: eventually, procedurally generate tabs/make tabs be generate-able on remote?
	public void SetMap(string mapName, bool isSolo) {
		//TODO: parse
		CloseLevelSelect();
	}
	public void OpenLevelSelect() {
		levelSelectScreen.gameObject.SetActive(true);
		MenuManager.instance.SetMenuScreen(false);
		mainPlayerDisplay.SetActive(false);
	}
	public void CloseLevelSelect() {
		levelSelectScreen.gameObject.SetActive(false);
		MenuManager.instance.SetMenuScreen(true);
		mainPlayerDisplay.SetActive(true);
	}
	public void GoToMode(int modeSegment) {
		if (modeSegment == 0) {
			StartCoroutine(JumpMode(0));
			return;
		}
		if (modeSegment == modeNormalizedThresholds.Count - 1) {
			StartCoroutine(JumpMode(1));
			return;
		}
		StartCoroutine(JumpMode((modeNormalizedThresholds[modeSegment] +
			modeNormalizedThresholds[modeSegment + 1]) / 2f));
	}
	private IEnumerator JumpMode(float targetPos) {
		for (float i = 0; i < 0.2f; i += Time.deltaTime) {
			levelScroller.horizontalNormalizedPosition +=
				(targetPos - levelScroller.horizontalNormalizedPosition) * Time.deltaTime * 30f;
			yield return new WaitForEndOfFrame();
		}
	}

	private void Awake() {
		instance = this;
	}

	private void Start() { }

	private void Update() {
		for (int i = 0; i < modeNormalizedThresholds.Count; i++) {
			float min = modeNormalizedThresholds[i];
			float max = i == modeNormalizedThresholds.Count - 1 ? 2 : modeNormalizedThresholds[i + 1];

			//show as white
			if (levelScroller.horizontalNormalizedPosition >= min && levelScroller.horizontalNormalizedPosition < max) {
				modeTexts[i].color = new Color(1f, 1f, 1f);
			} else {
				modeTexts[i].color = new Color(0.7f, 0.7f, 0.7f);
			}
		}
	}

	#endregion

}
