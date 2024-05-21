using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModeScrollControl : MonoBehaviour {

	//the base is singleplayer/multiplayer select -- that screen doesn't have a return button/header band
	[SerializeField] private RectTransform currentScreen;

	[SerializeField] private ScrollRect scroller;
	[SerializeField] private List<TMP_Text> modeTexts;
	[SerializeField] private List<float> modeNormalizedThresholds;

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
			scroller.horizontalNormalizedPosition +=
				(targetPos - scroller.horizontalNormalizedPosition) * Time.deltaTime * 30f;
			yield return new WaitForEndOfFrame();
		}
	}
	public void LeaveScreen() {
		currentScreen.gameObject.SetActive(false);
	}
	void Update() {
		for (int i = 0; i < modeNormalizedThresholds.Count; i++) {
			float min = modeNormalizedThresholds[i];
			float max = i == modeNormalizedThresholds.Count - 1 ? 2 : modeNormalizedThresholds[i + 1];

			//show as white
			if (scroller.horizontalNormalizedPosition >= min && scroller.horizontalNormalizedPosition < max) {
				modeTexts[i].color = new Color(1f, 1f, 1f);
			} else {
				modeTexts[i].color = new Color(0.7f, 0.7f, 0.7f);
			}
		}
	}
}
