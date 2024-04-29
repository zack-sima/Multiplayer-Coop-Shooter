using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities.StatHandler {
    public static class StatHandler {
        /// <summary>
        /// Call ONCE per frame after all stats have been applied to the modifier.
        /// </summary>
        public static void ApplyStatChanges(this NetworkedEntity entity, StatModifier stats) {
             // apply buffs and stuff. TODO: Infliction Handler for local player here.
            NetworkedEntity.playerInstance.HealthPercentNetworkEntityCall(stats.healthPercentModifier);
            NetworkedEntity.playerInstance.HealthFlatNetworkEntityCall(stats.healthFlatModifier);
    
            entity.GetEntity().SetMaxHealth(stats.maxHealthPercentModifier * entity.GetEntity().GetMaxHealth());
            entity.GetEntity().SetMaxHealth(entity.GetEntity().GetMaxHealth() + stats.maxHealthFlatModifier);
        }
    }
}
