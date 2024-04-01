using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	#region Prefabs

	[SerializeField] private GameObject explosionPrefab;

	#endregion

	#region Members

	[SerializeField] private float speed;
	[SerializeField] private float damage;

	private int senderTeam = -1;

	#endregion

	#region Functions

	public void Init(int team) {
		senderTeam = team;
	}
	private void OnTriggerEnter(Collider other) {
		//TODO: link with networking
		if (other.gameObject.TryGetComponent(out Entity e)) {
			if (e.GetTeam() == senderTeam) return;

			e.LoseHealth(damage);
		}

		Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}
	private void Start() {
		//TODO: link object startup/destruction with networking
		Destroy(gameObject, 5f);
	}
	void Update() {
		transform.Translate(speed * Time.deltaTime * Vector3.forward);
	}

	#endregion
}
