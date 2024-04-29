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

            //TODO: Link with garage / in game tree.
            switch(i) {
                //==== HEALING ====//
                case Heal:
                    ((Heal)i).cooldownPeriod = ((Heal)i).remainingCooldownTime = .5f;
                    ((Heal)i).healAmount = .8f; // percentage heal.
                    ((Heal)i).healDuration = 1f;
                    break;
                case AreaHeal:
                    ((AreaHeal)i).cooldownPeriod = 10f;
                    ((AreaHeal)i).healAmount = .8f; // percentage heal.
                    ((AreaHeal)i).healPeriod = 1f;
                    ((AreaHeal)i).healRadius = 5f;
                    break;
                case InfiHeal:
                    ((InfiHeal)i).cooldownPeriod = ((InfiHeal)i).remainingCooldownTime = 10f; 
                    ((InfiHeal)i).healPerSec = 300f;
                    ((InfiHeal)i).healDuration = 15f;
                    break;
                case HPSteal:
                    ((HPSteal)i).cooldownPeriod = 10f;
                    ((HPSteal)i).stealAmount = 250f; //FLAT hp stolen
                    ((HPSteal)i).stealPeriod = .5f;
                    ((HPSteal)i).stealRadius = 5f;
                    break;

                //==== DAMAGE ====//
                case RapidFire:
                    ((RapidFire)i).cooldownPeriod = ((RapidFire)i).remainingCooldownTime = .5f;
                    ((RapidFire)i).firingPeriod = ((RapidFire)i).remainingFiringTime = 3f;
                    break;
            }
        }
    }
}