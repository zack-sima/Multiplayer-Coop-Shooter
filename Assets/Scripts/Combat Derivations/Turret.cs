using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Turret : MonoBehaviour {

	#region References

	[SerializeField] protected TurretAnimatorBase animator;
	public TurretAnimatorBase GetAnimator() { return animator; }

	[SerializeField] private Transform bulletAnchor;

	#endregion

	#region Prefabs

	[SerializeField] private GameObject bulletPrefab;

	#endregion

	#region Members

	//ammo parameters
	[SerializeField] protected int maxAmmo;
	public int GetMaxAmmo() { return maxAmmo; }

	[Tooltip("Bullets per second")]
	[SerializeField] protected float ammoRegenerationSpeed;
	public float GetAmmoRegenSpeed() { return ammoRegenerationSpeed; }

	[SerializeField] protected bool isFullAuto;
	public bool GetIsFullAuto() { return isFullAuto; }

	[Tooltip("in seconds before shooting again")]
	[SerializeField] protected float shootSpeed;
	public float GetShootSpeed() { return shootSpeed; }

	//in degrees of y-randomness
	[SerializeField] protected float shootSpread;

	[Tooltip("in degrees of y-rotation per second")]
	[SerializeField] protected float rotateSpeed;

	//ex: bomb mech is not rotatable
	[SerializeField] protected bool rotatable;
	public bool GetIsRotatable() { return rotatable; }

	//for proximity explode/bomber turret only
	[SerializeField] protected bool proximityExplode;
	public bool GetIsProximityExploder() { return proximityExplode; }

	[SerializeField] protected float explosionRadius;
	public float GetExplosionRadius() { return explosionRadius; }

	[SerializeField] protected float explosionDamage;
	public float GetExplosionDamage() { return explosionDamage; }

	//set to shootSpeed
	protected float shootTimer = 0;

	//turret target rotation
	protected float targetRotation = 0;

	//NOTE: only follows target rotation if one has been set
	protected bool useTargetRotation = false;

	//rotate slowly when auto rotating back for mobile
	protected bool inSlowMode = false;

	#endregion

	#region Functions

	//mobile/new turret rotation
	public void SetTargetTurretRotation(float rotation, bool slow = false) {
		targetRotation = rotation;
		useTargetRotation = true;
		inSlowMode = slow;
	}
	//synced to networking, call only by local input;
	//this function can be called in a framework where the turret is not a sub-part of a Networked Entity
	public GameObject TryFireMainWeapon(int team, int bulletId = 0, CombatEntity optionalSender = null) {
		if (shootTimer > 0) return null; //can't shoot yet

		shootTimer += shootSpeed;

		Bullet b = Instantiate(bulletPrefab, bulletAnchor.position,
			Quaternion.Euler(bulletAnchor.eulerAngles.x, bulletAnchor.eulerAngles.y + Random.Range(
			-shootSpread / 2f, shootSpread / 2f), bulletAnchor.eulerAngles.z)).GetComponent<Bullet>();

		b.Init(optionalSender, team, bulletId, true);

		animator.FireMainWeapon();

		return b.gameObject;
	}
	//called by non-local clients' RPCs; this function must be called in the networked-structure
	public GameObject NonLocalFireWeapon(CombatEntity sender, int team, int bulletId) {
		animator.FireMainWeapon();

		Bullet b = Instantiate(bulletPrefab, bulletAnchor.position,
			Quaternion.Euler(bulletAnchor.eulerAngles.x, bulletAnchor.eulerAngles.y + Random.Range(
			-shootSpread / 2f, shootSpread / 2f), bulletAnchor.eulerAngles.z)).GetComponent<Bullet>();

		//not isLocal makes the bullet harmless and just self-destroy
		b.Init(sender, team, bulletId, isLocal: false);

		return b.gameObject;
	}
	//called when overclocked ability is on
	public void ReloadFaster() {
		if (shootTimer > 0) shootTimer -= Time.deltaTime;
	}
	protected void Update() {
		if (shootTimer > 0) shootTimer -= Time.deltaTime;

		if (useTargetRotation) {
			transform.rotation = Quaternion.RotateTowards(transform.rotation,
				Quaternion.Euler(0, targetRotation, 0),
				Time.deltaTime * rotateSpeed * (inSlowMode ? 0.55f : 1f));
		}
	}

	#endregion
}
