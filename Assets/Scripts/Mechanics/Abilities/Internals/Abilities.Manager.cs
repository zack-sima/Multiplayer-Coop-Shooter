using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Abilities.StatHandler;

namespace Abilities {

    public class StatModifier {
        public float healthPercentModifier = 1f;
        public float healthFlatModifier = 0f;
        public float maxHealthPercentModifier = 1f;
        public float maxHealthFlatModifier = 0f;
        //speed
            //armor
            //damage
            //etc.
    }

    public static class AbilityManagerExtensions { 
        /// <summary>
        /// For HUMANS only. Called every tick.
        /// </summary> //TODO: Rename this method.
        public static void SysTickAndAbilityHandler(this NetworkedEntity entity, List<(IAbility ability, bool isActivated)> abilities) {
            abilities.UpdateAbilityList();
            if (entity == null) return;
            StatModifier stats = new StatModifier();
            

            for(int i = 0; i < abilities.Count; i++) {
                IAbility a = abilities[i].ability;
                if (a is ISysTickable) { // SysTick Callback.
                    ((ISysTickable)a).SysTickCall();
                }

                /*======================| Abilities |======================*/

                switch(a) {
                    //==== HEALING ====//
                    case Heal:
                        if (((Heal)a).GetIsActive()) { 
                            stats.healthFlatModifier += ((Heal)a).healAmount * entity.GetEntity().GetMaxHealth() * Time.deltaTime / ((Heal)a).healDuration; 
                        } break;
                    case AreaHeal:
                        if (((AreaHeal)a).GetIsActive()) {
                            stats.healthFlatModifier += ((AreaHeal)a).healAmount * entity.GetEntity().GetMaxHealth() * Time.deltaTime / ((AreaHeal)a).healPeriod; 
                        } break;
                    case InfiHeal:
                        if (((InfiHeal)a).GetIsActive()) {
                            stats.healthFlatModifier += ((InfiHeal)a).healPerSec * Time.deltaTime;
                        } break;
                    case HPSteal:
                        if (((HPSteal)a).GetIsActive()) {
                            stats.healthFlatModifier += ((HPSteal)a).totalHPStolen * Time.deltaTime;
                            ((HPSteal)a).totalHPStolen -= ((HPSteal)a).totalHPStolen * Time.deltaTime;
                        } break;
                    
                    //==== DAMAGE ====//
                    case RapidFire:
                        if (((RapidFire)a).GetIsActive()) { 
                            NetworkedEntity.playerInstance.OverClockNetworkEntityCall();    
                        } break;
                        
                }

                //stats.healthFlatModifier += 1000 * Time.deltaTime;

                //entity.GetTotalDmgDealt() to determine damage charging!

                /*======================| Ability Callbacks |======================*/

                if (a is IActivatable) {
#if UNITY_EDITOR
                    if (Input.GetKeyDown(KeyCode.LeftShift)) {
                        ((IActivatable)a).Activate(entity, true); 
                        abilities[i] = (a, false); 
                    }
#endif
                    if (abilities[i].isActivated) { 
                        ((IActivatable)a).Activate(entity); 
                        abilities[i] = (a, false); 
                    }
                }
		    } 
            //Run this to the upgrade stats < 
            //stats.UpgradeStatChanges()

            //Apply stat values.
            entity.ApplyStatChanges(stats);
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
