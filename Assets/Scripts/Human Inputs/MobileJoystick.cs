using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// NOTE: salvaged from the IronInc. mobile folder, fix up
/// </summary>

public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
	[SerializeField] private RectTransform joystickTip;

	private bool buttonJustUp = false; //when pointer is up, this will be set true until first poll
	private bool buttonDown = false; //for full auto
	private bool buttonJustDown = false;
	private float joystickAngle = Mathf.PI / 2f, joystickMagnitude = 0; //magnitude between 0 and 1
	private float lastJoystickMagnitude = 0; //don't reset this one!
	protected PointerEventData currentPointer = null;
	private Canvas mainCanvas = null;

	public float GetJoystickAngle() {
		return joystickAngle;
	}
	public float GetJoystickMagnitude(bool getLast) {
		return getLast ? lastJoystickMagnitude : joystickMagnitude;
	}
	public bool GetButtonIsDown() {
		return buttonDown;
	}
	public void OnPointerDown(PointerEventData eventData) {
		buttonDown = true;
		currentPointer = eventData;
		PointerDown();
	}
	public void OnPointerUp(PointerEventData eventData) {
		buttonDown = false;
		currentPointer = null;
		PointerUp();
	}
	public bool ButtonWasJustUp() {
		if (buttonJustUp) {
			buttonJustUp = false;
			return true;
		}
		return false;
	}
	public bool ButtonWasJustDown() {
		if (buttonJustDown) {
			buttonJustDown = false;
			return true;
		}
		return false;
	}
	protected virtual void PointerDown() {
		buttonJustDown = true;
	}
	protected virtual void PointerUp() {
		buttonJustUp = true;
	}
	void Start() {
		mainCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
	}
	void Update() {
		if (currentPointer != null) {
			Vector2 diff = currentPointer.position - (Vector2)transform.position;
			float scale = GetComponent<RectTransform>().sizeDelta.x / 2f * mainCanvas.scaleFactor;

			float magnitude = Mathf.Min(diff.magnitude, scale);

			joystickTip.localPosition = diff.normalized * magnitude / mainCanvas.scaleFactor;
			joystickMagnitude = magnitude / scale;
			lastJoystickMagnitude = joystickMagnitude;
			joystickAngle = Mathf.Atan2(diff.y, diff.x);
		} else {
			joystickTip.localPosition = Vector2.zero;
			joystickMagnitude = 0;
		}
	}
}
