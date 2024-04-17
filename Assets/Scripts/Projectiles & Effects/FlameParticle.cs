using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameParticle : MonoBehaviour {
	private void Start() {
		Destroy(gameObject, 2.5f);
	}
	private void Update() {
		transform.Translate(5f * Time.deltaTime * Vector3.forward);
	}
}
