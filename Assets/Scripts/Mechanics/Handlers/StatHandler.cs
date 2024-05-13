using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Abilities.StatHandler;

namespace Abilities {

    /// <summary>
    /// All values are INCREMENTAL. INCLUDING PERCENTAGES. 
    /// Input of 0.05 to a % variable results in a 1.05 * targetStat;
    /// </summary>
    public class StatModifier {

        public static StatModifier operator +(StatModifier a, StatModifier b) {
            StatModifier c = new() {
                healthPercentModifier = a.healthPercentModifier + b.healthPercentModifier,
                healthFlatModifier = a.healthFlatModifier + b.healthFlatModifier,

                baseHealthPercentModifier = a.baseHealthPercentModifier + b.baseHealthPercentModifier,
                baseHealthFlatModifier = a.baseHealthFlatModifier + b.baseHealthFlatModifier
            };

            //// maxHealthPercentModifier = a.maxHealthPercentModifier + b.maxHealthPercentModifier;
            //// maxHealthFlatModifier = a.maxHealthFlatModifier + b.maxHealthFlatModifier;
            //Populate with more vars...

            return c;
        }

        public float healthPercentModifier = 0;
        public float healthFlatModifier = 0;
        
        public float baseHealthPercentModifier = 0; // Single frame
        public float baseHealthFlatModifier = 0; // Single frame

        public float reloadTimePercentModifier = 0;

        ////public float maxHealthPercentModifier = 1f;
        ////public float maxHealthFlatModifier = 0f;
        //speed
            //armor
            //damage
            //etc.
    }

    public static class StatHandlerExtensions { 
        /// <summary>
        /// For HUMANS only. Called every tick.
        /// </summary> //TODO: Rename this method.
        public static void SysTickStatHandler(this NetworkedEntity entity, List<(IAbility ability, bool isActivated)> abilities) {
            //TODO: Init abilities list from csv/persistent data.
            
            if (entity == null) return;
            if (entity == NetworkedEntity.playerInstance) abilities.UpdateAbilityList();
            StatModifier stats = new();
            
            for(int i = 0; i < abilities.Count; i++) {
                IAbility a = abilities[i].ability;
                if (a is ISysTickable) { // SysTick Callback.
                    ((ISysTickable)a).SysTickCall();
                } else if (a is IStatSysTickable) {
                    ((IStatSysTickable)a).SysTickCall(entity, stats);
                }

                //?======================| Abilities |======================?//

                switch(a) {
                    //?=~=~=~=~=| HEALING |=~=~=~=~=?//
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
                    
                    //?=~=~=~=~=| DAMAGE |=~=~=~=~=?//
                    case RapidFire:
                        if (((RapidFire)a).GetIsActive()) { 
                            entity.OverClockNetworkEntityCall();    
                        } break;
                        
                }

                //stats.healthFlatModifier += 1000 * Time.deltaTime;

                //entity.GetTotalDmgDealt() to determine damage charging!

                /*======================| Ability Callbacks |======================*/

                if (a is IActivatable) {
#if UNITY_EDITOR
                    if (Input.GetKey(KeyCode.Minus) && entity == NetworkedEntity.playerInstance) {
                        ((IActivatable)a).Activate(entity, true); 
                        abilities[i] = (a, false); 
                    }
                    if (Input.GetKeyDown(KeyCode.LeftShift) && entity == NetworkedEntity.playerInstance) {
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
            //Apply stat values.
            entity.PushStatChanges(stats);
        }

        public static void PushAbilityActivation(this List<(IAbility ability, bool isActivated)> abilities, int index) {
            if (abilities.Count <= index) { Debug.LogWarning("Index out of range for ability"); return;}
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
