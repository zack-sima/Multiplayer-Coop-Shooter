using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	#region Prefabs

	[SerializeField] private GameObject explosionPrefab;
	[SerializeField] private GameObject firePrefab;

	#endregion

	#region Members

	[SerializeField] private bool isGrenade;
	[SerializeField] private bool isMissile;
	[SerializeField] private bool isRailGun;

	//missiles have teams
	[SerializeField] private TeamMaterialManager missileTeamMaterials;
	[SerializeField] private Renderer missileRenderer;

	[SerializeField] protected float speed;
	public float GetSpeed() { return speed; }

	[SerializeField] private float damage;
	[SerializeField] private bool isExplosion, isFlame;
	[SerializeField] private float explosionRadius;
	[SerializeField] private float maxDistance = 10f;

	private CombatEntity senderEntity = null;
	private int senderTeam = -1;
	private int bulletId = -1;
	private bool senderIsLocal = false;
	private bool alreadyHitTarget = false;

	//for flame
	private readonly List<Collider> hitTargets = new();
	private GameObject spawnedFlame = null;

	//self destruction
	private float distanceTravelled = 0;
	private bool destroyed = false;

	//for missile
	private float time = 0.8f;
	private float bogoRotate = 0f;

	#endregion

	#region Functions

	public void Init(CombatEntity sender, int team, int bulletId, bool isLocal) {
		senderTeam = team;
		senderIsLocal = isLocal;
		senderEntity = sender;
		this.bulletId = bulletId;

		damage *= sender.GetTurret().GetBulletModi();// * (PlayerInfo.GetIsPVP() ? 1.5f : 1f);

		if (isMissile || isGrenade) missileRenderer.material = missileTeamMaterials.GetTeamColor(team);

		if (isMissile) {
			System.Random r = new(bulletId);
			bogoRotate = (float)r.NextDouble() * 3f;
			speed /= 1.25f;
		}
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
		if (!isRailGun) Destroy(gameObject);
		else Destroy(gameObject, 1.5f);
	}
	//tries to make an RPC call so everyone destroys the bullet
	private void DestroyLocalBullet() {
		try {
			if (senderEntity != null)
				senderEntity.GetNetworker().RPC_DestroyBullet(bulletId);
		} catch (System.Exception e) { Debug.LogWarning(e); }
		DestroyBulletEffect();
	}
	private void Start() {
		if (isGrenade) {
			GetComponent<Rigidbody>().AddForce(Random.Range(65f, 85f) * speed * transform.forward + Vector3.up * 50f +
				Random.insideUnitSphere * Random.Range(0f, 5f));
		}
		if (firePrefab != null) {
			spawnedFlame = Instantiate(firePrefab, transform.position, transform.rotation);
		}
		//no matter what, destroy after 5s
		Destroy(gameObject, 5f);
	}
	virtual protected void Update() {
		if (!isGrenade) {
			if (isMissile) {
				time = Mathf.Min(time + Time.deltaTime * 5f, 3.5f);
				speed += Time.deltaTime * time * time;
				transform.Rotate(Time.deltaTime * 5f * (speed - 5f) *
					Mathf.Sin(Mathf.Min(speed * 0.7f, 15f) * 1.2f * Mathf.PI + bogoRotate) * Vector3.up);
			}
			transform.Translate(speed * Time.deltaTime * Vector3.forward);
			distanceTravelled += speed * Time.deltaTime;
		} else {
			distanceTravelled += Time.deltaTime;
		}

		if (distanceTravelled > maxDistance && ((!destroyed &&
			senderEntity != null && (senderEntity.GetIsPlayer() || PlayerInfo.GetIsPVP())) || isGrenade)) {
			if (senderIsLocal && isExplosion) {
				DamageHandler.DealExplosiveDamage(transform.position, explosionRadius,
					damage, false, senderEntity);
			}
			DestroyBulletEffect();
			destroyed = true;
		}
	}

	#endregion
}
