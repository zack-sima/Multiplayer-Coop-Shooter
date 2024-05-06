using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	#region Prefabs

	[SerializeField] private GameObject explosionPrefab;
	[SerializeField] private GameObject firePrefab;

	#endregion

	#region Members

	[SerializeField] protected float speed;
	public float GetSpeed() { return speed; }

	[SerializeField] private float damage;
	[SerializeField] private bool isExplosion, isFlame;
	[SerializeField] private float explosionRadius;
	[SerializeField] private float destructionTimer = 5f;

	private CombatEntity senderEntity = null;
	private int senderTeam = -1;
	private int bulletId = -1;
	private bool senderIsLocal = false;
	private bool alreadyHitTarget = false;
	private bool isCrit = false;

	//for flame
	private readonly List<Collider> hitTargets = new();
	private GameObject spawnedFlame = null;

	#endregion

	#region Functions

	public void Init(CombatEntity sender, int team, int bulletId, bool isLocal) {
		senderTeam = team;
		senderIsLocal = isLocal;
		senderEntity = sender;
		this.bulletId = bulletId;
		Debug.LogWarning(damage *= sender.GetTurret().GetBulletModi());
	}
	//checks layermask (gpt)
	private bool IsLayerInLayerMask(GameObject obj, LayerMask mask) {
		return (mask.value & (1 << obj.layer)) != 0;
	}
	private void OnTriggerEnter(Collider other) {
		if (alreadyHitTarget) return;
		if (other.gameObject.layer == LayerMask.NameToLayer("Projectiles")) return;

		//flamethrower gets to hit every target once
		if (isFlame) {
			if (hitTargets.Contains(other)) return;
			hitTargets.Add(other);
		}
		if (other.gameObject.TryGetComponent(out CombatEntity e)) {
			try {
				if (!e.GetNetworker().GetInitialized() || e.GetTeam() == senderTeam) return;
			} catch { return; }

			if (senderIsLocal && !isExplosion) {
				try {
					DamageHandler.DealDamageToTarget(e, damage, senderEntity);
				} catch { return; }

				if (senderEntity != null && senderEntity.GetNetworker().GetIsPlayer())
					UIController.NudgePhone(isFlame ? 0 : 1);
			}
		}
		if (senderIsLocal && isExplosion) {
			DamageHandler.DealExplosiveDamage(transform.position, explosionRadius,
				damage, false, senderEntity);
		}
		if (isFlame && IsLayerInLayerMask(other.gameObject, LayerMask.GetMask("Default", "Ground"))) {
			StartCoroutine(DelayedDestroyFlame());
			return;
		}
		if (!isFlame) {
			MarkProjectileDestroyed();
		}
	}
	private IEnumerator DelayedDestroyFlame() {
		yield return new WaitForSeconds(0.15f);
		MarkProjectileDestroyed();
	}
	private void MarkProjectileDestroyed() {
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
		if (explosionPrefab != null)
			Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		if (spawnedFlame != null)
			Destroy(spawnedFlame);
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
		yield return new WaitForSeconds(destructionTimer);
		DestroyBulletEffect();
	}
	private void Start() {
		StartCoroutine(DelayedDestroy());

		if (firePrefab != null) {
			spawnedFlame = Instantiate(firePrefab, transform.position, transform.rotation);
		}
	}
	virtual protected void Update() {
		transform.Translate(speed * Time.deltaTime * Vector3.forward);
	}

	#endregion
}
