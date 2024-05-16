using System.Collections;
using System.Collections.Generic;
using CSV.Parsers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abilities {
    public static class UpgradeInit {
        public static void InitUpgrades(this List<InGameUpgradeInfo> upgrades) {

            //TODO: Dependency stacking, though idk if its needed ...
            foreach(InGameUpgradeInfo i in upgrades) {
                if (i.TryGetModi(nameof(CSVMd.UPCost), out double upCost) && i.TryGetModi(nameof(CSVMd.Level), out double lvl)) {
                    if (lvl <= 1) { // Only one upgrade of it.
                        UpgradesCatalog.instance.AddUpgrade(id: i.id, displayName: i.displayName, cost: (int)upCost, info: i);
                        continue;
                    } else {
                        UpgradesCatalog.UpgradeNode priorNode = UpgradesCatalog.instance.AddUpgrade(id: i.id, displayName: i.displayName, cost: (int)upCost, info: i, level: 1);
                        for(int j = 2; j < lvl + 1; j++) {
                            List<string> hards = new() { priorNode.GetUpgradeId() };
                            UpgradesCatalog.UpgradeNode currentNode = UpgradesCatalog.instance.AddUpgrade(id: i.id, displayName: i.displayName, cost: (int)(upCost * j), 
                                    info: i, level: j, hardRequirements: hards);
                            currentNode.prior = priorNode;
                            priorNode = currentNode;
                        }
                    }
                } else {
                    DebugUIManager.instance?.LogError("Upgrade " + i.id + " has no cost or level.", "UpgradeInit");
                }
            }
            
            
        }

        //Old dependency stacking code.
        // foreach(KeyValuePair<string, UpgradeInfo> pair in dict) {
        //     if (pair.Value.TryGetModi("Cost", out float cost) && pair.Value.TryGetModi("Lvls", out float lvl)) {
        //         if (lvl <= 1) { // Only one upgrade of it.
        //             UpgradesCatalog.instance.AddUpgrade(pair.Key, cost: (int)cost, info: pair.Value,
        //                 softRequirements: pair.Value.softRequirements.StackDuplicateDependencies(dict),
        //                 hardRequirements: pair.Value.hardRequirements.StackDuplicateDependencies(dict),
        //                 mutuallyExclusiveUpgrades: pair.Value.mutualRequirements.StackDuplicateDependencies(dict));
        //             continue;
        //         }
        //         UpgradesCatalog.UpgradeNode priorNode = UpgradesCatalog.instance.AddUpgrade(pair.Key, cost: (int)cost,
        //             info: pair.Value,level: 1, 
        //             softRequirements: pair.Value.softRequirements.StackDuplicateDependencies(dict),
        //             hardRequirements: pair.Value.hardRequirements.StackDuplicateDependencies(dict),
        //             mutuallyExclusiveUpgrades: pair.Value.mutualRequirements.StackDuplicateDependencies(dict));
        //         for(int i = 2; i < lvl + 1; i++) {
        //             List<string> hards = pair.Value.hardRequirements.StackDuplicateDependencies(dict);
        //             hards.Add(priorNode.GetUpgradeId());
        //             UpgradesCatalog.UpgradeNode currentNode = UpgradesCatalog.instance.AddUpgrade(
        //                 pair.Key, cost: (int)(cost * i), info: pair.Value, level: i, replacePrior: true,
        //                 softRequirements: pair.Value.softRequirements.StackDuplicateDependencies(dict),
        //                 hardRequirements: hards,
        //                 mutuallyExclusiveUpgrades: pair.Value.mutualRequirements.StackDuplicateDependencies(dict));
        //             currentNode.prior = priorNode;
        //             priorNode = currentNode;
        //         }
        //     } 
        // }

        // private static List<string> StackDuplicateDependencies(this List<string> depens, Dictionary<string, UpgradeInfo> dict) {
        //     List<string> temp = new(depens);
        //     foreach(string s in temp) {
        //         int dupeCount = 0;
        //         for(int i = 0; i < depens.Count; i++) {
        //             if (depens[i] == s) { 
        //                 dupeCount++;
        //                 depens.RemoveAt(i);
        //                 i--;
        //             }
        //         }
        //         if (dict.TryGetValue(s, out UpgradeInfo value) && value.TryGetModi(nameof(ModiName.Lvls), out float level)) {
        //             if ((int)level > 0) { depens.Add(s + " " + UpgradesCatalog.ToRoman(dupeCount)); }
        //         } else depens.Add(s);
        //     }
        //     if (depens.Count <= 0) return new();
        //     return depens;
        // }

    }

}

