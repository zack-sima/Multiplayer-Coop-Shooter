using System.Collections.Generic;
using UnityEngine;
using CSVParser.Init;

namespace Abilities.UpgradeHandler {
    
    public static partial class UpgradeHandler {

        public static void PushToUpgradeHandler(this Dictionary<string, UpgradesCatalog.UpgradeNode> dict, UpgradesCatalog.UpgradeNode n) {
            // * networkedentity.playerinstance.getturret. < (projectile & turret changes)
            if (dict.ContainsKey(n.GetUpgradeId())) { Debug.LogWarning("Upgrade : " + n.GetUpgradeId() + " : already exists"); return; }
            dict.PushUpgrade(n);
        }

        private static void PushUpgrade(this Dictionary<string, UpgradesCatalog.UpgradeNode> dict, UpgradesCatalog.UpgradeNode n) {
            // * PlayerInfo.instance. ... For stat changes < 
            if (n == null || n.info == null) { Debug.LogError("Upgrade Null"); return; }
            switch(n.upgradeName) {
                
                #region //?==| GENERAL.CSV |==?//

                //*?=======================| Stat |=======================?*//
                // * ANYTHING SINGLE-FRAME BASED
                case "Braced Internals": {
                    if (n.info.TryGetModi(nameof(ModiName.MaxHP), out float maxHp)) {
                        NetworkedEntity.playerInstance.GetEntity().SetMaxHealth(NetworkedEntity.playerInstance.GetEntity().GetBaseHealth() 
                            * maxHp + NetworkedEntity.playerInstance.GetEntity().GetMaxHealth());
                    }
                    break; }
                case "Dynamic Loader": {
                    if (n.info.TryGetModi(nameof(ModiName.Reload), out float reload) && n.info.TryGetModi(nameof(ModiName.AmmoRegen), out float ammoRegen) && TryGetTurret(out Turret turret)) {
                        turret.SetShootSpeed(reload * turret.GetBaseShootSpeed() + turret.GetShootSpeed());
                        turret.SetAmmoRegenRate(ammoRegen * turret.GetBaseAmmoRegenRate() + turret.GetAmmoRegenSpeed());
                    }
                    break; }
                case "Fire Control System":
                    // Add code for Fire Control System here
                    break;
                case "Hardened Ammo": {
                    if (n.info.TryGetModi(nameof(ModiName.Dmg), out float dmgModi) && TryGetTurret(out Turret turret)) {
                        turret.SetBulletDmgModi(dmgModi += turret.GetBulletModi());
                    }
                break; }
                case "Hardened Armor": {
                    if (n.info.TryGetModi(nameof(ModiName.MaxHP), out float maxHp)) {
                        NetworkedEntity.playerInstance.GetEntity().SetMaxHealth(NetworkedEntity.playerInstance.GetEntity().GetBaseHealth() 
                            * maxHp + NetworkedEntity.playerInstance.GetEntity().GetMaxHealth());
                    }
                    break; }
                case "Improved Optics": {
                    if (n.info.TryGetModi(nameof(ModiName.CritChance), out float critChance) 
                            && n.info.TryGetModi(nameof(ModiName.CritDmg), out float critDmg) 
                            && TryGetTurret(out Turret lsTurret)) {
                        (float chance, float dmg) vals = lsTurret.GetCritValues();
                        lsTurret.SetCritChance(vals.chance += critChance);
                        lsTurret.SetCritDamage(critDmg);
                    }
                break; }
                case "Laser Sight": {
                    if (n.info.TryGetModi(nameof(ModiName.CritChance), out float critChance) 
                            && n.info.TryGetModi(nameof(ModiName.CritDmg), out float critDmg) 
                            && TryGetTurret(out Turret lsTurret)) {
                        (float chance, float dmg) vals = lsTurret.GetCritValues();
                        lsTurret.SetCritChance(vals.chance += critChance);
                        lsTurret.SetCritDamage(critDmg);
                    }
                    break; }
                case "Polished Trigger": {
                    if (n.info.TryGetModi(nameof(ModiName.CritChance), out float critChance) 
                            && n.info.TryGetModi(nameof(ModiName.CritDmg), out float critDmg) 
                            && TryGetTurret(out Turret lsTurret)) {
                        (float chance, float dmg) vals = lsTurret.GetCritValues();
                        lsTurret.SetCritChance(vals.chance += critChance);
                        lsTurret.SetCritDamage(critDmg);
                    }
                    if (n.info.TryGetModi(nameof(ModiName.Dmg), out float dmgModi) && TryGetTurret(out Turret turret)) {
                        turret.SetBulletDmgModi(dmgModi += turret.GetBulletModi());
                    }
                break; }

                //*?=======================| PAbility |=======================?*//
                // * ANYTHING PER-FRAME BASE
                case "Advanced Targeting":

                    break;
                case "Embracing Bind":
                
                    break;
                case "Emergency Overclock":
                
                    break;
                case "Exposed Inductor":
                
                    break;
                case "Gas Canister":
                
                    break;
                case "Lighter Fluid":
                
                    break;
                case "Optical Battery":
                
                    break;
                case "R.T.G":
                
                    break;
                case "Regenerative Armor":
                    if (n.info.TryGetModi(nameof(ModiName.Misc1), out float RAoutput)) {
                        PlayerInfo.instance.GetAbilityList().Add((new RegenerativeArmor(RAoutput), false)); }
                    break;
                case "Rocket Pods":
                
                    break;
                case "Scavengers Eye":
                
                    break;
                case "Sticky Bomb":
                
                    break;
                case "Stun Grenades":
                
                    break;
                case "Tesla Pack":
                
                    break;

                 //*?=======================| Projectile |=======================?*//
                    //TODO: Projectile changes, such as burning rounds, explosive, etc.

                #endregion

                //TODO: Place other trees here.
                //...

                default: 
                    Debug.LogWarning("Ability " + n.GetUpgradeId() + " : does NOT exist in handler. Unable to pull Upgrade.");
                    return;
            }
            dict.Add(n.GetUpgradeId(), n);
        }

        private static bool TryGetCombatEntity(out CombatEntity c) {
            c = NetworkedEntity.playerInstance.GetCombatEntity();
            if (c == null) return false;
            else return true;
        }

        private static bool TryGetTurret(out Turret t) {
            TryGetCombatEntity(out CombatEntity c);
            if (c == null) { t = null; return false; }
            t = c.GetTurret();
            if (t == null) return false;
            return true;
        }
    }

}
