using System.Collections.Generic;
using UnityEngine;
using CSV;
using Abilities;

namespace Handlers {
    
    #region Nested

    public class StatModifier {
        public static StatModifier operator +(StatModifier a, StatModifier b) {
            StatModifier c = new() {
                healthPercentModifier = a.healthPercentModifier + b.healthPercentModifier,
                healthFlatModifier = a.healthFlatModifier + b.healthFlatModifier,

                baseHealthPercentModifier = a.baseHealthPercentModifier + b.baseHealthPercentModifier,
                baseHealthFlatModifier = a.baseHealthFlatModifier + b.baseHealthFlatModifier
            }; 
            return c;
        }
        public float healthPercentModifier = 0;
        public float healthFlatModifier = 0;
        public float baseHealthPercentModifier = 0; // Single frame
        public float baseHealthFlatModifier = 0; // Single frame
        public float reloadTimePercentModifier = 0;
    }

    public enum InflictionType {
        FlatHP,
        PercentHP
    }

    #endregion

    #region AbilityHandler

    public static class AbilityHandler { 
        /// <summary>
        /// For NetworkedEntities. Called every tick.
        /// </summary> 
        public static void SysTickStatHandler(this NetworkedEntity entity, List<(IAbility ability, bool isActivated)> abilities) {
            
            if (entity == null) return;
            
            for(int i = 0; i < abilities.Count; i++) {

                IAbility a = abilities[i].ability;

                // * SYSTICK CALL * //
                if (a is ISysTickable) { 
                    ((ISysTickable)a).SysTickCall(entity);
                } 

                //entity.GetTotalDmgDealt() to determine damage charging!

                // * ABILITY ACTIVATION * //
                if (a is IActivatable) {
                    if (abilities[i].isActivated) { 
                        ((IActivatable)a).Activate(entity); 
                        abilities[i] = (a, false); 
                    }
#if UNITY_EDITOR
                    // * CHEATS *
                    if (Input.GetKey(KeyCode.Minus) && entity == NetworkedEntity.playerInstance) {
                        ((IActivatable)a).Activate(entity, true); 
                        abilities[i] = (a, false); 
                    }
                    if (Input.GetKeyDown(KeyCode.LeftShift) && entity == NetworkedEntity.playerInstance) {
                        ((IActivatable)a).Activate(entity, true); 
                        abilities[i] = (a, false); 
                    }
#endif
                    
                }
            }
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

    #endregion

    #region AIAbilityHandler

    public static class AIAbilityHandler {
        //TODO: Add whatever parameters you need here to control what goes on what type of entity.
        public static void UpdateAIAbilityList(this List<(IAbility, bool)> input) {

            input.Clear(); // Remove all current abilties.

            Heal h = new Heal(CSVId.HealActive); // Initialize new ability.
            //TODO: idk if you need like a CSV for these bots to have different stats.

            input.Add((h, false)); // bool is for activation state.

            // entity.GetNetworker().PushAIAbilityActivation(1); < call to activate ability 2 (Heal) on NetworkEntity.
        }
    }

    #endregion

    #region InflictionHandler

    public static class InflictionHandler {
        public static void InflictionHandlerSysTick(this NetworkedEntity target, List<(InflictionType type, float param, float time)> inflicts) {
            //clean the list of inflictions.
            //Debug.Log("Systick ");
            for(int i = 0; i < inflicts.Count; i++) {
                if (inflicts[i].time - Time.deltaTime < 0) {
                    inflicts.RemoveAt(i);
                    i--;
                    continue;
                } 
                inflicts[i] = (inflicts[i].type, inflicts[i].param, inflicts[i].time - Time.deltaTime);
                target.InvokeInflictionHandler(inflicts[i].type, inflicts[i].param);
            }
        }

        public static void InvokeInflictionHandler(this NetworkedEntity target, InflictionType type, float param) {
            //Debug.Log("Invoke");
            switch(type) {
                case InflictionType.FlatHP:
                    //Debug.Log("FLATHP " + param);
                    if (param < 0) break;
                    target.HealthFlatNetworkEntityCall(param * Time.deltaTime);
                    break;
                case InflictionType.PercentHP:
                    if (param < 0) break;
                    target.HealthPercentNetworkEntityCall(param * Time.deltaTime);
                    break;
                
            }
        }

        public static void InitInfliction(this List<(InflictionType t, float, float)> inflicts, 
                InflictionType type, float param, float time) {
            for(int i = 0; i < inflicts.Count; i++) {
                //Debug.Log("Infliction Updated");
                if (inflicts[i].t == type) {
                    inflicts[i] = (inflicts[i].t, param, time);
                    return;
                }
            }
            //Debug.Log("Infliction Added");
            inflicts.Add((type, param, time));
        }
    }

    #endregion

    #region UpgradeHandler

    public static class UpgradeHandler {

        public static void PushToUpgradeHandler(this Dictionary<string, UpgradesCatalog.UpgradeNode> dict, UpgradesCatalog.UpgradeNode n) {
            // * networkedentity.playerinstance.getturret. < (projectile & turret changes)
            if (dict.ContainsKey(n.GetUpgradeId())) { Debug.LogWarning("Upgrade : " + n.GetUpgradeId() + " : already exists"); return; }
            dict.PushUpgrade(n);
        }

        private static void PushUpgrade(this Dictionary<string, UpgradesCatalog.UpgradeNode> dict, UpgradesCatalog.UpgradeNode n) {
            // * PlayerInfo.instance. ... For stat changes < 
            if (n == null || n.info == null) { Debug.LogError("Upgrade Null"); return; }

            foreach((IAbility i, bool b) in NetworkedEntity.playerInstance.GetAbilityList()) {
                if (i.TryPushUpgrade(n.upgradeName, n.info)) {
                    dict.Add(n.GetUpgradeId(), n);
                    return;
                }
            }

            switch(n.upgradeName) {
                // ? For one time stat change stuff. Gadgets and stuff.
                case nameof(CSVId.BracedInternalsGadget) + "BracedInternals": {
                    if (n.info.TryGetModi(nameof(CSVMd.MaxHP), out double maxHp)) {
                        NetworkedEntity.playerInstance.GetEntity().SetMaxHealth(NetworkedEntity.playerInstance.GetEntity().GetBaseHealth() 
                            * (float)maxHp + NetworkedEntity.playerInstance.GetEntity().GetMaxHealth());
                        NetworkedEntity.playerInstance.GetEntity().SetCurrentHealth(NetworkedEntity.playerInstance.GetEntity().GetBaseHealth() 
                            * (float)maxHp + NetworkedEntity.playerInstance.GetEntity().GetHealth());
                    }
                    break; }
                case nameof(CSVId.ImprovedLoaderGadget) + "ImprovedLoader": {
                    if (n.info.TryGetModi(nameof(CSVMd.Reload), out double reload) && n.info.TryGetModi(nameof(CSVMd.AmmoRegen), out double ammoRegen) && TryGetTurret(out Turret turret)) {
                        turret.SetShootSpeed(reload * turret.GetBaseShootSpeed() + turret.GetShootSpeed());
                        turret.SetAmmoRegenRate((float)ammoRegen * turret.GetBaseAmmoRegenRate() + turret.GetAmmoRegenSpeed());
                    }
                    break; }
                case nameof(CSVId.HardenedAmmoGadget) + "HardenedAmmo": {
                    if (n.info.TryGetModi(nameof(CSVMd.Damage), out double dmgModi) && TryGetTurret(out Turret turret)) {
                        turret.SetBulletDmgModi(dmgModi += turret.GetBulletModi());
                    }
                break; }
                case nameof(CSVId.HardenedArmorGadget) + "HardenedArmor": {
                    if (n.info.TryGetModi(nameof(CSVMd.MaxHP), out double maxHp)) {
                        NetworkedEntity.playerInstance.GetEntity().SetMaxHealth(NetworkedEntity.playerInstance.GetEntity().GetBaseHealth() 
                            * (float)maxHp + NetworkedEntity.playerInstance.GetEntity().GetMaxHealth());
                        NetworkedEntity.playerInstance.GetEntity().SetCurrentHealth(NetworkedEntity.playerInstance.GetEntity().GetBaseHealth() 
                            * (float)maxHp + NetworkedEntity.playerInstance.GetEntity().GetHealth());
                    }
                    break; }
                case nameof(CSVId.LaserSightGadget) + "LaserSight": {
                    if (n.info.TryGetModi(nameof(CSVMd.CritChance), out double critChance) 
                            && n.info.TryGetModi(nameof(CSVMd.CritDamage), out double critDmg) 
                            && TryGetTurret(out Turret lsTurret)) {
                        (float chance, float dmg) vals = lsTurret.GetCritValues();
                        lsTurret.SetCritChance(vals.chance += (float)critChance);
                        lsTurret.SetCritDamage((float)critDmg + vals.dmg);
                    }
                    break; }
                case nameof(CSVId.FireControlGadget) + "FireControl": {
                    if (n.info.TryGetModi(nameof(CSVMd.Reload), out double reload) && TryGetTurret(out Turret turret)) {
                        turret.SetShootSpeed(reload * turret.GetBaseShootSpeed() + turret.GetShootSpeed());
                    }
                    break; }
                case nameof(CSVId.PolishedTriggerGadget) + "PolishedTrigger": {
                    if (n.info.TryGetModi(nameof(CSVMd.CritChance), out double critChance) 
                            && TryGetTurret(out Turret lsTurret)) {
                        (float chance, float dmg) vals = lsTurret.GetCritValues();
                        lsTurret.SetCritChance(vals.chance += (float)critChance);
                    }
                    if (n.info.TryGetModi(nameof(CSVMd.Damage), out double dmgModi) && TryGetTurret(out Turret turret)) {
                        turret.SetBulletDmgModi(dmgModi += turret.GetBulletModi());
                    }
                break; }
                default: 
                    Debug.LogWarning("Ability " + n.GetUpgradeId() + " : does NOT exist in handler. Unable to pull Upgrade.");
                    return;
            }
            dict.Add(n.GetUpgradeId(), n);
        }
        public static bool TryGetActive(CSVId id, out IAbility a) {
            var list = NetworkedEntity.playerInstance.GetAbilityList();
            if (list == null) { a = null; return false; }
            foreach(var i in list) {
                if (i.Item1.GetId() == id.ToString()) {
                    a = i.Item1;
                    return true;
                }
            }
            a = null; return false;
        }

        public static bool TryGetCombatEntity(out CombatEntity c) {
            c = NetworkedEntity.playerInstance.GetCombatEntity();
            if (c == null) return false;
            else return true;
        }

        public static bool TryGetTurret(out Turret t) {
            TryGetCombatEntity(out CombatEntity c);
            if (c == null) { t = null; return false; }
            t = c.GetTurret();
            if (t == null) return false;
            return true;
        }
    }

    #endregion

    #region StatHandler

    public static class StatHandler {
        /// <summary>
        /// Call ONCE per frame by NetworkedEntity on LateUpdate after all stats have been applied to the modifier.
        /// </summary>
        public static void ApplyStatChanges(this NetworkedEntity entity, StatModifier stats) {

            stats.healthPercentModifier += 1f;

            entity.HealthPercentNetworkEntityCall(stats.healthPercentModifier);
            entity.HealthFlatNetworkEntityCall(stats.healthFlatModifier);
            
            entity.GetEntity().SetMaxHealth(stats.baseHealthPercentModifier * entity.GetEntity().GetBaseHealth() + stats.baseHealthFlatModifier + entity.GetEntity().GetMaxHealth());
        }
    }

    #endregion
}
