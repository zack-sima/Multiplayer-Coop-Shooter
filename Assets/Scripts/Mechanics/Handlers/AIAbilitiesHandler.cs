using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSV.Parsers;

namespace Abilities {
    public static class AIAbilitiesExtensions {
        //TODO: Add whatever parameters you need here to control what goes on what type of entity.
        public static void UpdateAIAbilityList(this List<(IAbility, bool)> input) {

            input.Clear(); // Remove all current abilties.

            // RapidFire f = new RapidFire(); // Initialize new ability.
            // input.Add((f, false)); // bool is for activation state.

            Heal h = new Heal(CSVId.HealActive); // Initialize new ability.
            h.cooldownPeriod = h.remainingCooldownTime = 3f; // Set cooldown period. (if you want stats that are not the default.)
            //TODO: idk if you need like a CSV for these bots to have different stats.

            input.Add((h, false)); // bool is for activation state.

            //entity.GetNetworker().PushAIAbilityActivation(1); < call to activate ability 2 (Heal) on NetworkEntity.

        }
    }
}