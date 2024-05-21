using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModeScrollTitle : MonoBehaviour {

	[SerializeField] private RectTransform textBegin, textEnd, screenTextBegin;
	[SerializeField] private RectTransform parent;

	Canvas mainCanvas;
	private void Start() {
		transform.SetParent(parent);
		mainCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
	}
	void LateUpdate() {
		float sizeX = GetComponent<RectTransform>().sizeDelta.x * mainCanvas.scaleFactor;
		if (textBegin.position.x > screenTextBegin.position.x) {
			transform.position = textBegin.position;
		} else if (screenTextBegin.position.x < textEnd.position.x - sizeX) {
			transform.position = new Vector2(screenTextBegin.position.x, transform.position.y);
		} else {
			transform.position = textEnd.position - sizeX * Vector3.right;
		}
	}
}
