using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All damage calls come here
/// </summary>

public static class DamageHandler {

	//damages Combat Entities in a given range around a given point.
	//  TODO: does not affect normal entities.
	public static void DealExplosiveDamage(Vector3 position, float radius,
		float damage, bool canDamageTeam, CombatEntity self = null) {

		foreach (CombatEntity e in new List<CombatEntity>(EntityController.instance.GetCombatEntities())) {
			if (e == null || e == self || !e.GetNetworker().GetInitialized()) continue;

			//same team
			if (!canDamageTeam && self != null && e.GetTeam() == self.GetTeam()) continue;

			float dist = AIBrain.GroundDistance(position, e.gameObject.transform.position);

			try { //for some reason, GetIsDead() throws error in chain blow up sometimes
				if (e.GetNetworker().GetIsDead() || dist > radius) continue;
			} catch { continue; }

			float realDmg = damage * Mathf.Min(1.5f * radius - 1.2f * dist, radius) / radius;
			e.GetNetworker().LoseLocalHealth(realDmg);
			e.GetNetworker().RPC_TakeDamage(e.GetNetworker().Object, realDmg);
		}
	}
	public static void DealDamageToTarget(CombatEntity target, float damage) {
		NetworkedEntity networker = target.GetNetworker();

		if (!networker.GetInitialized()) return;
		networker.LoseLocalHealth(damage);
		networker.RPC_TakeDamage(networker.Object, damage);
	}
}
