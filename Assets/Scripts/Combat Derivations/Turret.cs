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

	//unchanged after being set in inspector
	[SerializeField] private float shootSpeed;

	//set to shootSpeed
	private float shootTimer = 0;

	#endregion

	#region Functions

	//old-style turret rotation by keys
	public void RotateTurret(bool isRight) {
		transform.Rotate(0, Time.deltaTime * 150f * (isRight ? 1f : -1f), 0);
	}
	//mobile/new turret rotation
	public void SetTargetTurretRotation() {
		//TODO: rotate towards a point!
	}

	//TODO: sync to networking
	public void TryFireMainWeapon() {
		if (shootTimer > 0) return; //can't shoot yet

		shootTimer += shootSpeed;
		Bullet b = Instantiate(bulletPrefab, bulletAnchor.transform.position,
			bulletAnchor.transform.rotation).GetComponent<Bullet>();
		b.Init(entity.GetTeam());
		animator.FireMainWeapon();
	}

	private void Update() {
		if (shootTimer > 0) shootTimer -= Time.deltaTime;
	}

	#endregion
}
