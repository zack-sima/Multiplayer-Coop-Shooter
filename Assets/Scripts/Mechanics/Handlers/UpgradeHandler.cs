using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities.UpgradeHandler {
    public static class UpgradeHandler {

        public static void PushToUpgradeHandler(this Dictionary<string, UpgradesCatalog.UpgradeNode> dict, UpgradesCatalog.UpgradeNode n) {
            // * networkedentity.playerinstance.getturret. < (projectile & turret changes)
		    // * Passive ability Init here...
            if (dict.ContainsKey(n.GetUpgradeId())) { Debug.LogWarning("Upgrade : " + n.GetUpgradeId() + " : already exists"); return; }
            
            if (n.info.type == "Stat") { 
                dict.Add(n.GetUpgradeId(), n);
                PushAsStat(n); 
                return;
            }
            
            if (n.info.type == "PAbility") { 
                dict.Add(n.GetUpgradeId(), n);
                PushAsPassiveAbility(n); 
                return;
            }

            if (n.info.type == "Projectile") {
                //TODO: Projectile changes, such as burning rounds, explosive, etc.
            }

            Debug.LogWarning("Upgrade : " + n.GetUpgradeId() + " : has NO valid type specified. Did you init .Descriptions.CSV [Type] correctly?");
        }

        private static void PushAsStat(UpgradesCatalog.UpgradeNode n) {
            // * PlayerInfo.instance. ... For stat changes < 
        }

        private static void PushAsPassiveAbility(UpgradesCatalog.UpgradeNode n) {
            // * PlayerInfo.instnace. ... Use Push ability callback to add abilities to the list.
        }
    }
}
