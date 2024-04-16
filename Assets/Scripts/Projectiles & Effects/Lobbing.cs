using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Video;

public class Lobbing : Bullet {
	private float colliderTimer = .5f;

	void Awake() {
		gameObject.GetComponent<Collider>().enabled = false;
	}
	void Start() {
		GetComponent<Rigidbody>().velocity = speed * transform.forward;
	}

	protected override void Update() {
		if (colliderTimer < 0) {
			gameObject.GetComponent<Collider>().enabled = true;

		} else colliderTimer -= Time.deltaTime;
	}
}
