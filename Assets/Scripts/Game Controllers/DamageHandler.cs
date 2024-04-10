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

			float dist = Vector3.Distance(position, e.gameObject.transform.position);

			try { //for some reason, GetIsDead() throws error in chain blow up sometimes
				if (e.GetNetworker().GetIsDead() || dist > radius) continue;
			} catch { continue; }

			e.GetNetworker().RPC_TakeDamage(e.GetNetworker().Object,
				damage * Mathf.Min(radius - dist + radius / 2f, radius) / radius);
		}
	}
	public static void DealDamageToTarget(CombatEntity target, float damage) {
		NetworkedEntity networker = target.GetNetworker();
		networker.RPC_TakeDamage(networker.Object, damage);
	}
}
