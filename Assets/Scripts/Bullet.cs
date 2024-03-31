using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	#region Prefabs

	[SerializeField] private GameObject explosionPrefab;

	#endregion

	void Update() {
		transform.Translate(15f * Time.deltaTime * Vector3.forward);
	}
	private void OnCollisionEnter(Collision collision) {
		Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}
}
