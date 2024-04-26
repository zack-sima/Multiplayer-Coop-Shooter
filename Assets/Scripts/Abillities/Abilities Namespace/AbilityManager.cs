using System.Collections;
using System.Collections.Generic;
using Abilities.Internal;
using UnityEngine;

namespace Abilities.Manager {

    public static class AbilityManager { //idk if this is the best option ...
        public static void ManageAbilities(this List<IAbility> input, HumanCombatEntity human) {
            foreach(IAbility a in input) {
			if (a is ITimeBased) { // time update.
				float chargePercent = ((ITimeBased)a).GetPercentCharged(Time.deltaTime);
				//do some ui stuff with this ^^^
			}
			if (a is IActiveAbility) {
				//check if ability is activated
			}
			if (a is IPassiveAbility) {
                
				//apply passive buffs & changes
			}
		}
        }
    }

    public static class AbilityInitExtensions {
        public static void InitAbility(this IAbility i) {
            //pull from the init list of values.
            //placeholders rn
            switch(i) {
                case RapidHeal:
                    ((RapidHeal)i).SetParameters((10f, 500f, 3f));
                    break;
                case RapidFire:
                    ((RapidFire)i).SetParameters(10f);
                    break;
            }
        }
    }
}
