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
		float u1 = 1.0f - UnityEngine.Random.value;
		float u2 = 1.0f - UnityEngine.Random.value;
		float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
		damage = Mathf.Max(damage + damage * (0.1f * randStdNormal), 0);

		bool successfullyDamaged = false;

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

			//if exploding bomb blows up another bomb it waits a bit (0.2s rn)
			e.GetNetworker().RPC_TakeDamage(e.GetNetworker().Object, e.GetNetworker().Object, realDmg,
				(e.GetTurret().GetIsProximityExploder() && canDamageTeam) ? 0.2f : 0f);

			self.IncrementDamageCharge(realDmg); //ability damage charge up
			successfullyDamaged = true;
		}
		if (successfullyDamaged) {
			if (self != null && self.GetNetworker().GetIsPlayer())
				UIController.NudgePhone(2);
		}
	}

	public static void DealDamageToTarget(CombatEntity target, float damage, CombatEntity self = null) {
		NetworkedEntity networker = target.GetNetworker();

		if (!networker.GetInitialized()) return;

		float u1 = 1.0f - UnityEngine.Random.value;
		float u2 = 1.0f - UnityEngine.Random.value;
		float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
		damage = Mathf.Max(damage + damage * (0.2f * randStdNormal), 0);

		if (self != null) {
			(float chance, float dmg) crit = self.GetTurret().GetCritValues();
			if (Random.Range(0, 1f) < crit.chance && crit.dmg > 0) {
				damage *= crit.dmg;
				//TODO: crit sound.
				if (self.GetNetworker().GetIsPlayer()) {
					Debug.LogWarning("Crit hit for : " + damage);
					NetworkedEntity.playerInstance.critSoundEffect.PlayOneShot(
						NetworkedEntity.playerInstance.critSoundEffect.clip);
				}
			}
			self.IncrementDamageCharge(damage);
		} //ability damage charge up

		Fusion.NetworkObject selfNetworkObject = null;
		if (self != null) {
			selfNetworkObject = self.GetComponent<NetworkedEntity>().Object;
		}
		networker.LoseLocalHealth(damage);
		networker.RPC_TakeDamage(networker.Object, selfNetworkObject, damage, 0);
	}
}
