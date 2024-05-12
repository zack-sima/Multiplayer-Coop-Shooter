using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities {
    public static class AIAbilitiesExtensions {
        //TODO: Zack, add whatever parameters you need here to control what goes on what type of entity.
        public static void UpdateAIAbilityList(this List<(IAbility, bool)> input) {
            if (input.Count > 0) return;
            input.Clear(); // Remove all current abilties.

            RapidFire f = new RapidFire(); // Initialize new ability.
            input.Add((f, false)); // bool is for activation state.

            Heal h = new Heal(); // Initialize new ability.
            h.cooldownPeriod = h.remainingCooldownTime = 3f; // Set cooldown period. (if you want stats that are not the default.)
            //idk if you need like a CSV for these bots to have different stats.
            input.Add((h, false)); // bool is for activation state.

        }
    }
}