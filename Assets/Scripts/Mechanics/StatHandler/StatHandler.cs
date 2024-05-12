using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities.StatHandler {
    public static class StatHandler {
        /// <summary>
        /// Call ONCE per frame on LateUpdate after all stats have been applied to the modifier.
        /// </summary>
        public static void ApplyStatChanges(this NetworkedEntity entity, StatModifier stats) {

            stats.healthPercentModifier += 1f;

            entity.HealthPercentNetworkEntityCall(stats.healthPercentModifier);
            entity.HealthFlatNetworkEntityCall(stats.healthFlatModifier);

            if (entity != NetworkedEntity.playerInstance) { Debug.LogWarning("Non-player entity is being modified by stats."); return; }

            entity.GetEntity().SetMaxHealth(stats.baseHealthPercentModifier * entity.GetEntity().GetBaseHealth() + stats.baseHealthFlatModifier + entity.GetEntity().GetMaxHealth());
        }
    }
}
