using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour {

	#region References

	[SerializeField] private CombatEntity entity;
	[SerializeField] private TurretAnimatorBase animator;
	[SerializeField] private Transform bulletAnchor;

	#endregion

	#region Prefabs

	[SerializeField] private GameObject bulletPrefab;

	#endregion

	#region Members

	//in seconds before shooting again
	[SerializeField] private float shootSpeed;

	//in degrees of y-randomness
	[SerializeField] private float shootSpread;

	//in degrees of y-rotation per second
	[SerializeField] private float rotateSpeed;

	//set to shootSpeed
	private float shootTimer = 0;

	//turret target rotation
	private float targetRotation = 0;

	//NOTE: only follows target rotation if one has been set
	private bool useTargetRotation = false;

	#endregion

	#region Functions

	//old-style turret rotation by keys
	public void RotateTurret(bool isRight) {
		transform.Rotate(0, Time.deltaTime * rotateSpeed * (isRight ? 1f : -1f), 0);
	}
	//mobile/new turret rotation
	public void SetTargetTurretRotation(float rotation) {
		targetRotation = rotation;
		useTargetRotation = true;
	}

	//TODO: sync to networking
	public void TryFireMainWeapon() {
		if (shootTimer > 0) return; //can't shoot yet

		entity.PreventHealing();

		shootTimer += shootSpeed;

		Bullet b = Instantiate(bulletPrefab, bulletAnchor.position,
			Quaternion.Euler(bulletAnchor.eulerAngles.x, bulletAnchor.eulerAngles.y + Random.Range(
			-shootSpread / 2f, shootSpread / 2f), bulletAnchor.eulerAngles.z)).GetComponent<Bullet>();

		b.Init(entity.GetTeam());
		animator.FireMainWeapon();
	}

	private void Update() {
		if (shootTimer > 0) shootTimer -= Time.deltaTime;

		if (useTargetRotation) {
			transform.rotation = Quaternion.RotateTowards(transform.rotation,
				Quaternion.Euler(0, targetRotation, 0), Time.deltaTime * rotateSpeed);
		}
	}

	#endregion
}
