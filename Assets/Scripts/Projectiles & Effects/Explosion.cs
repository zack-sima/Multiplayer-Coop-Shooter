using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
	[SerializeField] private float timer = 3f;

	[SerializeField] private List<AudioSource> audioSources;
	[SerializeField] private string audioName;
	[SerializeField] private float audioCutoff;

	void Start() {
		if (AudioSourceController.CanPlaySound(audioName, audioCutoff)) {
			foreach (AudioSource s in audioSources)
				s.Play();
		}
		Destroy(gameObject, timer);
	}
}
