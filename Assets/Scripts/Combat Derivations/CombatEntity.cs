using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatEntity : Entity {
	//TODO: when spawning player/enemies, etc, player spawns a combat entity shell and spawns
	//specific hull/turret prefabs to populate it; separate all animators from shell structure

	#region References

	[SerializeField] private Hull hull; //for movement
	public Hull GetHull() { return hull; }

	[SerializeField] private Turret turret; //for combat
	public Turret GetTurret() { return turret; }

	#endregion

	#region Members

	//unique bulletid
	private int bulletsFired = 0;

	//keeps track of bullets spawned so when an RPC is called to destroy nonlocal
	private readonly Dictionary<int, GameObject> bullets = new();
	public void AddBullet(int bulletId, GameObject bulletRef) {
		bullets.TryAdd(bulletId, bulletRef);
	}
	public void RemoveBullet(int bulletId) {
		if (bullets.ContainsKey(bulletId)) {
			GameObject bullet = bullets[bulletId];
			bullets.Remove(bulletId);

			//NOTE: this must be after removing the bullet from the dictionary
			//because DestroyBulletEffect attempts to remove the bullet again (for non-local players)
			if (bullet != null) {
				bullet.GetComponent<Bullet>().DestroyBulletEffect();
			}
		}
	}

	#endregion

	#region Functions

	//non-local bullet just for decorative purposes
	public void NonLocalFireMainWeapon(int bulletId) {
		GameObject b = turret.NonLocalFireWeapon(this, GetTeam(), bulletId);

		if (b == null) return;

		AddBullet(bulletId, b);
	}
	//the one that actually creates a bullet that matters
	public void TryFireMainWeapon() {
		GameObject b = turret.TryFireMainWeapon(GetTeam(), bulletsFired, optionalSender: this);

		if (b == null) return;

		PreventHealing();
		AddBullet(bulletsFired, b);

		GetNetworker().RPC_FireWeapon(bulletsFired);

		bulletsFired++;
	}
	public override void RespawnEntity() {
		base.RespawnEntity();
		hull.gameObject.SetActive(true);
		hull.GetAnimator().Teleported();
		turret.gameObject.SetActive(true);
	}
	public override void SetEntityToDead() {
		base.SetEntityToDead();
		hull.gameObject.SetActive(false);
		turret.gameObject.SetActive(false);
	}
	public override void SetTeam(int newTeam) {
		base.SetTeam(newTeam);

		Material teamMat = GetTeamMaterials().GetTeamColor(newTeam);
		hull.GetAnimator().SetTeamMaterial(teamMat);
		turret.GetAnimator().SetTeamMaterial(teamMat);
	}
	public override void AddEntityToRegistry() {
		EntityController.instance.AddToCombatEntities(this);
	}
	public override void RemoveEntityFromRegistry() {
		EntityController.instance.RemoveFromCombatEntities(this);
	}

	#endregion

}
