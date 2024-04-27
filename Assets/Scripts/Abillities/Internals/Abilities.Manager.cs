using System.Collections.Generic;
using UnityEngine;

namespace Abilities {

    public static class AbilityManagerExtensions { 
        /// <summary>
        /// For HUMANS only. Called every tick.
        /// </summary>
        public static void SysTickAndAbilityHandler(this NetworkedEntity entity, List<(IAbility ability, bool isActivated)> abilities) {
            abilities.UpdateAbilityList();
            for(int i = 0; i < abilities.Count; i++) {
                IAbility a = abilities[i].ability;
                if (a is ISysTickable) { // SysTick Callback.
                    ((ISysTickable)a).SysTickCall();
                }

                /*======================| Abilities |======================*/

                switch(a) {
                    case RapidHeal:
                        if (((RapidHeal)a).GetIsActive()) {        
                            NetworkedEntity.playerInstance.HealthNetworkEntityCall(
                                ((RapidHeal)a).healAmount * entity.GetEntity().GetMaxHealth() * Time.deltaTime / ((RapidHeal)a).healDuration, true);
                            //active mode << ui button changes ??

                        } break;
                    case RapidFire:
                        // if (((RapidFire)a).GetIsActive()) {      
                        // } else {
                        //     if (UIController.instance.GetAbilityButton(i) == null) continue;
                        // } break;
                        break;
                }

                //entity.GetTotalDmgDealt() to determine damage charging!

                /*======================| Ability Callbacks |======================*/

                if (a is IActivatable) {
                    if (abilities[i].isActivated) { Debug.Log("Being activated"); ((IActivatable)a).Activate(); abilities[i] = (a, false); }
                }
                if (a is IPassiveable) {
                    
                    //apply passive buffs & changes
                }
		    } 
        }

        public static void PushAbilityActivation(this List<(IAbility ability, bool isActivated)> abilities, int index) {
            abilities[index] = (abilities[index].ability, true);
        }

        public static GameObject FindChild(this GameObject parent, string name) {
            foreach (Transform child in parent.transform) {
                if (child.name == name) { return child.gameObject; }
            }
            return null;
        }
    }
}
