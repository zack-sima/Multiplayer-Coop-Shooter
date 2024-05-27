using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
	[SerializeField] private float timer = 3f;

	[SerializeField] private List<AudioSource> audioSources;
	[SerializeField] private string audioName;
	[SerializeField] private float audioCutoff;

	[SerializeField] private float shakeStrength;

	void Start() {
		if (AudioSourceController.CanPlaySound(audioName, audioCutoff)) {
			foreach (AudioSource s in audioSources)
				s.Play();
		}
		if (NetworkedEntity.playerInstance != null) {
			float shake = Mathf.Clamp((shakeStrength * 7f - AIBrain.GroundDistance(
				NetworkedEntity.playerInstance.transform.position, transform.position)) / 10f * shakeStrength, 0f, 0.5f);

			if (shake > 0) HumanInputs.instance.ShakeCamera(shake);
		}
		Destroy(gameObject, timer);
	}
}
