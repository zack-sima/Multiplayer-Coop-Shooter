using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities {

    public static class AbilityUIManagerExtensions {
        /// <summary>
        /// Called every frame by ui to determine what UI to render.
        /// </summary>
        public static List<(IAbility, bool)> PullAbility() { return NetworkedEntity.playerInstance.GetAbilityList(); }

        /// <summary>
        /// Call when abilities list changes.
        /// </summary>
        public static void OnAbilityListChange() { UIController.instance.AbilitiesUpdated(); }
    }
}
