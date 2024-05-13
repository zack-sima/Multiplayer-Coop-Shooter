using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Turret : MonoBehaviour {

	#region References

	[SerializeField] protected TurretAnimatorBase animator;
	public TurretAnimatorBase GetAnimator() { return animator; }

	[SerializeField] private Transform bulletAnchor;
	protected Transform GetBulletAnchor() { return bulletAnchor; }

	#endregion

	#region Prefabs

	[SerializeField] private GameObject bulletPrefab;
	protected GameObject GetBulletPrefab() { return bulletPrefab; }

	#endregion

	#region Members

	//ammo parameters
	[SerializeField] protected int maxAmmo;
	public int GetMaxAmmo() { return maxAmmo; }
	public void SetMaxAmmo(int maxAmmo) { this.maxAmmo = maxAmmo; }

	[Tooltip("Bullets per second")]
	[SerializeField] protected float ammoRegenerationSpeed;
	public float GetAmmoRegenSpeed() { return ammoRegenerationSpeed; }

	[SerializeField] protected bool isFullAuto;
	public bool GetIsFullAuto() { return isFullAuto; }
	public void SetIsFullAuto(bool isFullAuto) { this.isFullAuto = isFullAuto; }

	[Tooltip("in seconds before shooting again")]
	[SerializeField] protected float shootSpeed;
	public float GetShootSpeed() { return shootSpeed; }

	//in degrees of y-randomness
	[SerializeField] protected float shootSpread;
	public float GetShootSpread() { return shootSpread; }
	public void SetShootSpread(float shootSpread) { this.shootSpread = shootSpread; }

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

	protected float critChance = 0, critDamage = 1;
	public (float critChance, float critDamage) GetCritValues() { return (critChance, critDamage); }
	public void SetCritChance(float critChance) { this.critChance = critChance; }
	public void SetCritDamage(float critDamage) { this.critDamage = critDamage; }

	//set to shootSpeed
	protected float shootTimer = 0;

	//turret target rotation
	protected float targetRotation = 0;

	//NOTE: only follows target rotation if one has been set
	protected bool useTargetRotation = false;

	//rotate super fast (but not instant) for mobile
	protected bool rotateFast = false;

	#endregion

	#region Functions

	//*======================| Stats |======================*//

	private float baseShootSpeed = 1f, baseAmmoRegen = 1f;
	protected float bulletDmgModi = 1f;

	public float GetBaseShootSpeed() { return baseShootSpeed; }
	public void SetShootSpeed(float shootSpeed) { this.shootSpeed = shootSpeed; }

	public float GetBaseAmmoRegenRate() { return baseAmmoRegen; }
	public void SetAmmoRegenRate(float ammoRegen) { ammoRegenerationSpeed = ammoRegen; }

	public float GetBaseBulletModi() { return 1f; } // Base is always 1f.
	public float GetBulletModi() { return bulletDmgModi; }
	public void SetBulletDmgModi(float bulletDmgModi) { this.bulletDmgModi = bulletDmgModi; }
	private void SetBaseValues() {
		baseShootSpeed = shootSpeed;
		baseAmmoRegen = ammoRegenerationSpeed;
	}

	//for non-instant rotations
	public void SetTargetTurretRotation(float rotation) {
		targetRotation = rotation;
		useTargetRotation = true;
		rotateFast = false;
	}
	//MOBILE: instant rotation when joystick is used
	public void SnapToTargetRotation(float rotation, bool instant) {
		targetRotation = rotation;
		useTargetRotation = true;

		if (instant) {
			transform.eulerAngles = new Vector3(0, rotation, 0);
		} else {
			rotateFast = true;
		}
	}
	//synced to networking, call only by local input;
	//this function can be called in a framework where the turret is not a sub-part of a Networked Entity
	public virtual List<GameObject> TryFireMainWeapon(int team, int bulletId = 0, CombatEntity optionalSender = null) {
		if (shootTimer > 0) return null; //can't shoot yet

		shootTimer += shootSpeed;

		Bullet b = Instantiate(bulletPrefab, bulletAnchor.position,
			Quaternion.Euler(bulletAnchor.eulerAngles.x, bulletAnchor.eulerAngles.y + Random.Range(
			-shootSpread / 2f, shootSpread / 2f), bulletAnchor.eulerAngles.z)).GetComponent<Bullet>();

		b.Init(optionalSender, team, bulletId, true);

		animator.FireMainWeapon(bulletId);

		return new() { b.gameObject };
	}
	//called by non-local clients' RPCs; this function must be called in the networked-structure
	public virtual List<GameObject> NonLocalFireWeapon(CombatEntity sender, int team, int bulletId) {
		animator.FireMainWeapon(bulletId);

		Bullet b = Instantiate(bulletPrefab, bulletAnchor.position,
			Quaternion.Euler(bulletAnchor.eulerAngles.x, bulletAnchor.eulerAngles.y + Random.Range(
			-shootSpread / 2f, shootSpread / 2f), bulletAnchor.eulerAngles.z)).GetComponent<Bullet>();

		//not isLocal makes the bullet harmless and just self-destroy
		b.Init(sender, team, bulletId, isLocal: false);

		return new() { b.gameObject };
	}
	//called when overclocked ability is on
	public void ReloadFaster() {
		if (shootTimer > 0) shootTimer -= Time.deltaTime;
	}
	protected virtual void Awake() {
		SetBaseValues();
	}
	protected virtual void Update() {
		if (shootTimer > 0) shootTimer -= Time.deltaTime;

		if (useTargetRotation) {
			transform.rotation = Quaternion.RotateTowards(transform.rotation,
				Quaternion.Euler(0, targetRotation, 0), Time.deltaTime * (rotateFast ? rotateSpeed * 2 : rotateSpeed));
		}
	}

	#endregion
}
