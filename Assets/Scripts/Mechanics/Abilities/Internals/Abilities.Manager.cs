using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Abilities {

    public static class AbilityManagerExtensions { 
        /// <summary>
        /// For HUMANS only. Called every tick.
        /// </summary>
        public static void SysTickAndAbilityHandler(this NetworkedEntity entity, List<(IAbility ability, bool isActivated)> abilities) {
            abilities.UpdateAbilityList();
            if (entity == null) return;
            float healthPercentModifier = 1f, healthFlatModifier = 0, maxHealthPercentModifier = 1f, maxHealthFlatModifier = 0;
            //speed
            //armor
            //damage
            //etc.

            for(int i = 0; i < abilities.Count; i++) {
                IAbility a = abilities[i].ability;
                if (a is ISysTickable) { // SysTick Callback.
                    ((ISysTickable)a).SysTickCall();
                }

                /*======================| Abilities |======================*/

                switch(a) {
                    //====HEALING====//
                    case Heal:
                        if (((Heal)a).GetIsActive()) { 
                            healthFlatModifier += ((Heal)a).healAmount * entity.GetEntity().GetMaxHealth() * Time.deltaTime / ((Heal)a).healDuration; 
                            //maxHealthFlatModifier += 100f * Time.deltaTime;      
                            //maxHealthFlatModifier += 100f * Time.deltaTime;
                            //heal entities around u
                            // foreach(CombatEntity e in EntityController.instance.GetCombatEntities()) {
                            //     Debug.Log("Pulling");
                            //     if (e.GetNetworker() == null) continue;
                            //     if (e.GetNetworker() == entity) continue;
                            //     Debug.Log("Applying");
                            //    //e.GetNetworker().HealthFlatNetworkEntityCall(500f * Time.deltaTime);
                            //     e.GetNetworker().LocalApplyInfliction(InflictionType.FlatHP, ((Heal)a).healAmount * entity.GetEntity().GetMaxHealth() / ((Heal)a).healDuration, ((Heal)a).healDuration);
                            //     e.GetNetworker().RPCApplyInfliction(InflictionType.FlatHP, ((Heal)a).healAmount * entity.GetEntity().GetMaxHealth() / ((Heal)a).healDuration, ((Heal)a).healDuration);
                    
                            //     //e.GetNetworker().HealthFlatNetworkEntityCall(((Heal)a).healAmount * entity.GetEntity().GetMaxHealth() * Time.deltaTime / ((Heal)a).healDuration);
                            // }
                        } break;
                    
                    case RapidFire:
                        if (((RapidFire)a).GetIsActive()) { 
                            NetworkedEntity.playerInstance.OverClockNetworkEntityCall();    
                        } break;
                        
                }

                //entity.GetTotalDmgDealt() to determine damage charging!

                /*======================| Ability Callbacks |======================*/

                if (a is IActivatable) {
                    if (abilities[i].isActivated) { ((IActivatable)a).Activate(entity); abilities[i] = (a, false); }
                }
                if (a is IPassiveable) {
                    
                    //apply passive buffs & changes
                }
		    } 

            // apply buffs and stuff. TODO: Infliction Handler for local player here.
            NetworkedEntity.playerInstance.HealthPercentNetworkEntityCall(healthPercentModifier);
            NetworkedEntity.playerInstance.HealthFlatNetworkEntityCall(healthFlatModifier);
    
            entity.GetEntity().SetMaxHealth(maxHealthPercentModifier * entity.GetEntity().GetMaxHealth());
            entity.GetEntity().SetMaxHealth(entity.GetEntity().GetMaxHealth() + maxHealthFlatModifier);
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
