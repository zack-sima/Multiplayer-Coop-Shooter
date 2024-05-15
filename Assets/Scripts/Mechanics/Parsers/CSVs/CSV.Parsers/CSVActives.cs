using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using CSV;
using UnityEngine.Rendering;
using System.Linq;
using Fusion;

namespace CSV.Parsers {
    
    public static partial class CSVParserExtensions {
        public static bool TryParse(this Dictionary<string, InventoryInfo> dict, string csv, bool isDebug = false, string debugId = "") {
            var tempDict = dict;
            string[] rowArray = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            string[] masterHeaders = rowArray[0].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            rowArray[0] = "";
            int row = 0;

            InventoryInfo info = null;
            List<string> tempModifier = new();
            List<string> tempHeaders = new(); 
            List<string> tempUpgradeHeaders = new();

            bool hasAdd = false;

            if (isDebug) DebugUIManager.instance.LogOutput($"/*=====| {debugId} CSV RESET INIT |=====*/\n");

            foreach(string s in rowArray) {
                row++;
                string[] columnArray= s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (isDebug) { DebugUIManager.instance.LogOutput(debugId + " CSV Progress : ", ((float)row)/ ((float)rowArray.Length)); }

                if (columnArray.Length < 2) continue;

                //Try Get the [stringId] of the item.
                { if (columnArray[0].Contains('[') && TryParseModi(columnArray[0], row, out string id)) {
                    if (id == nameof(CSVMd.StringId)) {

                        if (columnArray.Length < 2) {
                            string error = "ActiveId : Error with array size : @ row : " + row + " : ColumnArray length is less than 2.";
                            LogError(error, debugId); return false;
                        }

                        //Try Get actual stringId
                        if (!TryGetString(columnArray, 1, out string stringId)) { 
                            string error = "ActiveId : Error with stringId extraction : " + columnArray[1] + " @ row : " + row + " : Is ActiveId properly written?";
                            LogError(error, debugId); return false;
                        }

                        //Init the prior info.
                        if (info != null) {
                            if (hasAdd) PushToDict(tempDict, info.id, info);
                            hasAdd = false;
                        }

                        tempHeaders = new List<string>();
                        tempModifier = new List<string>();
                        tempUpgradeHeaders = new List<string>();

                        info = new InventoryInfo(stringId);
                        
                        //Init tempModifiers
                        for(int i = 2; i < columnArray.Length; i++) {
                            if (TryParseModi(columnArray[i], row, out string tempModiId)) {
                                if (tempModiId == nameof(CSVMd.Add)) {
                                    hasAdd = true;
                                }
                                tempModifier.Add(tempModiId);
                            } else {
                                string error = "ActiveId : Error with modifier extraction of : " + columnArray[i] + " @ row : " + row + " : Modi unable to parse [string] to string. Are Temp [Tags] properly written?";
                                LogError(error, debugId); return false;
                            }
                        }
                    } else if (id == nameof(CSVMd.Tags)) { // TempTags
                        for(int i = 1; i < columnArray.Length; i++) {
                            if (TryParseModi(columnArray[i], row, out string tempTag)) {
                                if (tempTag == nameof(CSVMd.Above)) {
                                    if (masterHeaders.Length > i && TryParseModi(masterHeaders[i], row, out string header)) 
                                        tempHeaders.Add(header);
                                    else {
                                        string error = "Tags: Error with master extraction of : " + masterHeaders[i] + " : @ row : " + row + " : Modi unable to parse [string] to string. Are Master [Tags] properly written?";
                                        LogError(error, debugId); return false;
                                    }
                                } else {
                                    if (TryParseModi(columnArray[i], row, out string header)) 
                                        tempHeaders.Add(header);
                                    else {
                                        string error = "Tags: Error with column extraction of : " + columnArray[i] + " : @ row : " + row + " : Modi unable to parse [string] to string. Are [Tags] properly written?";
                                        LogError(error, debugId); return false;
                                    }
                                }
                            } else {
                                string error = "Tags: Error with general extraction of : " + columnArray[i] + " : @ row : " + row + " : Modi unable to parse [string] to string. Are [Tags] properly written?";
                                LogError(error, debugId); return false;
                            }
                        }
                    } else if (id == nameof(CSVMd.UPTags)) {
                        tempUpgradeHeaders = new();
                        for(int i = 0; i < columnArray.Length; i++) {
                            if (TryParseModi(columnArray[i], row, out string tempTag)) {
                                tempUpgradeHeaders.Add(tempTag);
                            } else {
                                string error = "UPTags : Error with upgrade tag extraction of : " + columnArray[i] + " : @ row : " + row + " : Modi unable to parse [string] to string.";
                                LogError(error, debugId); return false;
                            }
                        }
                    } else if (id == nameof(CSVMd.IUpgrade)) {
                        //Try Get IUpgrades + bool checks 
                            //Read IUpgrades and init them on the inventory info.
                        if (info == null) {
                            string error = "IUpgrade : Error with null info : @ row : " + row + " : Info is null. Is the [ActiveId] properly written?";
                            LogError(error, debugId); return false;
                        }

                        if (!(columnArray.Length > 1)) {
                            string error = "IUpgrade : Error with array size : @ row : " + row + " : ColumnArray length is less than 1.";
                            LogError(error, debugId); return false;
                        }

                        InGameUpgradeInfo upgradeInfo = new InGameUpgradeInfo(columnArray[1]);
                        for(int i = 2; i < columnArray.Length; i++) {
                            if (double.TryParse(columnArray[i], out double modi)) {
                                if (tempUpgradeHeaders.Count > i) {
                                    upgradeInfo.PushModi(tempUpgradeHeaders[i], modi);
                                } else { LogError(debugId, "IUpgrade : Error with tempUpgradeHeaderCount : @ row " + row + " : TempUpgradeHeadersCount less than current column array size. Are the UPTags propely written?"); return false; }
                            } else {
                                string error = "IUpgrade : Error with column extraction of : " + columnArray[i] + " : @ row : " + row + " : Modi unable to parse to a double.";
                                LogError(error, debugId); return false;
                            }
                        }
                        info.PushInGameUpgrade(upgradeInfo);
                    }
                } else if (int.TryParse(columnArray[0], out int level)) {
                    for(int i = 1; i < columnArray.Length; i++) {
                        if (double.TryParse(columnArray[i], out double d)) {
                            if (tempHeaders.Count > i) {
                                info.PushInventoryModi(tempHeaders[i], d, level);
                            } else {
                                string error = "tempHeaders : Error with tempHeader.Count : @ row : " + row + " : TempHeaders length is less than i.";
                                LogError(error, debugId); return false;
                            }
                        } else {
                            if (tempHeaders.Count > i) {
                                if (tempHeaders[i] == nameof(CSVMd.Display)) {
                                    info.displayName = columnArray[i];
                                } else if (tempHeaders[i] == nameof(CSVMd.Description)) {
                                    info.description = columnArray[i];
                                } //else if () // TODO: Other interpretations of the data that are string must go here.
                            } else {
                                string error = "tempHeaders : Error with tempHeader count : @ row : " + row + " : TempHeaders length is less than i.";
                                LogError(error, debugId); return false;
                            }
                        }
                    }
                } }
            }
            if (info != null) {
                if (hasAdd) PushToDict(tempDict, info.id, info);
            }
                
            if (isDebug) DebugUIManager.instance.LogOutput($"/*=====| {debugId} CSV RESET DONE |=====*/\n");
            dict = tempDict;
            return true;
        }
        private static void LogWarning(string warning) {
            DebugUIManager.instance.LogOutput(warning);
            Debug.LogWarning(warning);
        }
        private static void LogError(string error, string debugId) {
            DebugUIManager.instance.LogOutput(debugId + " : " + error);
            Debug.LogError(debugId + " : " + error);
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
            var match = Regex.Match(input, @"\[([^\[\]]*)\]");
            if (match.Success) {
                return match.Groups[1].Value;
            } else {
                return null;
            }
        }
    }
    
