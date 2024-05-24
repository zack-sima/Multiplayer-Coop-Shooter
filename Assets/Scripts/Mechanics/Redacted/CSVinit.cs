// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using Abilities.UpgradeHandler;
// using System;

// namespace CSV {

//     public enum ModiName {
//         Cost, Lvls, DropRate, Dmg, Reload, AmmoRegen, MaxHP, MoveSpd, CritChance, CritDmg, Cooldown,
//         Misc1, Misc2, Misc3,
//         Null
//     }  

//     public static class CSVParserInitExtensions {

//         #region Upgrades CSV

//         /// <summary>
//         /// Init Upgrade stats and stuff from corresponding CSVs!
//         /// * IMPORTANT: ASSOCIATED ATTRIBUTES:
//         /// [Special] < Used in ---.Props.CSV && .Descriptions.CSV to skip line.
//         /// [Soft] < Used in ---.Dependencies.CSV to mark as Soft Requirements.
//         /// [Hard] < Used in ---.Dependencies.CSV to mark as Hard Requirements.
//         /// [Mutual] < Used in ---.Dependencies.CSV to mark as Mutual Requirements.
//         /// </summary>
//         public static void ParseUpgradesFromAllCSV(this UpgradesCatalog catalog) {
//             Dictionary<string, UpgradeInfo> dict = new();

//             //GrabGeneral CSVs
//             Dictionary<string, string> generalCSVs = UpgradesCatalog.instance.csvStorage.GetCSVs();
//             if (generalCSVs.TryGetValue("General.Props", out string props) 
//                     && generalCSVs.TryGetValue("General.Dependencies", out string dependencies)
//                     && generalCSVs.TryGetValue("General.Descriptions", out string descriptions)) {
//                 dict.InitGeneralCSV(props, dependencies, descriptions);
//             }

//             //TODO: Parse based on turret, hull, abilty selection, etc!
        
//         }

//         private static void InitGeneralCSV(this Dictionary<string, UpgradeInfo> dict, string props, string dependencies, string descriptions) {
            
//             string[] rows = props.Split(new char[] { '\n' }); //Init headers and rows.
//             string[] rawHeaders = rows[0].Split(',');
            
//             List<string> headers = new(); 
//             for(int i = 0; i < rawHeaders.Length; i++) { //Init headers
//                 if (rawHeaders[i] == "") {
//                     headers.Add(nameof(ModiName.Null));
//                 } else headers.Add(rawHeaders[i]);
//             }

//             /*==============| PROPERTY INITS |==============*/
//             bool hasHeaderAppeared = false;
//             for(int i = 0; i < rows.Length; i++) { 

//                 if (!hasHeaderAppeared || rows[i] == "[Header]") {  // Check for [Header] Attribute
//                     hasHeaderAppeared = true;
//                     continue;
//                 }

//                 string[] modi = rows[i].Split(',');
//                 if (modi.Length < 2) continue;
//                 if (modi[0] == "" || modi[0] == "[Special]") continue; // Filter out empties or [Special]s
//                 if (modi[1] == "") continue; // Add? Check

//                 UpgradeInfo tempUpgrade = new UpgradeInfo(modi[0]);
//                 for(int j = 1; j < modi.Length; j++) {
//                     if (modi[j] == "") continue;
                    
//                     if (float.TryParse(modi[j], out float result)) { 
//                         if (headers.Count > j) {
//                             if (headers[j] == nameof(ModiName.Null)) { continue; } // No header
//                             tempUpgrade.PushModi(headers[j], result); //push modi onto tempUpgrade
//                         }
//                     }
//                 }
//                 dict.Add(modi[0], tempUpgrade);
//             }

//             /*==============| DEPENDENCY INITS |==============*/
//             rows = dependencies.Split(new char[] { '\n' });
//             string lastHeader = "";
//             for(int i = 0; i < rows.Length; i++) {
//                 string[] depens = rows[i].Split(',');
//                 string targetId = "";
//                 for (int j = 0; j < depens.Length; j++) {
//                     if (depens[j].Contains("[Soft]")) { lastHeader = "[Soft]"; break; }
//                     else if (depens[j].Contains("[Hard]")) { lastHeader = "[Hard]"; break; }
//                     else if (depens[j].Contains("[Mutual]")) { lastHeader = "[Mutual]"; break;}

//                     if (j == 0 && depens[j] == "") continue; // Empty checks
//                     else if (depens[j] == "") continue;

//                     if (j == 0) { targetId = depens[j]; continue; } // Target init

//                     if (dict.TryGetValue(targetId, out UpgradeInfo target) && dict.ContainsKey(depens[j])) {
//                         if (lastHeader == "[Soft]") target.softRequirements.Add(depens[j]);
//                         else if (lastHeader == "[Hard]") target.hardRequirements.Add(depens[j]);
//                         else if (lastHeader == "[Mutual]") target.mutualRequirements.Add(depens[j]);
//                     }
//                 }
//             }

//             /*==============| DESCRIPTION INITS |==============*/
//             rows = descriptions.Split(new char[] { '\n' });
//             for (int i = 1; i < rows.Length; i++) { // SKIP the first line.
//                 string[] descripts = rows[i].Split(',');
//                 if (descripts.Length <= 0) break; // i dont think this is possible ? but whatever
//                 if (descripts[0] == "" || descripts[0] == "[Special]") continue;

//                 if (dict.TryGetValue(descripts[0], out UpgradeInfo target)) {
//                     if (descripts.Length > 1) { target.description = descripts[1]; }
//                     //if (descripts.Length > 2) { target.type = descripts[2]; }
//                 }
//             }

