using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities {

    public static class AbilityInitExtensions {

        public static void UpdateAbilityList(this List<(IAbility, bool)> input) {

            //TODO: Link with garage / in game tech tree.
            if (input.Count > 0) return;

            input.Clear();
            RapidFire f = new RapidFire();
            input.Add((f, false));
            Heal h = new Heal();
            input.Add((h, false));

            AbilityUIManagerExtensions.OnAbilityListChange(); // callback for ui update. REQUIRED, o.w. no abilities will show.
        }

        public static void UpdateAbility(this IAbility i) {

            //TODO: Link with garage / in game tech tree.
            switch(i) {
                case Heal:
                    ((Heal)i).cooldownPeriod = ((Heal)i).remainingCooldownTime = 10f;
                    ((Heal)i).healAmount = .8f; // percentage heal.
                    ((Heal)i).healDuration = 3f;
                    break;
                case RapidFire:
                    ((RapidFire)i).cooldownPeriod = ((RapidFire)i).remainingCooldownTime = 10f;
                    ((RapidFire)i).firingPeriod = ((RapidFire)i).remainingFiringTime = 3f;
                    break;
            }
        }
    }
}