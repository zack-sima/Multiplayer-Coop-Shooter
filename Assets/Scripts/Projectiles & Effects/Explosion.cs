using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
	[SerializeField] private float timer = 3f;

	void Start() {
		Destroy(gameObject, timer);
	}
}