//             /*==============| CATALOG INITS |==============*/
//             foreach(KeyValuePair<string, UpgradeInfo> pair in dict) {
//                 if (pair.Value.TryGetModi("Cost", out float cost) && pair.Value.TryGetModi("Lvls", out float lvl)) {
//                     if (lvl <= 1) { // Only one upgrade of it.
//                         UpgradesCatalog.instance.AddUpgrade(pair.Key, cost: (int)cost, info: pair.Value,
//                             softRequirements: pair.Value.softRequirements.StackDuplicateDependencies(dict),
//                             hardRequirements: pair.Value.hardRequirements.StackDuplicateDependencies(dict),
//                             mutuallyExclusiveUpgrades: pair.Value.mutualRequirements.StackDuplicateDependencies(dict));
//                         continue;
//                     }
//                     UpgradesCatalog.UpgradeNode priorNode = UpgradesCatalog.instance.AddUpgrade(pair.Key, cost: (int)cost,
//                         info: pair.Value,level: 1, 
//                         softRequirements: pair.Value.softRequirements.StackDuplicateDependencies(dict),
//                         hardRequirements: pair.Value.hardRequirements.StackDuplicateDependencies(dict),
//                         mutuallyExclusiveUpgrades: pair.Value.mutualRequirements.StackDuplicateDependencies(dict));
//                     for(int i = 2; i < lvl + 1; i++) {
//                         List<string> hards = pair.Value.hardRequirements.StackDuplicateDependencies(dict);
//                         hards.Add(priorNode.GetUpgradeId());
//                         UpgradesCatalog.UpgradeNode currentNode = UpgradesCatalog.instance.AddUpgrade(
//                             pair.Key, cost: (int)(cost * i), info: pair.Value, level: i, replacePrior: true,
//                             softRequirements: pair.Value.softRequirements.StackDuplicateDependencies(dict),
//                             hardRequirements: hards,
//                             mutuallyExclusiveUpgrades: pair.Value.mutualRequirements.StackDuplicateDependencies(dict));
//                         currentNode.prior = priorNode;
//                         priorNode = currentNode;
//                     }
//                 } 
//             }

//             // ? Debug Logging abilities. ?
//             // foreach(KeyValuePair<string, UpgradeInfo> s in dict) { 
//             //     Debug.LogWarning(s.Key + s.Value.ToString());
//             // }
//         }

//         #endregion

//         #region GarageInfo Parsing
        
//         public static void ParseTurretInfos(this Dictionary<string, GarageInfo> turrets, string props) {

//             var lines = props.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
//             GarageInfo currentTurret = null;

//             foreach (var line in lines) {
//                 var cols = line.Split(',');
//                 if (cols[0].StartsWith("[TurretId]")) {
//                     string turretName = cols[1].Trim();
//                     if (!string.IsNullOrEmpty(turretName)) {
//                         currentTurret = new GarageInfo(turretName);
//                         turrets[turretName] = currentTurret;
//                     }
//                 } else if (int.TryParse(cols[0], out int level)) {
//                     if (cols[1] == "[Max]") currentTurret?.SetIsMax(level); // Max level
//                     float.TryParse(cols[1], out float cost);
//                     float.TryParse(cols[2], out float damage);
//                     float.TryParse(cols[3], out float fireRate);
//                     float.TryParse(cols[4], out float maxAmmo);
//                     currentTurret?.PushModi("Cost", cost, level);
//                     currentTurret?.PushModi("Damage", damage, level);
//                     currentTurret?.PushModi("FireRate", fireRate, level);
//                     currentTurret?.PushModi("MaxAmmo", maxAmmo, level);
//                 }
//             }
//         }

//         public static void ParseHullInfos(this Dictionary<string, GarageInfo> hulls, string props) {
//             var lines = props.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
//             GarageInfo currentHull = null;

//             foreach (var line in lines) {
//                 var cols = line.Split(',');
//                 if (cols[0].StartsWith("[HullId]")) {
//                     string hullName = cols[1].Trim();
//                     if (!string.IsNullOrEmpty(hullName)) {
//                         currentHull = new GarageInfo(hullName);
//                         hulls[hullName] = currentHull;
//                     }
//                 } else if (int.TryParse(cols[0], out int level)) {
//                     if (cols[1] == "[Max]") currentHull?.SetIsMax(level); // Max level
//                     float.TryParse(cols[1], out float cost);
//                     float.TryParse(cols[2], out float health);
//                     float.TryParse(cols[3], out float movement);
//                     currentHull?.PushModi("Cost", cost, level);
//                     currentHull?.PushModi("Health", health, level);
//                     currentHull?.PushModi("Movement", movement, level);
//                 }
//             }
//         }

//         #endregion

//         private static List<string> StackDuplicateDependencies(this List<string> depens, Dictionary<string, UpgradeInfo> dict) {
//             List<string> temp = new(depens);
//             foreach(string s in temp) {
//                 int dupeCount = 0;
//                 for(int i = 0; i < depens.Count; i++) {
//                     if (depens[i] == s) { 
//                         dupeCount++;
//                         depens.RemoveAt(i);
//                         i--;
//                     }
//                 }
//                 if (dict.TryGetValue(s, out UpgradeInfo value) && value.TryGetModi(nameof(ModiName.Lvls), out float level)) {
//                     if ((int)level > 0) { depens.Add(s + " " + UpgradesCatalog.ToRoman(dupeCount)); }
//                 } else depens.Add(s);
//             }
//             if (depens.Count <= 0) return new();
//             return depens;
//         }

//         //TODO: Upgrade stat parser to apply said upgrades.
//         //1.5 dmg% == 150% == (1.5 + 1) * value 
//         // Need a dict of upgrades!
//     }
// }
