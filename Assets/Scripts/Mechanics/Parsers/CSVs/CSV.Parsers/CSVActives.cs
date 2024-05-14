using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using CSV;
using UnityEngine.Rendering;

namespace CSV.Parsers {
    
    public static partial class CSVParserExtensions {
        public static bool TryParse(this Dictionary<string, InventoryInfo> dict, string csv, bool debug) {

            string[] firstCut = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            string[] masterHeaders = firstCut[0].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            firstCut[0] = "";
            int row = 0;
            InventoryInfo info;
            List<string> tempHeaders;

            foreach(string s in firstCut) {
                string[] secondCut = s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (secondCut.Length < 1) continue;

                //Try Get the [stringId] of the item.
                { if (TryParseModi(secondCut[0], out string id)) {
                    if (id == nameof(CSVMd.ActiveId)) {

                        //Try Get actual stringId
                        if (!TryGetString(secondCut, 1, out string stringId)) { continue; }

                        tempHeaders = new();
                        info = new InventoryInfo(stringId);
                        
                        //Init tempHeaders
                        for(int i = 2; i < secondCut.Length; i++) {
                            if (TryParseModi(secondCut[i], out string modiId)) {
                                if (modiId == nameof(CSVMd.Above) && masterHeaders.Length > i)
                                    tempHeaders.Add(masterHeaders[i]);
                            } else tempHeaders.Add(modiId);
                        }

                        //Read levels and init modifiers.

                        //Read IUpgrades and init them.

                        //Push to dict.

                    }
                }}
                foreach(string s2 in secondCut) {

                    if (debug) Debug.Log($"Row {row}: {s2}");

                }
            }
            

            //[Active ID]
            //[Tags]

            //levels

            //[UPTags]

            //[IUpgrades]
            return false;
        }

        private static bool TryGetString(string[] input, int index, out string output) {
            output = "";
            if (input.Length > index) {
                output = input[index];
                return true;
            }
            return false;
        }
        private static void PushToDict(Dictionary<string, InventoryInfo> dict, string id, InventoryInfo info) {
            if (!dict.ContainsKey(id)) {
                dict.Add(id, info);
            } else {
                dict[id] = info;
            }
        }
        private static bool TryParseModi(string input, out string id) {
            id = "";
            string s = ExtractStringId(input)?.Replace(" ", "");
            if (s == null) return false;
            id = s;
            return true;
        }
        private static string ExtractStringId(string input) {
            var match = Regex.Match(input, @"\[(.*?)\]");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
    }
    [System.Serializable]
    public class InventoryInfo {
        public readonly string id;
        public string description;
        public int maxLevel;
        private Dictionary<(string, int), float> modiByLevel = new();
        private Dictionary<string, InGameUpgradeInfo> inGameUpgrades = new();
        private int currentLevel = 0; // 0 == locked.

        public InventoryInfo(string id) {
            this.id = id;
        }

        public bool CanUpgrade() { return currentLevel < maxLevel; }

        public void PushInventoryModi(string id, float input, int level) {
            if (modiByLevel.ContainsKey((id, level))) {
                modiByLevel[(id, level)] = input;
            } else {
                modiByLevel.Add((id, level), input);
            }
        }

        public void PushInGameUpgrade(InGameUpgradeInfo input) {
            if (!inGameUpgrades.ContainsKey(input.id)) inGameUpgrades.Add(input.id, input);
            else inGameUpgrades[input.id] = input;
        }

        public bool TryUpgrading() {
            if (CanUpgrade()) {
                currentLevel++;
                return true;
            }
            return false;
        }

        public bool TryGetModi(string id, int level, out float output) {
            output = 0;
            if (modiByLevel.ContainsKey((id, level))) {
                output = modiByLevel[(id, level)];
                return true;
            }
            return false;
        }
    }

    [System.Serializable]
    public class InGameUpgradeInfo {
        public readonly string id;
        public List<string> softRequirements = new(), hardRequirements = new(), mutualRequirements = new();
        public Dictionary<(string, int), float> modi = new();
        public int maxLevel = 1;
        public InGameUpgradeInfo(string id) { this.id = id;} 
        
    }
}