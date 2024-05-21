// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace CSV {
//     #region Internal Classes

//     [System.Serializable]
//     public class GarageInfo {
//         private Dictionary<int, Dictionary<string, float>> modiLevels = new();
//         private List<string> modiIds = new();
//         public List<string> GetModiIds() { return modiIds; }
//         public string description { get; set; } = "";
//         public readonly string turretId;
//         private uint maxLevel = 1;
//         public uint currentLevel = 0;
//         public void SetIsMax(int b) { maxLevel = (uint)b; }
//         public uint GetIsMax() { return maxLevel; }
//         public GarageInfo(string id) { turretId = id; }

//         public Dictionary<string, float> GetCurrentStats(uint level) {
//             if (modiLevels.ContainsKey((int)level)) return modiLevels[(int)level];
//             else return null;
//         }

//         public void PushModi(string id, float input, int level) {
//             if (modiLevels.ContainsKey(level)) {
//                 if (modiLevels[level].ContainsKey(id)) modiLevels[level][id] = input;
//                 else modiLevels[level].Add(id, input);
//             } else {
//                 Dictionary<string, float> temp = new() {{id, input}};
//                 modiLevels.Add(level, temp);
//             }
//         }

//         public bool TryGetModi(string id, int level, out float output) {
//             output = 0;
//             if (modiLevels.ContainsKey(level)) {
//                 if (modiLevels[level].ContainsKey(id)) {
//                     output = modiLevels[level][id];
//                     return true;
//                 }
//             }
//             return false;
//         }

//         public override string ToString() {
//             string s = "";
//             foreach(KeyValuePair<int, Dictionary<string, float>> p in modiLevels) {
//                 s += " Level: " + p.Key + ";";
//                 foreach(KeyValuePair<string, float> p2 in p.Value) {
//                     s += " " + p2.Key + " : " + p2.Value + ";";
//                 }
//             }
//             s += " Description: " + description + ";";
//             return s;
//         }
//     }
//     [System.Serializable]
//     public class UpgradeInfo {
//         public enum ModiName { Cost, Damage, FireRate, MaxAmmo, Health, Movement, CritChance }
//         public enum UpgradeType { Active, Gadget }
//         private Dictionary<string, float> modi = new();
//         private List<string> modiIds = new(); 
//         public List<string> GetModiIds() { return modiIds; }
//         public string description { get; set; } = "";
//         public uint count = 1;
//         public readonly string upgradeId;
//         public UpgradeType type; // TODO: Make into a readonly 

//         public UpgradeInfo(string id) { upgradeId = id;} // TODO: Implement Type init.
        
//         public List<string> softRequirements = new(), hardRequirements = new(), mutualRequirements = new();

//         public void PushModi(string id, float input) {
//             if (modi.ContainsKey(id)) modi[id] = input;
//             else modi.Add(id, input);
//         }
        
//         public bool TryGetModi(string id, out float output) { 
//             output = 0;
//             if (modi.ContainsKey(id)) {
//                 output = modi[id];
//                 return true;
//             } 
//             return false;
//         }
//         public override string ToString() {
//             string s = "";
//             foreach(KeyValuePair<string, float> p in modi) {
//                 s += " " + p.Key + " : " + p.Value + ";";
//             }
//             s += " Description: " + description + ";";
//             return s;
//         }
//     }

//     #endregion
// }    