    [System.Serializable]
    public class InventoryInfo {
        public readonly string id;
        public string displayName;
        public string description;
        private Dictionary<int, Dictionary<string, double>> modiByLevel = new();
        private Dictionary<string, InGameUpgradeInfo> inGameUpgrades = new();

        public InventoryInfo(string id) {
            this.id = id;
        }

        public int GetMaxLevel() {
            return modiByLevel.Keys.Max();
        }

        public void PushInventoryModi(string id, double input, int level) {
            if (!modiByLevel.ContainsKey(level)) modiByLevel.Add(level, new Dictionary<string, double>());
            if (!modiByLevel[level].ContainsKey(id)) modiByLevel[level].Add(id, input);
            else modiByLevel[level][id] = input;
        }

        public void PushInGameUpgrade(InGameUpgradeInfo input) {
            if (!inGameUpgrades.ContainsKey(input.id)) inGameUpgrades.Add(input.id, input);
            else inGameUpgrades[input.id] = input;
        }

        public bool TryGetModi(string id, int level, out double output) {
            output = 0;
            if (modiByLevel.ContainsKey(level) && modiByLevel[level].ContainsKey(id)) {
                output = modiByLevel[level][id];
                return true;
            }
            return false;
        }
        public override string ToString() {
            //Print all the modis and inGameUpgrades as well as all other info.
            string s = "ID : " + id + "\n";
            s += "DisplayName : " + displayName + "\n";
            s += "Description : " + description + "\n";
            s += "MaxLevel : " + GetMaxLevel() + "\n";

            foreach(var kvp in modiByLevel) {
                s += "Level : " + kvp.Key + "\n";
                foreach(var kvp2 in kvp.Value) {
                    s += "Modi : " + kvp2.Key + " : " + kvp2.Value + "\n";
                }
                s += "\n";
            }
            s += "\n";
            return s;
        }
    }

    [System.Serializable]
    public class InGameUpgradeInfo {
        public readonly string id;
        public List<string> softRequirements = new(), hardRequirements = new(), mutualRequirements = new();
        public Dictionary<string, double> modi = new();
        public int maxLevel = 1;
        public InGameUpgradeInfo(string id) { this.id = id;} 

        public bool TryGetModi(string id, out double output) {
            output = 0;
            if (modi.ContainsKey(id)) {
                output = modi[id];
                return true;
            }
            return false;
        }

        public void PushModi(string id, double input) {
            if (!modi.ContainsKey(id)) modi.Add(id, input);
            else modi[id] = input;
        }

        public override string ToString() {
            //return all the modis and requirements.
            string s = "ID : " + id + "\n";
            foreach(var kvp in modi) {
                s += "Modi : " + kvp.Key + " : " + kvp.Value + "\n";
            }
            s += "\n";
            return s;
        }

    }
}