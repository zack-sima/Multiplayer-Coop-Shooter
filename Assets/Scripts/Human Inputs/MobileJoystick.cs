using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// NOTE: salvaged from the IronInc. mobile folder, fix up
/// </summary>

public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
	[SerializeField] private RectTransform joystickTip;

	public event Action OnJoystickPressed;
	public event Action OnJoystickReleased;

	private bool buttonDown = false; //for full auto
	private float joystickAngle = Mathf.PI / 2f, joystickMagnitude = 0; //magnitude between 0 and 1
	protected PointerEventData currentPointer = null;
	private Canvas mainCanvas = null;

	private Vector2 initPosition = new();

	public float GetJoystickAngle() {
		return joystickAngle;
	}
	public float GetJoystickMagnitude() {
		return joystickMagnitude;
	}
	public bool GetButtonIsDown() {
		return buttonDown;
	}
	public void SetPosition(Vector2 position) {
		transform.position = position;
	}
	public void RevertPosition() {
		GetComponent<RectTransform>().anchoredPosition = initPosition;
	}
	public void OnPointerDown(PointerEventData eventData) {
		buttonDown = true;
		currentPointer = eventData;
		OnJoystickPressed?.Invoke();
	}
	public void OnPointerUp(PointerEventData eventData) {
		buttonDown = false;
		currentPointer = null;
		OnJoystickPressed?.Invoke();
	}
	void Start() {
		mainCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
		initPosition = GetComponent<RectTransform>().anchoredPosition;
	}
	void Update() {
		if (currentPointer != null) {
			Vector2 diff = currentPointer.position - (Vector2)transform.position;
			float scale = GetComponent<RectTransform>().sizeDelta.x / 2f * mainCanvas.scaleFactor;

			float magnitude = Mathf.Min(diff.magnitude, scale);

			joystickTip.localPosition = diff.normalized * magnitude / mainCanvas.scaleFactor;
			joystickMagnitude = magnitude / scale;
			joystickAngle = Mathf.Atan2(diff.y, diff.x);
		} else {
			joystickTip.localPosition = Vector2.zero;
			joystickMagnitude = 0;
		}
	}
}
