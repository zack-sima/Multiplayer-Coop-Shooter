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

            //TODO: Fix stat applications. Also get rid of the bullshit class passing. Do it in a cleaner, better way!
            entity.GetEntity().SetMaxHealth(stats.baseHealthPercentModifier * entity.GetEntity().GetBaseHealth() +  entity.GetEntity().GetBaseHealth());
            entity.GetEntity().SetMaxHealth(stats.baseHealthFlatModifier +  entity.GetEntity().GetMaxHealth());
        }
    }
}
