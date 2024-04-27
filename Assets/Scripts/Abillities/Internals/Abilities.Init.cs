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
            RapidHeal h = new RapidHeal();
            input.Add((h, false));

            AbilityUIManagerExtensions.OnAbilityListChange(); // callback for ui update. REQUIRED, o.w. no abilities will show.
        }

        public static void UpdateAbility(this IAbility i) {

            //TODO: Link with garage / in game tech tree.
            switch(i) {
                case RapidHeal:
                    ((RapidHeal)i).cooldownPeriod = ((RapidHeal)i).remainingCooldownTime = 10f;
                    ((RapidHeal)i).healAmount = .3f; // percentage heal.
                    ((RapidHeal)i).healDuration = 3f;
                    break;
                case RapidFire:
                    ((RapidFire)i).cooldownPeriod = ((RapidFire)i).remainingCooldownTime = 10f;
                    ((RapidFire)i).firingPeriod = ((RapidFire)i).remainingFiringTime = 3f;
                    break;
            }
        }
    }
}