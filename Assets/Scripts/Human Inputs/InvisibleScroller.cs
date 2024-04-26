using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InvisibleScroller : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
	private bool pointerDown = false;
	public bool GetPointerDown() { return pointerDown; }

	private Vector2 pressPosition = Vector2.zero;
	public Vector2 GetPressPosition() { return pressPosition; }

	private PointerEventData currentPointer = null;
	private Vector2 lastPointerPosition = Vector2.zero;
	private Vector2 deltaPointerPosition = Vector2.zero;
	public Vector2 GetMouseDelta() {
		return deltaPointerPosition;
	}

	public void OnPointerDown(PointerEventData eventData) {
		pressPosition = eventData.pressPosition;
		lastPointerPosition = eventData.position;
		currentPointer = eventData;
		pointerDown = true;
	}
	public void OnPointerUp(PointerEventData eventData) {
		pointerDown = false;
	}
	private void Update() {
		if (!pointerDown) {
			deltaPointerPosition = Vector2.zero;
		} else {
			deltaPointerPosition = currentPointer.position - lastPointerPosition;
			lastPointerPosition = currentPointer.position;
		}
	}
}