using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModeScrollTitle : MonoBehaviour {

	[SerializeField] private RectTransform textBegin, textEnd, screenTextBegin;

	void Update() {
		if (textBegin.position.x > screenTextBegin.position.x) {
			transform.position = textBegin.position;
		} else if (screenTextBegin.position.x < textEnd.position.x) {
			transform.position = new Vector2(screenTextBegin.position.x, transform.position.y);
		} else {
			transform.position = textEnd.position;
		}
	}
}
