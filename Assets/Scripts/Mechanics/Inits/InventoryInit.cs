using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abilities;
using ExitGames.Client.Photon.StructWrapping;
using CSV.Parsers;

public static class InventoryInit {
    public static void UpdateInventory(this (List<(IAbility ability, bool isActivated)> actives, 
                Dictionary<string, UpgradesCatalog.UpgradeNode> upgrades) inventory) {
        

        Dictionary<string, Dictionary<string, int>> requested = PlayerDataHandler.instance.GetEquippedInfos();

        if (requested == null || requested.Count == 0) { 
            LogError("No items requested to be equipped.", "InventoryHandler");
            return; 
        }

        //*=======================| ACTIVES |=======================*//

        if (requested.ContainsKey(nameof(CSVType.ACTIVES))) {  
            Dictionary<string, int> activeInfos = requested[nameof(CSVType.ACTIVES)];

            foreach(KeyValuePair<string, int> a in activeInfos) { //active inits
                inventory.actives.InitActive(a.Key, a.Value);
            }

            
            //     input.Clear();
            //     Heal f = new();
            //     input.Add((f, false));
            //     RapidFire h = new();
            //     input.Add((h, false));

        } else {
            LogWarning("No actives requested to be equipped.", "InventoryHandler");
        }
        AbilityUIManagerExtensions.OnAbilityListChange();

        //*=======================| GADGETS |=======================*//

        if (requested.ContainsKey(nameof(CSVType.GADGETS))) {
            Dictionary<string, int> gadgetInfos = requested[nameof(CSVType.GADGETS)];

        } else {
            LogWarning("No gadgets requested to be equipped.", "InventoryHandler");
        }

        //turret and hull stat inits.
        
    }

    private static void LogWarning(string error, string debugId) {
        DebugUIManager.instance?.LogOutput(debugId + " : " + error);
        Debug.LogWarning(debugId + " : " + error);
    }

    private static void LogError(string error, string debugId) {
        DebugUIManager.instance?.LogOutput(debugId + " : " + error);
        Debug.LogError(debugId + " : " + error);
    }
}
