using System.Collections.Generic;
using UnityEngine;
using CSV.Parsers;

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

                #region //?==| ACTIVE ABILITIES |==?//

                case nameof(CSVId.HealActive) + "Faster Heals": {
                    if (n.info.TryGetModi(nameof(CSVMd.Cooldown), out double cooldown) && TryGetActive(CSVId.HealActive, out IAbility h)) {
                        ((Heal)h).cooldownPeriod -= (float)cooldown;
                    }
                    break; }

                case nameof(CSVId.HealActive) + "Bigger Heals": {
                    if (n.info.TryGetModi(nameof(CSVMd.HealAmount), out double healAmount) && TryGetActive(CSVId.HealActive, out IAbility h)) {
                        ((Heal)h).healAmount += (float)healAmount;
                    }
                    break; }

                case nameof(CSVId.RapidFireActive) + "Faster Fire": {
                    if (n.info.TryGetModi(nameof(CSVMd.Cooldown), out double cooldown) && TryGetActive(CSVId.RapidFireActive, out IAbility r)) {
                        ((RapidFire)r).cooldownPeriod -= (float)cooldown;
                    }
                    break; }

                case nameof(CSVId.SentryActive) + "Reinforced Sentry": {
                    if (n.info.TryGetModi(nameof(CSVMd.SentryHealth), out double maxHp) && TryGetActive(CSVId.SentryActive, out IAbility s)) {
                        ((Sentry)s).maxHealth += (int)maxHp;
                    }
                    break; }
                
                case nameof(CSVId.SentryActive) + "Larger Caliber": {
                    if (n.info.TryGetModi(nameof(CSVMd.SentryDamage), out double damage) && TryGetActive(CSVId.SentryActive, out IAbility s)) {
                        ((Sentry)s).dmgModi += (float)damage;
                    }
                    break; }

                #endregion

                #region //?==| ACTIVE GADGETS |==?//

                case nameof(CSVId.RegenerativeArmorGadget) + "Enhanced Healing": {
                    if (n.info.TryGetModi(nameof(CSVMd.HealAmount), out double hpPercentPerSec) && TryGetActive(CSVId.RegenerativeArmorGadget, out IAbility r)) {
                        ((RegenerativeArmorGadget)r).hpPercentPerSec += (float)hpPercentPerSec;
                    }
                    break; }
                
                #endregion
                #region //?==| PASSIVE GADGETS |==?//
                // //*?=======================| Stat |=======================?*//
                // // * ANYTHING SINGLE-FRAME BASED
                case nameof(CSVId.HardenedAmmoGadget) + "Hardened Ammo": {
                    if (n.info.TryGetModi(nameof(CSVMd.Damage), out double dmgModi) && TryGetTurret(out Turret turret)) {
                        turret.SetBulletDmgModi((float)dmgModi + turret.GetBulletModi());
                    }
                    break; }
                case nameof(CSVId.ImprovedLoaderGadget) + "Improved Loader": {
                    if (n.info.TryGetModi(nameof(CSVMd.Reload), out double reload) && n.info.TryGetModi(nameof(CSVMd.AmmoRegen), out double ammoRegen) && TryGetTurret(out Turret turret)) {
                        turret.SetShootSpeed((float)reload * turret.GetBaseShootSpeed() + turret.GetShootSpeed());
                        turret.SetAmmoRegenRate((float)ammoRegen * turret.GetBaseAmmoRegenRate() + turret.GetAmmoRegenSpeed());
                    }
                    break; }
                case nameof(CSVId.HardenedArmorGadget) + "Hardened Armor": {
                    if (n.info.TryGetModi(nameof(CSVMd.MaxHP), out double maxHp)) {
                        TryGetCombatEntity(out CombatEntity c);
                        c.SetMaxHealth((float)maxHp * c.GetBaseHealth());
                    }
                    break; }
                case nameof(CSVId.FireControlGadget) + "Fire Control": {
                    if (n.info.TryGetModi(nameof(CSVMd.Reload), out double reload) && TryGetTurret(out Turret turret)) {
                        turret.SetShootSpeed((float)reload * turret.GetBaseShootSpeed() + turret.GetShootSpeed());
                    }
                    break; }
                case nameof(CSVId.PolishedTriggerGadget) + "Polished Trigger": {
                    if (n.info.TryGetModi(nameof(CSVMd.CritChance), out double crit) && TryGetTurret(out Turret turret)) {
                        turret.SetCritChance((float)crit + turret.GetCritValues().Item1);
                    }
                    break; }
                case nameof(CSVId.LaserSightGadget) + "Laser Sight": {
                    if (n.info.TryGetModi(nameof(CSVMd.CritDamage), out double critDmg) && TryGetTurret(out Turret turret)) {
                        turret.SetCritDamage((float)critDmg + turret.GetCritValues().Item2);
                    }
                    break; }
                case nameof(CSVId.BracedInternalsGadget) + "Braced Internals": {
                    if (n.info.TryGetModi(nameof(CSVMd.MaxHP), out double maxHp)) {
                        TryGetCombatEntity(out CombatEntity c);
                        c.SetMaxHealth((float)maxHp * c.GetBaseHealth());
                    }
                    break; }
                #endregion
                // case "Braced Internals": {
                //     if (n.info.TryGetModi(nameof(ModiName.MaxHP), out float maxHp)) {
                //         NetworkedEntity.playerInstance.GetEntity().SetMaxHealth(NetworkedEntity.playerInstance.GetEntity().GetBaseHealth() 
                //             * maxHp + NetworkedEntity.playerInstance.GetEntity().GetMaxHealth());
                //     }
                //     break; }
                // case "Dynamic Loader": {
                //     if (n.info.TryGetModi(nameof(ModiName.Reload), out float reload) && n.info.TryGetModi(nameof(ModiName.AmmoRegen), out float ammoRegen) && TryGetTurret(out Turret turret)) {
                //         turret.SetShootSpeed(reload * turret.GetBaseShootSpeed() + turret.GetShootSpeed());
                //         turret.SetAmmoRegenRate(ammoRegen * turret.GetBaseAmmoRegenRate() + turret.GetAmmoRegenSpeed());
                //     }
                //     break; }
                // case "Fire Control System": {
                //     if (n.info.TryGetModi(nameof(ModiName.Dmg), out float dmgModi) && TryGetTurret(out Turret turret)) {
                //         turret.SetBulletDmgModi(dmgModi += turret.GetBulletModi());
                //     }
                //     break;}
                // case "Hardened Ammo": {
                //     if (n.info.TryGetModi(nameof(ModiName.Dmg), out float dmgModi) && TryGetTurret(out Turret turret)) {
                //         turret.SetBulletDmgModi(dmgModi += turret.GetBulletModi());
                //     }
                // break; }
                // case "Hardened Armor": {
                //     if (n.info.TryGetModi(nameof(ModiName.MaxHP), out float maxHp)) {
                //         NetworkedEntity.playerInstance.GetEntity().SetMaxHealth(NetworkedEntity.playerInstance.GetEntity().GetBaseHealth() 
                //             * maxHp + NetworkedEntity.playerInstance.GetEntity().GetMaxHealth());
                //         NetworkedEntity.playerInstance.GetEntity().SetCurrentHealth(NetworkedEntity.playerInstance.GetEntity().GetBaseHealth() 
                //             * maxHp + NetworkedEntity.playerInstance.GetEntity().GetHealth());
                //     }
                //     break; }
                // case "Improved Optics": {
                //     if (n.info.TryGetModi(nameof(ModiName.CritChance), out float critChance) 
                //             && n.info.TryGetModi(nameof(ModiName.CritDmg), out float critDmg) 
                //             && TryGetTurret(out Turret lsTurret)) {
                //         (float chance, float dmg) vals = lsTurret.GetCritValues();
                //         lsTurret.SetCritChance(vals.chance += critChance);
                //         lsTurret.SetCritDamage(critDmg);
                //     }
                // break; }
                // case "Laser Sight": {
                //     if (n.info.TryGetModi(nameof(ModiName.CritChance), out float critChance) 
                //             && n.info.TryGetModi(nameof(ModiName.CritDmg), out float critDmg) 
                //             && TryGetTurret(out Turret lsTurret)) {
                //         (float chance, float dmg) vals = lsTurret.GetCritValues();
                //         lsTurret.SetCritChance(vals.chance += critChance);
                //         lsTurret.SetCritDamage(critDmg);
                //     }
                //     break; }
                // case "Polished Trigger": {
                //     if (n.info.TryGetModi(nameof(ModiName.CritChance), out float critChance) 
                //             && n.info.TryGetModi(nameof(ModiName.CritDmg), out float critDmg) 
                //             && TryGetTurret(out Turret lsTurret)) {
                //         (float chance, float dmg) vals = lsTurret.GetCritValues();
                //         lsTurret.SetCritChance(vals.chance += critChance);
                //         lsTurret.SetCritDamage(critDmg);
                //     }
                //     if (n.info.TryGetModi(nameof(ModiName.Dmg), out float dmgModi) && TryGetTurret(out Turret turret)) {
                //         turret.SetBulletDmgModi(dmgModi += turret.GetBulletModi());
                //     }
                // break; }
                // // * ANYTHING PER-FRAME BASE
                // case "Advanced Targeting":

                //     break;
                // case "Embracing Bind":
                
                //     break;
                // case "Emergency Overclock":
                
                //     break;
                // case "Exposed Inductor":
                
                //     break;
                // case "Gas Canister":
                
                //     break;
                // case "Lighter Fluid":
                
                //     break;
                // case "Optical Battery":
                
                //     break;
                // case "R.T.G":
                
                //     break;
                // case "Regenerative Armor":
                //     if (n.info.TryGetModi(nameof(ModiName.Misc1), out float RAoutput)) {
                        // NetworkedEntity.playerInstance.GetAbilityList().Add((new RegenerativeArmor(RAoutput), false)); }
                //     break;
                // case "Rocket Pods":
                
                //     break;
                // case "Scavengers Eye":
                
                //     break;
                // case "Sticky Bomb":
                
                //     break;
                // case "Stun Grenades":
                
                //     break;
                // case "Tesla Pack":
                
                //     break;

                //  //*?=======================| Projectile |=======================?*//
                //     //TODO: Projectile changes, such as burning rounds, explosive, etc.

                // #endregion

                // //TODO: Place other trees here.
                // //...

                default: 
                    Debug.LogWarning("Ability " + n.GetUpgradeId() + " : does NOT exist in handler. Unable to pull Upgrade.");
                    return;
            }
            dict.Add(n.GetUpgradeId(), n);
        }

        private static bool TryGetActive(CSVId id, out IAbility a) {
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
