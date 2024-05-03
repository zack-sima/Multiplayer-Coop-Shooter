using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSVParser.Init;
using System.Runtime.CompilerServices;

namespace Abilities.UpgradeHandler {
    
    public static class UpgradeHandler {

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
                case "Braced Internals":
                    if (n.info.TryGetModi(nameof(ModiName.MaxHP), out float BIoutput)) {
                        StatModifier temp = new StatModifier(true);
                        temp.baseHealthPercentModifier += BIoutput;
                        Debug.LogError("Internals Braced " + temp.baseHealthPercentModifier);
                        PlayerInfo.instance.SetStatModifier(temp);
                    }
                    break;
                case "Dynamic Loader":
                    // Add code for Dynamic Loader here
                    break;
                case "Fire Control System":
                    // Add code for Fire Control System here
                    break;
                case "Hardened Ammo":
                    // Add code for Hardened Ammo here
                    break;
                case "Hardened Armor":
                    // Add code for Hardened Armor here
                    break;
                case "Improved Optics":
                    // Add code for Improved Optics here
                    break;
                case "Laser Sight":
                    // Add code for Laser Sight here
                    break;
                case "Polished Trigger":
                    // Add code for Polished Trigger here
                    break;
                case "Turbocharger":
                    // Add code for Turbocharger here
                    break;

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
    }

}
