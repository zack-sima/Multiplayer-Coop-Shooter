using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gatling : Turret {
	//second anchor
	[SerializeField] private Transform bulletAnchor2;

	//gatling shoots 2 instead of 1
	public override List<GameObject> TryFireMainWeapon(int team, int bulletId = 0, CombatEntity optionalSender = null) {
		if (shootTimer > 0) return null; //can't shoot yet

		shootTimer += shootSpeed;

		Bullet b = Instantiate(GetBulletPrefab(), GetBulletAnchor().position,
			Quaternion.Euler(GetBulletAnchor().eulerAngles.x, GetBulletAnchor().eulerAngles.y + Random.Range(
			-shootSpread / 2f, shootSpread / 2f), GetBulletAnchor().eulerAngles.z)).GetComponent<Bullet>();

		b.Init(optionalSender, team, bulletId, true);

		Bullet b2 = Instantiate(GetBulletPrefab(), bulletAnchor2.position,
			Quaternion.Euler(bulletAnchor2.eulerAngles.x, bulletAnchor2.eulerAngles.y + Random.Range(
			-shootSpread / 2f, shootSpread / 2f), bulletAnchor2.eulerAngles.z)).GetComponent<Bullet>();

		b2.Init(optionalSender, team, bulletId, true);

		animator.FireMainWeapon(bulletId);

		return new() { b.gameObject, b2.gameObject };
	}
	//called by non-local clients' RPCs; this function must be called in the networked-structure
	public override List<GameObject> NonLocalFireWeapon(CombatEntity sender, int team, int bulletId) {
		animator.FireMainWeapon(bulletId);

		Bullet b = Instantiate(GetBulletPrefab(), GetBulletAnchor().position,
			Quaternion.Euler(GetBulletAnchor().eulerAngles.x, GetBulletAnchor().eulerAngles.y + Random.Range(
			-shootSpread / 2f, shootSpread / 2f), GetBulletAnchor().eulerAngles.z)).GetComponent<Bullet>();

		//not isLocal makes the bullet harmless and just self-destroy
		b.Init(sender, team, bulletId, isLocal: false);

		Bullet b2 = Instantiate(GetBulletPrefab(), bulletAnchor2.position,
			Quaternion.Euler(bulletAnchor2.eulerAngles.x, bulletAnchor2.eulerAngles.y + Random.Range(
			-shootSpread / 2f, shootSpread / 2f), bulletAnchor2.eulerAngles.z)).GetComponent<Bullet>();

		b2.Init(sender, team, bulletId, isLocal: false);

		return new() { b.gameObject, b2.gameObject };
	}
}
