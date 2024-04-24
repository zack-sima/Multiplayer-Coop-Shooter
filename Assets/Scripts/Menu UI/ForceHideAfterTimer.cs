using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Forces the overlay to close itself if the lobby failed to load, etc
/// </summary>

public class ForceHideAfterTimer : MonoBehaviour {

	[SerializeField] private float timer;

	float countdown = 0f;

	public void OnEnable() {
		countdown = timer;
	}
	private void Update() {
		countdown -= Time.deltaTime;
		if (countdown <= 0) gameObject.SetActive(false);
	}
}
