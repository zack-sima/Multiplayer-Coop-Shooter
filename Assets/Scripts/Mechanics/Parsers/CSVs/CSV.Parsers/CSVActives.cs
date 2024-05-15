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

            string[] rowArray = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            string[] masterHeaders = rowArray[0].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            rowArray[0] = "";
            int row = -1;

            InventoryInfo info;
            List<string> tempModifier, tempHeaders;

            bool hasStringId = false;

            foreach(string s in rowArray) {
                row++;
                string[] columnArray= s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (columnArray.Length < 1) continue;

                //Try Get the [stringId] of the item.
                { if (!hasStringId && TryParseModi(columnArray[0], row, out string id)) {
                    if (id == nameof(CSVMd.ActiveId)) {

                        //Try Get actual stringId
                        if (!TryGetString(columnArray, 1, out string stringId)) { continue; }

                        hasStringId = true;
                        tempHeaders = tempModifier = new();
                        info = new InventoryInfo(stringId);
                        
                        //Init tempModifiers
                        for(int i = 2; i < columnArray.Length; i++) {
                            //This is for the tempTags bruh
                            // if (TryParseModi(secondCut[i], out string modiId)) {
                            //     if (modiId == nameof(CSVMd.Above) && masterHeaders.Length > i)
                            //         tempModifier.Add(masterHeaders[i]);
                            // } else tempModifier.Add(modiId);
                        }


                    }
                }}

                //Try Get TempTags + bool checks
                    //Init tempTags

                //Try Get Levels + bool checks 
                    //Read levels and init modifiers.

                //Try Get IUpgrades + bool checks 
                    //Read IUpgrades and init them on the inventory info.

                //Push to the main dict.

            }
            return true;
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
        private static bool TryParseModi(string input, int row, out string id) {
            id = "";
            string s = ExtractStringId(input)?.Replace(" ", "");
            if (s == null) { DebugUIManager.instance.LogOutput("Error with extracting Modifier " + input + " @ row : " + row); return false; }
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
        public string displayName;
        public string description;
        public int maxLevel;
        private Dictionary<(string, int), double> modiByLevel = new();
        private Dictionary<string, InGameUpgradeInfo> inGameUpgrades = new();

        public InventoryInfo(string id) {
            this.id = id;
        }

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

        public bool TryGetModi(string id, int level, out double output) {
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