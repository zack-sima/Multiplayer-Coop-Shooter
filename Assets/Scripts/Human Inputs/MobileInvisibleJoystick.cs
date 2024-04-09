using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attack and movement joysticks are allowed to be pressed anywhere on the map (below other buttons).
/// </summary>

public class MobileInvisibleJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
	[SerializeField] private MobileJoystick activeJoystick;

	public void OnPointerDown(PointerEventData eventData) {
		activeJoystick.SetPosition(eventData.pressPosition);
		activeJoystick.OnPointerDown(eventData);
	}
	public void OnPointerUp(PointerEventData eventData) {
		activeJoystick.RevertPosition();
		activeJoystick.OnPointerUp(eventData);
	}
}
