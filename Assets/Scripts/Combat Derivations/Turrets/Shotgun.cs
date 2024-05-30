using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Turret {
	//shoot multiple bullets
	public override List<GameObject> TryFireMainWeapon(int team, int bulletId = 0, CombatEntity optionalSender = null) {
		if (shootTimer > 0) return null; //can't shoot yet

		shootTimer += shootSpeed;

		List<GameObject> spawned = new();
		for (int i = -3; i < 4; i++) {
			Bullet b = Instantiate(GetBulletPrefab(), GetBulletAnchor().position,
			Quaternion.Euler(GetBulletAnchor().eulerAngles.x, GetBulletAnchor().eulerAngles.y + i * 4.2f +
				Random.Range(-shootSpread / 2f, shootSpread / 2f),
				GetBulletAnchor().eulerAngles.z)).GetComponent<Bullet>();
			spawned.Add(b.gameObject);
			b.Init(optionalSender, team, bulletId, true);
		}

		animator.FireMainWeapon(bulletId);

		return spawned;
	}
	//called by non-local clients' RPCs; this function must be called in the networked-structure
	public override List<GameObject> NonLocalFireWeapon(CombatEntity sender, int team, int bulletId) {
		animator.FireMainWeapon(bulletId);

		List<GameObject> spawned = new();
		for (int i = -3; i < 4; i++) {
			Bullet b = Instantiate(GetBulletPrefab(), GetBulletAnchor().position,
			Quaternion.Euler(GetBulletAnchor().eulerAngles.x, GetBulletAnchor().eulerAngles.y + i * 4.2f +
				Random.Range(-shootSpread / 2f, shootSpread / 2f),
				GetBulletAnchor().eulerAngles.z)).GetComponent<Bullet>();

			//not isLocal makes the bullet harmless and just self-destroy
			b.Init(sender, team, bulletId, isLocal: false);
		}

		return spawned;
	}
}
