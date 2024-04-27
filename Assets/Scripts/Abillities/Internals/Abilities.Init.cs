using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities {
    
    public static class AbilityInitExtensions {
        public static void UpdateAbilityList(this List<(IAbility, bool)> input) {
            //Placeholders rn.
            if (input.Count > 0) return;
            Debug.Log("Abilities Updated");
            input.Clear();
            RapidHeal h = new RapidHeal();
            input.Add((h, false));
            AbilityUIManagerExtensions.OnAbilityListChange(); // callback for ui update.
        }
        public static void UpdateAbility(this IAbility i) {
            //pull from the init list of values.
            //placeholders rn
            //float okay = Time.deltaTime;
            
            switch(i) {
                case RapidHeal:
                    ((RapidHeal)i).cooldownPeriod = ((RapidHeal)i).remainingCooldownTime = 3f;
                    ((RapidHeal)i).healAmount = .3f; // percentage heal.
                    ((RapidHeal)i).healDuration = 3f;
                    break;
                // case RapidFire:
                //     ((RapidFire)i).SetParameters(10f);
                //     break;
            }
        }
    }
}