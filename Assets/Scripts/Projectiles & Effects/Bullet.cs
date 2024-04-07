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

	private CombatEntity senderEntity = null;
	private int senderTeam = -1;
	private int bulletId = -1;
	private bool senderIsLocal = false;
	private bool alreadyHitTarget = false;

	#endregion

	#region Functions

	public void Init(CombatEntity sender, int team, int bulletId, bool isLocal) {
		senderTeam = team;
		senderIsLocal = isLocal;
		senderEntity = sender;
		this.bulletId = bulletId;
	}
	private void OnTriggerEnter(Collider other) {
		if (alreadyHitTarget) return;

		if (other.gameObject.TryGetComponent(out CombatEntity e)) {
			if (e.GetTeam() == senderTeam) return;
			if (senderIsLocal) {
				NetworkedEntity networker = e.GetNetworker();
				networker.RPC_TakeDamage(networker.Object, damage);
			}
		}
		alreadyHitTarget = true;
		if (senderIsLocal) {
			DestroyLocalBullet();
		} else {
			DestroyBulletEffect();
		}
	}
	//just destroys the bullet and removes entity reference to it
	public void DestroyBulletEffect() {
		senderEntity.RemoveBullet(bulletId);
		Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}
	//tries to make an RPC call so everyone destroys the bullet
	private void DestroyLocalBullet() {
		try {
			if (senderEntity != null)
				senderEntity.GetNetworker().RPC_DestroyBullet(bulletId);
		} catch (System.Exception e) { Debug.LogWarning(e); }
		DestroyBulletEffect();
	}
	private IEnumerator DelayedDestroy() {
		yield return new WaitForSeconds(5f);
		DestroyBulletEffect();
	}
	private void Start() {
		StartCoroutine(DelayedDestroy());
	}
	void Update() {
		transform.Translate(speed * Time.deltaTime * Vector3.forward);
	}

	#endregion
}
